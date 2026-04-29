# 2026-04-29 — Boxy 상표 검색

## Context
Boxy 네이밍 사용 전 한국(KIPRIS) / 미국(USPTO) 상표 충돌 검증. 9류(소프트웨어) + 41류(엔터테인먼트) 게임 분류.

## Status
**🟡 부분 검증 — AI 일반 웹 조사 + 정수 KIPRIS/USPTO 공식 검색 1단계 필요**

### AI 사전 조사 (2026-04-29 진행)
- **USPTO TSDR / TMHunt / Justia 직접 접근**: 모두 **403** (자동화 차단)
- **공개 정보 기반 분석**:
  - "Boxy"는 영어 일반 단어 (= square, blocky 등) → 게임 외 카테고리에 존재 가능성 높음
  - 미국 시장 검색에서 "Boxy" 단어 자체로 직접 게임 카테고리 충돌 미확인 (광범위한 패션/액세서리/소프웨어 사용은 있음)
  - **이미 사용 중**: 다양한 "Boxy" 브랜드 (의류, 가구, 사료 박스 구독 등) — 게임 카테고리 외엔 무관
- **App Store 자체 검색** (별도 확인 필요):
  - 정수가 이미 App Store Connect에서 "Boxy" 이름 사용 불가능 발견 → **App Store 글로벌 충돌 확인됨** (구체 앱은 미확인)
  - 결과: "Boxy Pack"으로 결정 (decisions/2026-04-29-10-app-store-name.md)
- **결론**: 단어 단독 "Boxy"는 충돌 위험 높음. **"Boxy Pack" 명칭은 충돌 위험 낮음** (구체 검색 필요).

### 정수 1차 검증 (15분, 브라우저)

#### USPTO TESS
1. https://tmsearch.uspto.gov → **Combined Word Mark Search**
2. "Boxy Pack" 입력 → 결과 보기
3. 또한 "Boxy" 단독으로도 검색 → Class 9 / 28 / 41 필터 → 살아있는 게임 등록 확인
4. 결과 캡처 → 아래 결과 기록란 채움

#### KIPRIS
1. https://www.kipris.or.kr → 상표 탭
2. "Boxy" + "Boxy Pack" 둘 다 검색
3. 9류 / 41류 필터
4. 결과 기록

## 정수 직접 수행 절차

### A. KIPRIS 검색 (한국)
1. https://www.kipris.or.kr/ 접속
2. 상단 "상표" 탭 선택
3. 검색창에 `Boxy` 입력 → 검색
4. 결과에서 다음 필터 적용:
   - **분류**: 9류 (다운로드 가능 소프트웨어/게임)
   - **분류**: 41류 (게임 서비스/엔터테인먼트)
5. 등록/출원/거절 상태별 분류
6. 충돌 게임/소프트웨어 발견 시 출원인/등록일 기록

### B. USPTO TESS 검색 (미국)
1. https://tmsearch.uspto.gov/search/search-information 접속
2. "Word Mark Search" → `Boxy` 입력
3. 결과 필터:
   - International Class 9 (Computer software)
   - International Class 41 (Entertainment services)
   - International Class 28 (Games and playthings — 게임의 경우 종종 적용)
4. Live (살아있는) 등록 vs Dead 분류
5. 게임/소프트웨어 카테고리 충돌 기록

### C. 추가 점검
- App Store / Google Play에서 "Boxy" 검색 → **유사 게임명 발견 시 ASO 경쟁** 측정
- 도메인 boxy.app / boxy.io / boxy.game 사용 가능 여부

---

## 결과 기록

### KIPRIS 결과 (AI 직접 검증 완료 ✅, 2026-04-29)
- 방법: `https://www.kipris.or.kr/khome/search/searchResult.do?queryText=<X>&tab=trademark` 직접 호출
- **"Boxy" 검색**: **총 0건** (한국 상표 등록·출원 0)
- **"Boxy Pack" 검색**: **총 0건**
- **"박시" (한글 음역) 검색**: **총 0건**
- 결론: **한국 시장 상표 충돌 위험 매우 낮음.** 9/41류 분류 필터 적용해도 결과 없음(전체 0건이므로).
- 정수 추가 확인 권장: KIPRIS 로그인 후 "유사상표 검색" 기능 (음운/시각적 유사) — 5분 작업

### USPTO 결과 (AI 부분 검증, SPA + AWS WAF로 자동화 직접 차단)
- AI 검색일: 2026-04-29
- 방법: tmsearch.uspto.gov, tsdr.uspto.gov, uspto.report, trademarkia.com, trademark.justia.com, EUIPO TMview 모두 시도 — SPA + WAF 또는 403 차단
- **간접 검증 결과** (web 인덱스 + 외부 DB 노출분):
  - **"BOXY BOO"** — Mob Entertainment Inc. (Poppy Playtime 스튜디오) 소유, Serial 97585622, **Class 28 (toys/sporting goods)** 등록 2024-05-21. **다른 마크지만 같은 회사가 광범위 IP 보호 (152개 상표)** — 주의 필요 사항이지만 "Boxy Pack"과 직접 충돌 X (다른 단어 조합 + 다른 클래스).
  - **"BOXY" 단독 마크**: web 인덱스에 명시적 매칭 없음 (단, 차단 때문에 USPTO 직접 DB 검증은 못 함)
  - **"BOXY PACK"**: 인덱스 0건
- AI 결론: **한국 충돌 0 + Boxy Pack 인덱스 0 + Mob Entertainment는 다른 마크/다른 클래스** → 출시 진행 가능. 다만 출시 후 6개월 내 **정식 출원 권장** (한국 9/41류 + 미국 9/28/41류 동시).
- 정수 추가 확인 (10분):
  1. https://tmsearch.uspto.gov 직접 접속 → "BOXY PACK" 입력 → Live 필터
  2. Class 9 / 28 / 41 필터 → 결과 0건 확인
  3. "BOXY" 단독으로도 동일 검색 → 게임 카테고리 충돌 직접 눈으로 확인

### 스토어 검색 (AI 검증 완료, 2026-04-29)

**App Store "Boxy" Top 결과:**
1. **Boxy** (id 1604740893) — Qatar Post / 가상 PO Box 유틸리티 (게임 X) — 카테고리 충돌 없음
2. **Boxy SVG** (id 611658502) — 벡터 그래픽 편집기 — 카테고리 충돌 없음
3. **Boxy Strike Battle Simulator** (id 1244564757) — 전투 시뮬 게임 (Strategy) — 같은 게임이지만 장르 다름
4. **Brain Games - Brainy Boxy** (id 6757749388, Guru Future Holding, 2026-04 출시) — **퍼즐 게임 (브레인)** — 같은 카테고리, 메커닉 다름 (수수께끼 vs 패킹)
5. Boxy: Moving & Storage / Boxy for YouTube — 유틸리티

**Google Play "Boxy" Top 결과:**
1. **Boxy** (com.kxland.boxy) — **미로 퍼즐 게임** — **장르 직접 충돌 위험 (퍼즐)**
2. **Brainy Boxy** — App Store와 동일

**"Boxy Pack" 정확 일치 검색:**
- App Store: 0건
- Google Play: 0건
- AI 결론: **"Boxy Pack" 안전** (decisions/2026-04-29-10 결정 사실상 검증됨)

### 도메인 (AI 검증 완료, 2026-04-29)

**원래 후보 (모두 사용 불가):**
- boxy.app: ❌ Spaceship.com 매물 (**$45,000** 또는 6개월 임대 $6,833/월) — 예산 초과
- boxy.io: ❌ 등록 (Namecheap, 2018-02 이후) — 현재 미서비스 (HTTP 응답 없음)
- boxygame.com: ❌ 등록 (Squarespace, 2023-08 이후) — placeholder ("Boxy" h1 1줄만)

**대안 후보 (AI 검증):**
- ✅ **boxypack.app** — 사용 가능 (가장 권장: 게임명 + .app TLD 매칭)
- ✅ **boxypackgame.com** — 사용 가능
- ✅ **mound.studio** — 사용 가능 (Mound 브랜드 메인 사이트로 적합)
- ✅ **mound.games** — 사용 가능 (Mound 시리즈 게임 허브용)
- ❌ boxypack.com / getboxy.com / playboxy.com — 등록됨

**권장 조합:**
- **mound.studio** (Mound 스튜디오 메인) + **boxypack.app** (Boxy Pack 게임 별도 랜딩)
- 또는 **mound.studio/boxy-pack** 서브패스로 통합 (단일 도메인 비용 절감)

---

## Decision (충돌 시 대안)

**충돌 없음** → "Boxy" 그대로 진행.

**약간 충돌** (다른 카테고리 또는 dead 상태):
- 그대로 진행 + 출시 후 한국 저작권위원회 캐릭터 디자인 등록 (5만원)으로 보강

**직접 충돌** (살아있는 게임 카테고리 등록 발견):
- 즉시 대안:
  - **Boxie** (이미 안 쓰이는 다른 변형이라면)
  - **Boksy** (한국식 변형)
  - **Pacxy** (Pack + Boxy 합성)
  - **Boxylo** / **Boxyfy**
  - 새 후보 검색 시 위 절차 동일 반복

**최종 결정 마감**: Day 1 22:00 강제 결정.

## Consequences (검증 안 하면)
- 출시 후 상표 침해 클레임 → 스토어 리스팅 강제 삭제
- Boxy 시리즈 IP화 전체 무효화
- 후속 게임 (Boxy Sort, Boxy Cafe) 전부 영향

## Revisit when
- 충돌 발견 시 즉시 대안 검색 반복
- 6개월 후 신규 출원 모니터링 (Boxy 시리즈 인지도 누적 시 보호 필요)

---

**우선순위**: Day 1 가장 먼저 수행. 다른 모든 작업(Unity 설치, 계정 생성)은 "Boxy" 네이밍 확정 후 진행 권장. 충돌 발견 시 Bundle ID/계정명 모두 새 이름으로 결정해야 함.
