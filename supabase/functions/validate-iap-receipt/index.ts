// Boxy IAP 영수증 서버 검증
// CLAUDE.md §11-4 P0-1: 클라 직검증보다 안전한 자체 서버 검증.
//
// 클라이언트 호출:
//   POST /validate-iap-receipt
//   Body: { platform: "apple"|"google", receipt: "<base64 또는 JSON>", productId: string, transactionId: string, userId?: string }
//
// 응답:
//   200 { valid: true, transaction_id: string }   // 새 검증 성공 또는 이미 기록된 valid 트랜잭션
//   200 { valid: true, duplicate: true }          // 중복 (같은 transaction_id 두 번째 호출 — 차단)
//   200 { valid: false, reason: string }          // 검증 실패
//
// 보안:
//   - APPLE_SHARED_SECRET / GOOGLE_PLAY_SERVICE_ACCOUNT_JSON 환경변수로 보관 (Edge Function secret)
//   - transactions 테이블 (platform, transaction_id) UNIQUE → 중복 지급 자동 차단
//   - service_role만 transactions 쓰기 (RLS)

import { createClient } from "https://esm.sh/@supabase/supabase-js@2";
import { serve } from "https://deno.land/std@0.224.0/http/server.ts";

const supabase = createClient(
    Deno.env.get("SUPABASE_URL")!,
    Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")!,
);

const APPLE_SHARED_SECRET = Deno.env.get("APPLE_SHARED_SECRET") ?? "";
const APPLE_USE_SANDBOX = (Deno.env.get("APPLE_USE_SANDBOX") ?? "true") === "true";

serve(async (req) => {
    if (req.method !== "POST") return json({ error: "POST only" }, 405);

    let body: ValidateRequest;
    try {
        body = await req.json();
    } catch {
        return json({ error: "invalid json" }, 400);
    }

    if (!body.platform || !body.receipt || !body.productId || !body.transactionId) {
        return json({ error: "missing fields" }, 400);
    }

    // 이미 기록됐으면 중복 — 차단
    const { data: existing } = await supabase
        .from("transactions")
        .select("receipt_status")
        .eq("platform", body.platform)
        .eq("transaction_id", body.transactionId)
        .maybeSingle();

    if (existing) {
        return json({
            valid: existing.receipt_status === "valid",
            duplicate: true,
            transaction_id: body.transactionId,
        });
    }

    // 플랫폼별 검증
    let result: { valid: boolean; reason?: string; rawResponse?: unknown };
    if (body.platform === "apple") {
        result = await verifyApple(body.receipt);
    } else if (body.platform === "google") {
        // Google은 서비스 계정 OAuth + androidpublisher API 호출 필요. v1.0은 클라 RSA 검증으로 우선 보강 + 서버는 transaction_id 기록만.
        // v1.1에서 자체 서버 OAuth 구현 추가.
        result = { valid: true, reason: "google: client RSA verification trusted (v1.1 server validation pending)" };
    } else {
        return json({ error: "unsupported platform" }, 400);
    }

    // INSERT — UNIQUE 제약 (이미 위에서 중복 체크했지만 race condition 대비)
    const { error: insertErr } = await supabase.from("transactions").insert({
        transaction_id: body.transactionId,
        platform: body.platform,
        product_id: body.productId,
        user_id: body.userId ?? null,
        receipt_status: result.valid ? "valid" : "invalid",
        raw_response: result.rawResponse ?? { reason: result.reason ?? "" },
    });

    if (insertErr) {
        console.error("[validate-iap-receipt] insert error:", insertErr);
        // unique violation = race로 다른 요청이 먼저 INSERT — 중복으로 처리
        if (insertErr.code === "23505") {
            return json({ valid: result.valid, duplicate: true, transaction_id: body.transactionId });
        }
        return json({ error: insertErr.message }, 500);
    }

    return json({
        valid: result.valid,
        transaction_id: body.transactionId,
        reason: result.reason,
    });
});

async function verifyApple(receiptBase64: string): Promise<{ valid: boolean; reason?: string; rawResponse?: unknown }> {
    const sandboxUrl = "https://sandbox.itunes.apple.com/verifyReceipt";
    const productionUrl = "https://buy.itunes.apple.com/verifyReceipt";
    const startUrl = APPLE_USE_SANDBOX ? sandboxUrl : productionUrl;

    let response = await fetch(startUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            "receipt-data": receiptBase64,
            password: APPLE_SHARED_SECRET,
        }),
    }).then((r) => r.json());

    // 21007 = sandbox→production 재시도, 21008 = production→sandbox 재시도
    if (response.status === 21007) {
        response = await fetch(sandboxUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ "receipt-data": receiptBase64, password: APPLE_SHARED_SECRET }),
        }).then((r) => r.json());
    } else if (response.status === 21008) {
        response = await fetch(productionUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ "receipt-data": receiptBase64, password: APPLE_SHARED_SECRET }),
        }).then((r) => r.json());
    }

    if (response.status === 0) {
        return { valid: true, rawResponse: response };
    }

    return {
        valid: false,
        reason: `apple status ${response.status}`,
        rawResponse: response,
    };
}

interface ValidateRequest {
    platform: "apple" | "google";
    receipt: string;
    productId: string;
    transactionId: string;
    userId?: string;
}

function json(body: unknown, status = 200): Response {
    return new Response(JSON.stringify(body), {
        status,
        headers: { "Content-Type": "application/json" },
    });
}
