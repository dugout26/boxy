#!/bin/bash
# Boxy iOS Simulator 빌드 + 설치 + 실행 + 스크린샷
#
# 1. Unity로 Simulator SDK 빌드 (Builds/ios-simulator/Unity-iPhone.xcodeproj 생성)
# 2. xcodebuild로 .app 컴파일 (서명 불필요, 시뮬레이터는 인증서 안 씀)
# 3. iPhone Simulator boot
# 4. 앱 install + launch
# 5. 5초 후 스크린샷 → /tmp/boxy-sim-{timestamp}.png
#
# 사용: ./scripts/ios-simulator-test.sh [iPhoneModel]
#   기본: iPhone 16

set -euo pipefail

DEVICE_NAME="${1:-iPhone 16}"
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$PROJECT_ROOT"

UNITY="/Applications/Unity/Hub/Editor/6000.3.14f1/Unity.app/Contents/MacOS/Unity"

echo "🔨 Unity Simulator 빌드"
"$UNITY" -batchmode -nographics \
    -projectPath "$PROJECT_ROOT" \
    -buildTarget iOS \
    -executeMethod Boxy.Editor.BoxyBuilder.BuildSimulatorIOS \
    -logFile /tmp/boxy-sim-unity.log \
    -quit

XCODE_PROJ="Builds/ios-simulator/Unity-iPhone.xcodeproj"
if [ ! -d "$XCODE_PROJ" ]; then
    echo "❌ Unity 빌드 실패 — /tmp/boxy-sim-unity.log 확인"
    exit 1
fi

echo "🔧 pbxproj 서명 패치 (Simulator는 서명 X)"
PBXPROJ="$XCODE_PROJ/project.pbxproj"
sed -i '' \
    -e 's/"CODE_SIGN_IDENTITY\[sdk=iphoneos\*\]" = "iPhone Developer";/"CODE_SIGN_IDENTITY[sdk=iphoneos*]" = "";/g' \
    -e 's/CODE_SIGN_IDENTITY = "iPhone Developer";/CODE_SIGN_IDENTITY = "";/g' \
    "$PBXPROJ"

echo "📱 Simulator UDID 검색 ($DEVICE_NAME)"
DEVICE_ID=$(xcrun simctl list devices "iOS" | grep "$DEVICE_NAME (" | head -1 | grep -oE "[A-F0-9-]{36}" || true)
if [ -z "$DEVICE_ID" ]; then
    echo "❌ '$DEVICE_NAME' 시뮬레이터 없음. 사용 가능 디바이스:"
    xcrun simctl list devices "iOS" | grep -E "iPhone|iPad" | head -20
    exit 1
fi
echo "Device UDID: $DEVICE_ID"

echo "🏗  xcodebuild Simulator (generic destination, 서명 X)"
DERIVED="Builds/ios-simulator-build"
rm -rf "$DERIVED"
xcodebuild build \
    -project "$XCODE_PROJ" \
    -scheme Unity-iPhone \
    -configuration Debug \
    -sdk iphonesimulator \
    -destination "generic/platform=iOS Simulator" \
    -derivedDataPath "$DERIVED" \
    CODE_SIGNING_ALLOWED=NO \
    CODE_SIGNING_REQUIRED=NO \
    CODE_SIGN_IDENTITY="" \
    PROVISIONING_PROFILE="" \
    PROVISIONING_PROFILE_SPECIFIER="" \
    ONLY_ACTIVE_ARCH=NO \
    2>&1 | tail -10

APP=$(find "$DERIVED" -name "*.app" -type d -path "*iphonesimulator*" -not -path "*PlugIns*" -not -path "*Frameworks*" | head -1)
if [ -z "$APP" ]; then
    echo "❌ .app 못 찾음 in $DERIVED"
    exit 1
fi
echo "✅ App built: $APP"

echo "📱 Simulator boot ($DEVICE_NAME, $DEVICE_ID)"

# Boot if not booted
STATE=$(xcrun simctl list devices | grep "$DEVICE_ID" | grep -oE "Booted|Shutdown" || echo "Shutdown")
if [ "$STATE" != "Booted" ]; then
    xcrun simctl boot "$DEVICE_ID"
    open -a Simulator --args -CurrentDeviceUDID "$DEVICE_ID"
    sleep 3
fi

BUNDLE_ID=$(plutil -extract CFBundleIdentifier raw "$APP/Info.plist")
echo "📲 Install + launch ($BUNDLE_ID)"
xcrun simctl uninstall "$DEVICE_ID" "$BUNDLE_ID" 2>/dev/null || true
xcrun simctl install "$DEVICE_ID" "$APP"
xcrun simctl launch "$DEVICE_ID" "$BUNDLE_ID"

# 화면 안정화 + Unity 스플래시 통과 대기
sleep 8

TS=$(date +%H%M%S)
SHOT="/tmp/boxy-sim-$TS.png"
xcrun simctl io "$DEVICE_ID" screenshot "$SHOT"

echo ""
echo "✅ 스크린샷: $SHOT"
echo ""
echo "검증 추가 스크린샷:"
echo "  xcrun simctl io $DEVICE_ID screenshot /tmp/boxy-sim-NEXT.png"
echo "  앱 종료: xcrun simctl terminate $DEVICE_ID com.mound.boxy.dev"
