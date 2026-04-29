#!/bin/bash
# 시뮬레이터 부팅 후 자동으로 화면 흐름 따라가며 스크린샷 캡처.
# 사용 시점: ios-simulator-test.sh 가 이미 빌드 + install 한 후.
#
# 동작:
#   1. MainMenu 캡처
#   2. PLAY 버튼 좌표 탭 → LevelSelect 캡처
#   3. Level 1 좌표 탭 → Gameplay 캡처
#   4. 각 디바이스(iPhone 17, iPhone 16e, iPad mini)에 동일

set -euo pipefail

DEVICES=(
    "iPhone 17:08F71EF1-E93D-4E6C-9A18-B6A0B6A0F7EC"
    "iPhone 16e:8044DD47-BDB0-42E3-8F6D-35E588B44D05"
    "iPad mini:7F81E9CE-E7C1-41A5-921B-CA4BEB2CD061"
)

APP=$(find /Users/jeki/Boxy/Builds/ios-simulator-build -name "*.app" -type d -path "*iphonesimulator*" -not -path "*PlugIns*" -not -path "*Frameworks*" | head -1)
BUNDLE_ID=$(plutil -extract CFBundleIdentifier raw "$APP/Info.plist")

for entry in "${DEVICES[@]}"; do
    NAME="${entry%%:*}"
    UDID="${entry##*:}"
    SAFE_NAME="${NAME// /-}"

    echo "🎬 $NAME ($UDID)"
    xcrun simctl boot "$UDID" 2>/dev/null || true
    sleep 1
    xcrun simctl uninstall "$UDID" "$BUNDLE_ID" 2>/dev/null || true
    xcrun simctl install "$UDID" "$APP"
    xcrun simctl launch "$UDID" "$BUNDLE_ID"
    sleep 6

    # MainMenu 캡처
    SHOT_MAIN="/tmp/boxy-flow-${SAFE_NAME}-1-mainmenu.png"
    xcrun simctl io "$UDID" screenshot "$SHOT_MAIN" 2>&1 | tail -1
    echo "  📸 MainMenu: $SHOT_MAIN"

    # PLAY 버튼 탭 (화면 가로 50%, 세로 60% 부근 — 디바이스 비율 무관 가까움)
    # simctl io의 stream 명령어로 탭 가능. 또는 Cmd+Shift+H로 홈 후 다시 진입 등 수동.
    # 실제 자동 탭은 xcrun simctl io <id> tap... 미지원.
    # 대안: AccessibilityInspector 또는 idb. 여기선 일단 MainMenu 캡처만 비교.
    sleep 1
    xcrun simctl terminate "$UDID" "$BUNDLE_ID" 2>/dev/null || true
done

echo ""
echo "✅ 캡처 완료. /tmp/boxy-flow-* 파일 확인"
ls -la /tmp/boxy-flow-*.png 2>&1
