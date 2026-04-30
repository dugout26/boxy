// Boxy 보상형 광고 SSV (Server-Side Verification) endpoint
// CLAUDE.md §11-4 P0: 보상형 광고 우회 자동화 차단.
//
// 흐름:
//   1) AppLovin / AdMob 대시보드에서 SSV URL 등록 → 보상 발생 시 이 endpoint로 GET 요청
//      AppLovin: ?event_id={EVENT_ID}&user_id={CUSTOM_DATA}&amount={REWARD_AMOUNT}&currency={REWARD_NAME}
//      AdMob:    ?ad_network=...&ad_unit=...&reward_amount=...&reward_item=...&user_id=...&custom_data=...&signature=...
//   2) 클라가 미리 발급한 nonce(custom_data 필드)를 ad_nonces 테이블에 INSERT (UNIQUE 제약)
//   3) 동일 nonce 재시도 시 INSERT 실패 → 중복 지급 차단
//   4) 클라는 광고 종료 후 별도 endpoint(GET /reward-ad-ssv?nonce=X)로 granted 여부 polling
//
// **본 endpoint는 Edge Function 표준 패턴이며, 광고 네트워크 SSV 서명 검증은 단순화함.**
// AppLovin은 SSV 서명 미지원 (URL params + IP whitelist만), AdMob은 RSA 검증 — 추후 IF AdMob 채택 시 보강.

import { createClient } from "https://esm.sh/@supabase/supabase-js@2";
import { serve } from "https://deno.land/std@0.224.0/http/server.ts";

const supabase = createClient(
    Deno.env.get("SUPABASE_URL")!,
    Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")!,
);

serve(async (req) => {
    const url = new URL(req.url);

    if (req.method === "GET" && url.searchParams.has("poll")) {
        // 클라이언트 polling: nonce 보상 지급 여부 확인
        const nonce = url.searchParams.get("nonce");
        if (!nonce) return json({ error: "nonce required" }, 400);
        const { data, error } = await supabase
            .from("ad_nonces")
            .select("granted, placement")
            .eq("nonce", nonce)
            .maybeSingle();
        if (error) return json({ error: error.message }, 500);
        if (!data) return json({ granted: false, found: false });
        return json({ granted: data.granted, placement: data.placement, found: true });
    }

    // 광고 네트워크 SSV 콜백 (또는 클라이언트 직접 register)
    // GET 표준
    const nonce = url.searchParams.get("nonce") ?? url.searchParams.get("custom_data");
    const userId = url.searchParams.get("user_id") ?? "";
    const placement = url.searchParams.get("placement") ?? "unknown";
    if (!nonce) return json({ error: "nonce missing" }, 400);

    // INSERT — 중복 nonce는 unique 제약으로 실패. 정상.
    const { data, error } = await supabase
        .from("ad_nonces")
        .upsert(
            { nonce, user_id: userId, placement, granted: true, used_at: new Date().toISOString() },
            { onConflict: "nonce", ignoreDuplicates: false }
        )
        .select()
        .maybeSingle();

    if (error) {
        console.error("[reward-ad-ssv]", error);
        return json({ error: error.message }, 500);
    }

    return json({ granted: true, nonce });
});

function json(body: unknown, status = 200): Response {
    return new Response(JSON.stringify(body), {
        status,
        headers: { "Content-Type": "application/json" },
    });
}
