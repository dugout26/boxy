#!/bin/bash
# App Store 스크린샷 자동화 — Apple 요구 디바이스 사이즈로 자동 캡처.
# 8장 권장: 메인메뉴, 레벨선택, 게임플레이(빈), 게임플레이(절반), 게임플레이(클리어), 별3개, 결과화면, Onboarding.
#
# 사용 전 준비:
#   - Builds/ios-simulator-build/ 빌드 완료 (BuildSimulatorIOS 실행)
#   - 각 단계별 스크린샷은 simctl으로 자동 캡처
#
# Apple 요구 디바이스 (App Store 등록):
#   - 6.9" (iPhone 17 Pro Max): 1290×2796
#   - 13" iPad: 2048×2732 또는 2064×2752
#
# 본 스크립트는 첫 화면(MainMenu) 자동 캡처만 — LevelSelect/Gameplay 캡처는
# 정수가 시뮬레이터에서 직접 PLAY 클릭 후 simctl io ... screenshot 호출.

set -e

DEST="${SCREENSHOT_DEST:-marketing/screenshots}"
mkdir -p "$DEST"

# Apple 요구 사이즈에 맞는 디바이스
DEVICES=(
  "iPhone 17 Pro Max|3A53E048-03EF-4788-97BE-C4864E349801|1290x2796"
  "iPad mini|7F81E9CE-E7C1-41A5-921B-CA4BEB2CD061|1488x2266"
)

APP=$(find /Users/jeki/Boxy/Builds/ios-simulator-build -name "*.app" -type d -path "*iphonesimulator*" -not -path "*PlugIns*" -not -path "*Frameworks*" 2>/dev/null | head -1)
if [ -z "$APP" ]; then
    echo "❌ Simulator .app 없음. 먼저 빌드: BoxyBuilder.BuildSimulatorIOS"
    exit 1
fi
BUNDLE_ID=$(plutil -extract CFBundleIdentifier raw "$APP/Info.plist")

for entry in "${DEVICES[@]}"; do
    NAME="${entry%%|*}"
    REST="${entry#*|}"
    UDID="${REST%%|*}"
    SIZE="${REST##*|}"

    echo "📱 $NAME ($SIZE)"
    STATE=$(xcrun simctl list devices | grep "$UDID" | grep -oE "Booted|Shutdown" || echo "Shutdown")
    if [ "$STATE" != "Booted" ]; then
        xcrun simctl boot "$UDID" 2>/dev/null || true
        sleep 3
    fi
    xcrun simctl uninstall "$UDID" "$BUNDLE_ID" 2>/dev/null || true
    xcrun simctl install "$UDID" "$APP"
    xcrun simctl launch "$UDID" "$BUNDLE_ID"
    sleep 8

    # 메인 메뉴 스크린샷
    SHOT="$DEST/01-mainmenu-${NAME// /_}.png"
    xcrun simctl io "$UDID" screenshot "$SHOT"
    echo "  ✅ $SHOT"
done

echo ""
echo "📸 자동 캡처 완료 — 메인 메뉴 1장씩."
echo "   레벨 선택/게임플레이/결과 등 추가 화면은 정수가:"
echo "     1. open -a Simulator"
echo "     2. PLAY 버튼 클릭 → 레벨 선택 진입 후"
echo "     3. xcrun simctl io <UDID> screenshot $DEST/02-levelselect.png"
echo "     4. 반복"
echo ""
echo "App Store Connect 업로드 8장 권장 화면:"
echo "  01. MainMenu (자동 캡처됨)"
echo "  02. LevelSelect (테마 탭 + 레벨 그리드)"
echo "  03. Gameplay 빈 그리드 + 아이템"
echo "  04. Gameplay 절반 채워진 상태"
echo "  05. Gameplay 거의 다 됨 ('almost there' 순간)"
echo "  06. Result 별 3개"
echo "  07. Onboarding 환영"
echo "  08. Settings 또는 AdReward"
