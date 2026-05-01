#!/usr/bin/env bash
# CLAUDE.md §19 — 기술적 강제 검사
# 사용법:
#   scripts/check-violations.sh staged   # git staged 파일만 (pre-commit)
#   scripts/check-violations.sh all      # Assets/ 전체 .cs

set -e

MODE="${1:-staged}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [ "$MODE" = "staged" ]; then
  if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    echo "[check-violations] git 저장소가 아닙니다."
    exit 2
  fi
  FILES=$(git diff --cached --name-only --diff-filter=ACM 2>/dev/null | grep '\.cs$' || true)
elif [ "$MODE" = "all" ]; then
  if [ -d "Assets" ]; then
    FILES=$(find Assets -type f -name '*.cs' 2>/dev/null || true)
  else
    FILES=$(find . -type f -name '*.cs' -not -path './Library/*' -not -path './Temp/*' 2>/dev/null || true)
  fi
else
  echo "사용법: $0 [staged|all]"
  exit 2
fi

if [ -z "$FILES" ]; then
  echo "[check-violations] 검사할 .cs 파일 없음"
  exit 0
fi

ERRORS=0
WARNINGS=0

# 검사 함수: rule, desc, pattern, severity
run_check() {
  local rule="$1"
  local desc="$2"
  local pattern="$3"
  local severity="$4"

  local hits
  hits=$(printf '%s\n' $FILES | xargs grep -nE "$pattern" 2>/dev/null || true)
  _report_hits "$rule" "$desc" "$severity" "$hits"
}

# 검사 함수 (특정 경로 제외): rule, desc, pattern, severity, exclude_path_substr
run_check_excluding() {
  local rule="$1"
  local desc="$2"
  local pattern="$3"
  local severity="$4"
  local exclude="$5"

  local filtered_files
  # exclude는 alternation 가능 (예: "Editor|Tests") — grep -vE로 regex OR 처리
  filtered_files=$(printf '%s\n' $FILES | grep -vE "/($exclude)/" || true)
  local hits
  hits=$(printf '%s\n' $filtered_files | xargs grep -nE "$pattern" 2>/dev/null || true)
  _report_hits "$rule" "$desc" "$severity" "$hits"
}

_report_hits() {
  local rule="$1"
  local desc="$2"
  local severity="$3"
  local hits="$4"

  if [ -n "$hits" ]; then
    if [ "$severity" = "error" ]; then
      echo "❌ §${rule} 위반: ${desc}"
      ERRORS=$((ERRORS + 1))
    else
      echo "⚠️  §${rule} 검토: ${desc}"
      WARNINGS=$((WARNINGS + 1))
    fi
    printf '%s\n' "$hits" | head -10 | sed 's/^/   /'
    echo ""
  fi
}

# §9-1 하드코딩 API 키 — Google 공식 테스트 publisher (3940256099942544) 제외 (post-filter)
# 테스트 ID 출처: https://developers.google.com/admob/android/test-ads
key_hits=$(printf '%s\n' $FILES | xargs grep -nE \
  'AIza[A-Za-z0-9_-]{35}|ca-app-pub-[0-9]{15,}[~/][0-9]+|sk_(live|test)_[A-Za-z0-9]{20,}|pk_(live|test)_[A-Za-z0-9]{20,}' \
  2>/dev/null | grep -v '3940256099942544' || true)
if [ -n "$key_hits" ]; then
  echo "❌ §9-1 위반: 하드코딩된 API 키 의심"
  ERRORS=$((ERRORS + 1))
  printf '%s\n' "$key_hits" | head -10 | sed 's/^/   /'
  echo ""
fi

# §4-1 GameObject.Find / FindObjectOfType
# Editor/ + Tests/ 폴더는 예외 — 에디터 자동화 + PlayMode 통합 테스트는 씬 로드 후 컴포넌트 query 필요 (런타임 성능과 무관)
run_check_excluding "4-1" "GameObject.Find / FindObjectOfType 사용 — [SerializeField] 또는 DI로" \
  'GameObject\.Find[A-Za-z]*\(|FindObjectOfType<|FindObjectsOfType<|FindAnyObjectByType<|FindFirstObjectByType<' \
  "error" \
  "Editor|Tests"

# §11-1 빈 catch 블록
run_check "11-1" "빈 catch 블록 — 최소 로그 또는 처리 필요" \
  'catch[[:space:]]*(\([^)]*\))?[[:space:]]*\{[[:space:]]*\}' \
  "error"

# §6-1 public 컬렉션 노출
run_check "6-1" "public 컬렉션 노출 — Repository/도메인 타입으로 wrap" \
  '^[[:space:]]*public[[:space:]]+(Dictionary|List|HashSet|SortedDictionary|Queue|Stack)<' \
  "error"

# §3-1 주석 처리된 코드 의심 — 식별자 다음 () 호출 패턴만. 문서 주석 (// CLAUDE.md §X-X) 제외
run_check "3-1" "주석 처리된 코드 의심 — 즉시 삭제 (git이 히스토리 보관)" \
  '^[[:space:]]*//[[:space:]]*[A-Za-z_][.A-Za-z0-9_]*\(' \
  "warn"

# §11-2 async void (Unity 이벤트 핸들러 외 의심)
run_check "11-2" "async void 사용 — Unity 이벤트 핸들러만 허용" \
  'async[[:space:]]+void[[:space:]]+[A-Za-z_]' \
  "warn"

# §3-1 TODO 단독 (TODO(#issue) 형식 권장)
run_check "3-1" "TODO 단독 — TODO(#issue) 형식 권장" \
  '//[[:space:]]*TODO[^(:]' \
  "warn"

# §3-3 매직 문자열 PlayerPrefs 직접 호출
run_check "3-3" "PlayerPrefs 매직 문자열 — PrefsKey 정적 클래스 사용" \
  'PlayerPrefs\.(Get|Set|Has|Delete)[A-Za-z]*\([[:space:]]*"' \
  "warn"

echo ""
if [ "$ERRORS" -gt 0 ]; then
  echo "🛑 에러 ${ERRORS}건, 경고 ${WARNINGS}건 — 커밋 차단"
  echo "수정 후 다시 시도하세요. 우회 필요 시 'git commit --no-verify' (권장 X)"
  exit 1
fi

if [ "$WARNINGS" -gt 0 ]; then
  echo "✅ 에러 없음 (경고 ${WARNINGS}건 — 검토 권장)"
else
  echo "✅ 패턴 검사 통과"
fi
exit 0
