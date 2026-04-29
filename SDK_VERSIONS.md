# Boxy — SDK 버전 메모

> CLAUDE.md §2-4 환각 방지의 기준점.
> 모든 외부 SDK 호출 코드는 이 문서에 박힌 버전과 호환되어야 함.

**Last updated**: 2026-04-28 (외부 정보 수집 결과 반영)

---

## Unity Editor
- **Unity 6.3 LTS — `6000.3.14f1`** (Day 0 시점 Hub 최신 LTS)
- 지원 종료: 2027-12 (Unity 6.0 LTS는 2026-10이므로 6.3을 선택)
- 다운로드: Unity Hub CLI 또는 Hub → Installs → Add

## 광고
- **Google Mobile Ads Unity Plugin v11.0.0** (2025-02-25)
  - Android SDK 25.0.0 / iOS SDK 13.0.0 의존
  - GitHub: https://github.com/googleads/googleads-mobile-unity/releases
- **AppLovin MAX Unity Plugin v8.6.2** (2025-03-30)
  - Android SDK 13.6.2 / iOS SDK 13.6.2 의존
  - GitHub: https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin/releases
- 통합 패키지: Mobile Monetization Pro V2 (Asset Store에서 최신 버전 확인 후 기입)

## Analytics / Attribution
- **Firebase Unity SDK v13.10.0**
  - GitHub: https://github.com/firebase/firebase-unity-sdk/releases
  - Crashlytics 포함
- **GameAnalytics Unity** — 최신 안정 버전 (Asset Store)
- **AppsFlyer Unity SDK** — 최신 안정 버전 (https://github.com/AppsFlyerSDK/appsflyer-unity-plugin)

## IAP
- **Unity In-App Purchasing** — Package Manager의 Unity Gaming Services 그룹 최신 (com.unity.purchasing)

## UI / 햅틱 / i18n
- **Lofelt Nice Vibrations** — Asset Store 최신
- **Unity Localization Package** — Package Manager `com.unity.localization` 최신 안정

## 폰트
- **Pretendard** — https://github.com/orioncactus/pretendard/releases 최신
  - Variable 100~900, OFL 라이선스

---

## 갱신 절차
1. 패키지 업데이트 시 즉시 이 문서 갱신
2. 메이저 업데이트는 changelog 검토 후 적용 (CLAUDE.md §2-4 환각 방지)
3. 새 SDK 추가 시 인터페이스로 한 번 감쌈 (CLAUDE.md §5-3 SDK 격리)
4. SDK 교체는 CLAUDE.md §17 결정 기록 필수

---

## 시장 벤치마크 참고 (2025-2026 GameAnalytics)
- D1 retention: 평균 26~28%
- D7 retention: 평균 ~8% / 상위 25%는 7~8% 이상
- ARPDAU: 중간값 $0.02 / 상위 15% $0.10~0.14
- → Boxy 합격선: D1 30% / D7 10% / ARPDAU $0.10 (기획서 보정 권장)
