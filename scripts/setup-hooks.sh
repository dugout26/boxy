#!/usr/bin/env bash
# CLAUDE.md §19 — git pre-commit hook 설치
# 1회 실행: scripts/setup-hooks.sh

set -e

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [ ! -d ".git" ]; then
  echo "[setup-hooks] git 저장소 아님. 먼저 'git init' 실행."
  exit 1
fi

mkdir -p .git/hooks

cat > .git/hooks/pre-commit <<'HOOK'
#!/usr/bin/env bash
# 자동 생성됨 by scripts/setup-hooks.sh
# CLAUDE.md §19 기술적 강제

ROOT="$(git rev-parse --show-toplevel)"
exec "$ROOT/scripts/check-violations.sh" staged
HOOK

chmod +x .git/hooks/pre-commit
chmod +x "$ROOT/scripts/check-violations.sh"

echo "✅ pre-commit hook 설치 완료"
echo "   - 커밋 시 자동 검사: 활성"
echo "   - 수동 전체 검사:    scripts/check-violations.sh all"
echo "   - 우회(권장 X):      git commit --no-verify"
