# 2026-04-30 — Supabase Edge Functions (P0-1 IAP 서버 검증 + 광고 SSV)

## Context
시니어 리뷰 의견 (Cozy 디자인 톤 채택과 동일 일자, 별도 보안 검토):
- 무료 게임이라도 **IAP + 핵심 보상형 광고가 있는 카테고리**는 v1.0부터 자체 서버 검증 필요
- Boxy는 IAP 3종 (₩3,900 / ₩2,500 / ₩6,600) + 광고 보상형 핵심 (Hint / Undo) → P0 영역
- 자체 서버 = Supabase Edge Function 2개 + DB 테이블 2개로 충분

## Decision
**Supabase boxy-mound 프로젝트 v1.0에 도입.** 자체 서버 호스팅 (Vercel / Cloudflare Workers) 대비 셋업 부담 최소.

## Project Info
- Name: `boxy-mound`
- Reference: `lckhgvndkvmpqgmcqaea`
- URL: `https://lckhgvndkvmpqgmcqaea.supabase.co`
- Org: dugout26.gm@gmail.com (oouvgxmxsdxllbqedetx)
- Region: Northeast Asia (Tokyo) — 한국 사용자 latency 최소
- Dashboard: https://supabase.com/dashboard/project/lckhgvndkvmpqgmcqaea

## Schema (migration 20260430000000_security_p0_schema.sql)

### transactions
- `(platform, transaction_id)` UNIQUE — IAP 중복 지급 차단의 핵심
- 컬럼: `transaction_id, platform (apple/google), product_id, user_id, receipt_status (valid/invalid/revoked), raw_response jsonb`
- RLS 활성 — service_role만 INSERT (Edge Function 통과)

### ad_nonces
- `nonce` PRIMARY KEY — 같은 nonce 두 번 사용 차단
- 컬럼: `nonce, user_id, placement, granted, created_at, used_at`
- RLS 활성

## Edge Functions

### reward-ad-ssv
- URL: `https://lckhgvndkvmpqgmcqaea.supabase.co/functions/v1/reward-ad-ssv`
- 동작:
  1. AppLovin/AdMob SSV 콜백 → nonce + granted=true INSERT (UPSERT, conflict ignore)
  2. 클라이언트 polling: GET ?poll=1&nonce=X → granted 여부 반환
- 흐름:
  - 클라가 nonce 생성 (`AdSsvClient.GenerateNonce()`)
  - AppLovin/AdMob ShowRewardedAd custom_data로 nonce 전달
  - 광고 네트워크 SSV 콜백이 reward-ad-ssv?nonce=X 호출
  - 광고 종료 후 클라가 `PollGrantedAsync(nonce)` → granted=true면 보상 지급

### validate-iap-receipt
- URL: `https://lckhgvndkvmpqgmcqaea.supabase.co/functions/v1/validate-iap-receipt`
- 동작:
  1. POST {platform, receipt, productId, transactionId, userId?}
  2. 이미 transactions 테이블에 있으면 duplicate=true 반환 (중복 지급 차단)
  3. Apple: verifyReceipt API 직검증 (sandbox/production 자동 전환)
  4. Google: 클라 RSA 검증 trust + transaction_id 기록 (v1.1 자체 OAuth 검증 추가)
  5. INSERT transactions (UNIQUE 제약으로 race condition 방어)

### 환경변수 (Edge Function secrets — 정수가 dashboard 또는 CLI로 설정)
```bash
supabase secrets set APPLE_SHARED_SECRET=<App Store Connect → My Apps → App-Specific Shared Secret>
supabase secrets set APPLE_USE_SANDBOX=true   # production 시 false
```

## Client Integration

### Mound.Monetization.CloudReceiptValidator
- Supabase Edge Function 호출 wrapper
- `validate-iap-receipt` POST + 응답 파싱
- BoxyBootstrap이 `useStubProviders=false + Supabase URL/anonKey 설정` 시 우선 활성

### Mound.Monetization.AdSsvClient
- `GenerateNonce()` + `PollGrantedAsync(nonce, ct)` 8초 polling
- 광고 종료 후 보상 지급 전 호출 — granted=true 만 통과

### EnvironmentConfig.SupabaseConfig (nested)
```csharp
public sealed class SupabaseConfig {
    string Url               // https://lckhgvndkvmpqgmcqaea.supabase.co
    string AnonKey           // RLS 보호 — 클라 노출 OK
    string ValidateIapUrl    // = Url + "/functions/v1/validate-iap-receipt"
    string AdSsvUrl          // = Url + "/functions/v1/reward-ad-ssv"
}
```

### BoxyBootstrap wire-up
- `useStubProviders=true` (dev/Editor) → NullReceiptValidator + AdSsv null
- `useStubProviders=false + Supabase 설정` → CloudReceiptValidator + AdSsvClient
- Cloud 미설정 시 → CompositeReceiptValidator (Apple/Google 클라 직검증) fallback + 경고 로그

## Consequences
좋은 것:
- IAP 영수증 우회 차단 (transactions UNIQUE → 같은 transaction_id 두 번째 호출 = duplicate=true)
- 광고 보상 우회 자동화 차단 (광고 네트워크 SSV 콜백 없으면 nonce 미존재 → granted=false)
- 자체 서버 호스팅 부담 X (Supabase 무료 tier 충분: 500MB DB + 2M Edge Function 호출/월)
- Tokyo region — 한국 사용자 latency ~30ms

나쁜 것 / 위험:
- Supabase 무료 tier 한도 (월 2M function 호출) — Boxy DAU 1만 + 인당 광고 5회 시 일 5만 = 월 150만 호출 → 한도 내. 안전.
- service_role key가 Edge Function env에 있음 — Supabase secrets 메커니즘 신뢰
- anon key 노출 — RLS로 보호되므로 OK. 단 anon key로 transactions/ad_nonces 직접 SELECT/INSERT 불가 (RLS)

## v1.0 출시 전 정수 액션
1. **Apple Shared Secret 발급** (App Store Connect → My Apps → App Information → App-Specific Shared Secret)
2. `supabase secrets set APPLE_SHARED_SECRET=<value>` 실행 (이미 PAT 있음)
3. **Google License Key** (Google Play Console → Monetization setup → Licensing) — `EnvironmentConfig.GoogleReceipt.licenseKeyBase64`에 입력
4. **EnvironmentConfig.asset 생성** (Unity Editor → Right-click → Create → Mound → Environment Config)
5. **Supabase anon key 입력**:
   ```
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imxja2hndm5ka3ZtcHFnbWNxYWVhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Nzc1MTU2ODEsImV4cCI6MjA5MzA5MTY4MX0.StOjJJWbVrKcHa4SfTsAJd_r5p_VGjcsuJWM2EyY09w
   ```
6. **AppLovin/AdMob SSV URL 등록**:
   - AppLovin: 대시보드 → Apps → Boxy → Configure → Server-Side Reward Callback URL = `https://lckhgvndkvmpqgmcqaea.supabase.co/functions/v1/reward-ad-ssv`
   - AdMob: AdMob 대시보드 → Apps → Boxy → Ad units → Rewarded → SSV settings → 동일 URL

## Revisit when
- 월 Edge Function 호출 2M 초과 → Pro plan 업그레이드 ($25/월) 또는 자체 서버 (Cloud Functions / Lambda) 이전
- Google IAP 자체 OAuth 검증 필요 시 — service account JWT + androidpublisher API (v1.1)
- 클라우드 세이브 추가 시 — `user_saves` 테이블 + Game Center / Google Play Games 연동 (v1.1)
