#!/bin/bash
# Boxy iOS archive + TestFlight 업로드 자동화
#
# 사전 조건:
#   1. Unity로 Builds/ios-${ENV}/ Xcode 프로젝트 생성 완료
#      (Boxy.Editor.BoxyBuilder.BuildDevIOS / BuildStagingIOS / BuildProductionIOS)
#   2. App Store Connect API key 발급 (.p8 파일 + Key ID + Issuer ID)
#   3. Apple Developer Team ID 확인
#   4. App Store Connect에 해당 Bundle ID로 앱 레코드 생성
#
# 환경 변수 (필수):
#   ASC_KEY_ID        — App Store Connect API Key ID (예: ABC123DEF4)
#   ASC_ISSUER_ID     — Issuer ID (UUID 형식)
#   ASC_KEY_PATH      — .p8 파일 절대 경로 (예: ~/private_keys/AuthKey_ABC123DEF4.p8)
#   TEAM_ID           — Apple Developer Team ID (10자리)
#
# 옵션:
#   ENV=dev|prod  — 기본 dev (2-tier, decisions/2026-04-29-09-app-id-strategy-2tier.md)
#
# 사용:
#   export ASC_KEY_ID=...
#   export ASC_ISSUER_ID=...
#   export ASC_KEY_PATH=~/private_keys/AuthKey_XXX.p8
#   export TEAM_ID=...
#   ENV=dev ./scripts/ios-archive-upload.sh

set -euo pipefail

# 재발방지 (decisions/2026-04-29-15): 명시적 BOXY_ALLOW_UPLOAD=1 환경변수 없으면 업로드 차단.
# 정수가 의도적으로 업로드 명령 시에만 export BOXY_ALLOW_UPLOAD=1 한 후 실행.
if [ "${BOXY_ALLOW_UPLOAD:-}" != "1" ]; then
    echo "❌ TestFlight 업로드 차단됨 — 100% 완성 후 정수 명시적 승인 필요."
    echo "   업로드 의도 시: BOXY_ALLOW_UPLOAD=1 ENV=dev ./scripts/ios-archive-upload.sh"
    exit 2
fi

ENV="${ENV:-dev}"
case "$ENV" in
    dev)  BUNDLE_ID="com.mound.boxy.dev" ;;
    prod) BUNDLE_ID="com.mound.boxy" ;;
    *) echo "❌ Unknown ENV: $ENV (dev|prod)"; exit 1 ;;
esac

# 환경변수 검증
: "${ASC_KEY_ID:?Need ASC_KEY_ID — App Store Connect API Key ID}"
: "${ASC_ISSUER_ID:?Need ASC_ISSUER_ID — App Store Connect Issuer ID}"
: "${ASC_KEY_PATH:?Need ASC_KEY_PATH — .p8 file path}"
: "${TEAM_ID:?Need TEAM_ID — Apple Developer Team ID}"

if [ ! -f "$ASC_KEY_PATH" ]; then
    echo "❌ ASC_KEY_PATH 파일 없음: $ASC_KEY_PATH"
    exit 1
fi

XCODE_PROJ="Builds/ios-${ENV}/Unity-iPhone.xcodeproj"
ARCHIVE_PATH="Builds/boxy-${ENV}.xcarchive"
IPA_DIR="Builds/ios-${ENV}-ipa"
EXPORT_OPTS="Builds/ExportOptions-${ENV}.plist"

UNITY="/Applications/Unity/Hub/Editor/6000.3.14f1/Unity.app/Contents/MacOS/Unity"
echo "🔨 Unity build (buildNumber +1)"
case "$ENV" in
    dev)  METHOD="Boxy.Editor.BoxyBuilder.BumpAndBuildDevIOS" ;;
    prod) METHOD="Boxy.Editor.BoxyBuilder.BumpAndBuildProductionIOS" ;;
esac
"$UNITY" -batchmode -nographics \
    -projectPath "$(pwd)" \
    -buildTarget iOS \
    -executeMethod "$METHOD" \
    -logFile "/tmp/boxy-ios-${ENV}.log" \
    -quit
if [ ! -d "$XCODE_PROJ" ]; then
    echo "❌ Unity 빌드 실패 — /tmp/boxy-ios-${ENV}.log 확인"
    exit 1
fi

# ExportOptions.plist 생성 (TestFlight = app-store-connect)
cat > "$EXPORT_OPTS" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>method</key>
    <string>app-store-connect</string>
    <key>teamID</key>
    <string>${TEAM_ID}</string>
    <key>signingStyle</key>
    <string>automatic</string>
    <key>uploadBitcode</key>
    <false/>
    <key>uploadSymbols</key>
    <true/>
    <key>destination</key>
    <string>export</string>
</dict>
</plist>
EOF

echo "🔧 Patching pbxproj for automatic signing"
# Unity가 생성한 pbxproj는 CODE_SIGN_IDENTITY="iPhone Developer" 강제 + DEVELOPMENT_TEAM="" 비어있음.
# Archive 시 Distribution cert 자동 선택을 막으므로 sed로 직접 패치.
PBXPROJ="$XCODE_PROJ/project.pbxproj"
[ -f "${PBXPROJ}.bak" ] || cp "$PBXPROJ" "${PBXPROJ}.bak"
# 1) "iPhone Developer" 강제 cert 제거 → 빈 값
sed -i '' \
    -e 's/"CODE_SIGN_IDENTITY\[sdk=iphoneos\*\]" = "iPhone Developer";/"CODE_SIGN_IDENTITY[sdk=iphoneos*]" = "";/g' \
    -e 's/CODE_SIGN_IDENTITY = "iPhone Developer";/CODE_SIGN_IDENTITY = "";/g' \
    -e "s/DEVELOPMENT_TEAM = \"\";/DEVELOPMENT_TEAM = \"$TEAM_ID\";/g" \
    "$PBXPROJ"

echo "🔨 Archive: $ENV ($BUNDLE_ID)"
xcodebuild archive \
    -project "$XCODE_PROJ" \
    -scheme Unity-iPhone \
    -configuration Release \
    -destination "generic/platform=iOS" \
    -archivePath "$ARCHIVE_PATH" \
    -allowProvisioningUpdates \
    -authenticationKeyID "$ASC_KEY_ID" \
    -authenticationKeyIssuerID "$ASC_ISSUER_ID" \
    -authenticationKeyPath "$ASC_KEY_PATH" \
    DEVELOPMENT_TEAM="$TEAM_ID" \
    CODE_SIGN_STYLE=Automatic \
    | tail -30
# PRODUCT_BUNDLE_IDENTIFIER override는 의도적으로 제거. xcodebuild가 모든 target에 강제 적용하면
# UnityFramework target도 같은 ID 받아 CFBundleIdentifier Collision (90685) 발생.
# Bundle ID는 이미 BoxyBuilder.cs의 PlayerSettings.SetApplicationIdentifier로 main target에 박혀있음.

echo ""
echo "📦 Export IPA"
xcodebuild -exportArchive \
    -archivePath "$ARCHIVE_PATH" \
    -exportOptionsPlist "$EXPORT_OPTS" \
    -exportPath "$IPA_DIR" \
    -allowProvisioningUpdates \
    -authenticationKeyID "$ASC_KEY_ID" \
    -authenticationKeyIssuerID "$ASC_ISSUER_ID" \
    -authenticationKeyPath "$ASC_KEY_PATH" \
    | tail -10

IPA=$(find "$IPA_DIR" -name "*.ipa" | head -1)
if [ -z "$IPA" ]; then
    echo "❌ IPA 파일 못 찾음 in $IPA_DIR"
    exit 1
fi
echo "✅ IPA: $IPA ($(du -h "$IPA" | cut -f1))"

echo ""
echo "🚀 Upload to TestFlight"
xcrun altool --upload-app \
    -f "$IPA" \
    -t ios \
    --apiKey "$ASC_KEY_ID" \
    --apiIssuer "$ASC_ISSUER_ID"

echo ""
echo "✅ TestFlight 업로드 완료"
echo "   App Store Connect → TestFlight → 빌드 처리 (10~30분 대기)"
echo "   → 처리 완료 후 정수 Apple ID로 내부 테스터 추가 → TestFlight 앱에서 설치"
