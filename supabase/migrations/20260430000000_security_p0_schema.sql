-- Boxy v1.0 보안 P0 스키마
-- CLAUDE.md §11-4 P0-1 (IAP receipt 검증) + 광고 보상 SSV nonce 중복 차단
-- 출시 전 1회 적용. 변경 시 새 migration 파일 추가 (replace 금지).

-------------------------------------------------------------------------------
-- transactions: IAP 영수증 검증 결과 + 중복 지급 차단
-------------------------------------------------------------------------------
create table if not exists public.transactions (
    id              bigserial primary key,
    transaction_id  text not null,                       -- Apple originalTransactionId / Google orderId 또는 purchaseToken
    platform        text not null check (platform in ('apple','google')),
    product_id      text not null,                       -- boxy_remove_ads / boxy_hint_bundle_10 / boxy_starter_pack
    user_id         text,                                -- 익명 device id 또는 player id (정수 정책)
    receipt_status  text not null check (receipt_status in ('valid','invalid','revoked')),
    raw_response    jsonb,                               -- 검증 raw response (디버깅 + 사후 추적)
    created_at      timestamptz not null default now(),
    -- 같은 (platform, transaction_id)는 한 번만 기록 — 중복 지급 차단의 핵심
    constraint transactions_platform_txid_uniq unique (platform, transaction_id)
);

create index if not exists transactions_user_idx on public.transactions(user_id);
create index if not exists transactions_product_idx on public.transactions(product_id);

-------------------------------------------------------------------------------
-- ad_nonces: 보상형 광고 SSV nonce — 한 번 사용 후 폐기
-------------------------------------------------------------------------------
create table if not exists public.ad_nonces (
    nonce       text primary key,                        -- AppLovin / AdMob에서 발급한 SSV nonce
    user_id     text,
    placement   text not null,                           -- RewardedHint / RewardedUndo / RewardedRetry
    granted     boolean not null default false,          -- 보상 지급 여부
    created_at  timestamptz not null default now(),
    used_at     timestamptz
);

create index if not exists ad_nonces_user_idx on public.ad_nonces(user_id);
create index if not exists ad_nonces_created_idx on public.ad_nonces(created_at desc);

-- 7일 이상 된 미사용 nonce 자동 정리 (운영 부하 절감)
-- pg_cron 활성 시: select cron.schedule('cleanup_old_nonces','0 3 * * *', $$delete from ad_nonces where created_at < now() - interval '7 days';$$);

-------------------------------------------------------------------------------
-- RLS 활성 — Edge Function의 service role만 쓰기, 클라이언트는 read X
-------------------------------------------------------------------------------
alter table public.transactions enable row level security;
alter table public.ad_nonces    enable row level security;

-- 클라이언트(anon/authenticated) 직접 접근 금지 — Edge Function (service_role)만 통과
-- service_role은 RLS bypass — 별도 policy 불필요
