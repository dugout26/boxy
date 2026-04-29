#!/bin/bash
# 4개 씬 자동 스크린샷 — first-scene 임시 변경 후 빌드 + 캡처.
# 결과: marketing/screenshots/{01-mainmenu,02-levelselect,03-gameplay,04-settings}-{device}.png

set -e
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$PROJECT_ROOT"
DEST="${SCREENSHOT_DEST:-$PROJECT_ROOT/marketing/screenshots}"
case "$DEST" in /*) ;; *) DEST="$PROJECT_ROOT/$DEST";; esac
mkdir -p "$DEST"
echo "🎯 Screenshot dest: $DEST"

UNITY="/Applications/Unity/Hub/Editor/6000.3.14f1/Unity.app/Contents/MacOS/Unity"

# Apple 권장 디바이스 (App Store 등록)
DEVICES=(
  "iPhone 17 Pro Max|3A53E048-03EF-4788-97BE-C4864E349801"
  "iPad mini|7F81E9CE-E7C1-41A5-921B-CA4BEB2CD061"
)

# 씬별 빌드 메서드
SCENES=(
  "01-mainmenu|Boxy.Editor.BoxyBuilder.BuildSimulatorIOS"
  "02-levelselect|Boxy.Editor.BoxyBuilder.BuildSimulatorIOS_LevelSelect"
  "03-gameplay|Boxy.Editor.BoxyBuilder.BuildSimulatorIOS_Gameplay"
  "04-settings|Boxy.Editor.BoxyBuilder.BuildSimulatorIOS_Settings"
)

for scene_entry in "${SCENES[@]}"; do
    LABEL="${scene_entry%|*}"
    METHOD="${scene_entry#*|}"
    echo ""
    echo "🎬 $LABEL — Unity build ($METHOD)"
    "$UNITY" -batchmode -nographics -projectPath "$(pwd)" -buildTarget iOS \
        -executeMethod "$METHOD" \
        -logFile "/tmp/boxy-shot-${LABEL}.log" -quit

    if [ ! -d "Builds/ios-simulator/Unity-iPhone.xcodeproj" ]; then
        echo "❌ $LABEL Unity 빌드 실패"; continue
    fi

    # pbxproj 패치
    sed -i '' \
        -e 's/"CODE_SIGN_IDENTITY\[sdk=iphoneos\*\]" = "iPhone Developer";/"CODE_SIGN_IDENTITY[sdk=iphoneos*]" = "";/g' \
        -e 's/CODE_SIGN_IDENTITY = "iPhone Developer";/CODE_SIGN_IDENTITY = "";/g' \
        Builds/ios-simulator/Unity-iPhone.xcodeproj/project.pbxproj

    # xcodebuild
    rm -rf Builds/ios-simulator-build
    xcodebuild build \
        -project Builds/ios-simulator/Unity-iPhone.xcodeproj \
        -scheme Unity-iPhone -configuration Debug -sdk iphonesimulator \
        -destination "generic/platform=iOS Simulator" \
        -derivedDataPath Builds/ios-simulator-build \
        CODE_SIGNING_ALLOWED=NO CODE_SIGN_IDENTITY="" \
        PROVISIONING_PROFILE="" PROVISIONING_PROFILE_SPECIFIER="" ONLY_ACTIVE_ARCH=NO 2>&1 | tail -2 | grep -E "BUILD SUCCEEDED|BUILD FAILED"

    APP=$(find Builds/ios-simulator-build -name "*.app" -type d -path "*iphonesimulator*" -not -path "*PlugIns*" -not -path "*Frameworks*" | head -1)
    [ -z "$APP" ] && { echo "❌ .app 못 찾음"; continue; }
    BUNDLE_ID=$(plutil -extract CFBundleIdentifier raw "$APP/Info.plist")

    # 디바이스별 install + capture
    for entry in "${DEVICES[@]}"; do
        NAME="${entry%|*}"
        UDID="${entry#*|}"
        STATE=$(xcrun simctl list devices | grep "$UDID" | grep -oE "Booted|Shutdown" || echo "Shutdown")
        if [ "$STATE" != "Booted" ]; then xcrun simctl boot "$UDID" 2>/dev/null || true; sleep 2; fi
        xcrun simctl uninstall "$UDID" "$BUNDLE_ID" 2>/dev/null || true
        xcrun simctl install "$UDID" "$APP" 2>&1 | head -3
        xcrun simctl launch "$UDID" "$BUNDLE_ID" 2>&1 | tail -1
        sleep 6
        SHOT="$DEST/${LABEL}-${NAME// /_}.png"
        xcrun simctl io "$UDID" screenshot "$SHOT" 2>&1 | tail -1
        echo "  📸 $SHOT"
    done
done

echo ""
echo "✅ 자동 스크린샷 완료 — $DEST"
ls "$DEST"
