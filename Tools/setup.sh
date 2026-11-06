#!/usr/bin/env bash
# ClaudeKit 사전 준비 (macOS)
# 필요한 도구가 없으면 설치하고, Git 설정을 한 번에 끝낸다. 여러 번 실행해도 안전하다.
#
# 사용법 (저장소 루트에서):
#   bash Tools/setup.sh
#   bash Tools/setup.sh "/경로/Unity프로젝트"
#
# 하는 일
#   1. Git, Git LFS, GitHub CLI(gh), Unity CLI 설치 (없을 때만, Homebrew 필요)
#   2. git lfs install
#   3. Unity Smart Merge(UnityYAMLMerge)를 git 병합 도구로 등록
#   4. 프로젝트 경로를 주면 그 Unity 프로젝트에 com.unity.pipeline 패키지 추가
#   5. GitHub 로그인 상태 확인 (로그인은 직접: gh auth login)

set -e
PROJECT_PATH="$1"

has() { command -v "$1" >/dev/null 2>&1; }

if [ "$(uname)" != "Darwin" ]; then
  echo "[오류] 이 스크립트는 macOS 전용입니다. Linux는 README의 '사전 준비' 표를 보고 직접 설치하세요."
  exit 1
fi

if ! has brew; then
  echo "[오류] Homebrew가 없습니다. https://brew.sh 에서 설치한 뒤 다시 실행하세요."
  exit 1
fi

# 1. 도구 설치
has git && echo "[있음] Git" || { echo "[설치] Git"; brew install git; }
git lfs version >/dev/null 2>&1 && echo "[있음] Git LFS" || { echo "[설치] Git LFS"; brew install git-lfs; }
has gh && echo "[있음] gh" || { echo "[설치] gh"; brew install gh; }
has unity && echo "[있음] Unity CLI" || { echo "[설치] Unity CLI"; curl -fsSL https://unity.com/install.sh | bash; }

# 2. Git LFS
git lfs install >/dev/null
echo "[설정] Git LFS"

# 3. Unity Smart Merge 등록
#    프로젝트의 Unity 버전이 설치돼 있으면 그 버전의 도구를, 없으면 설치된 것 중 가장 최신 버전을 쓴다.
#    Unity Hub 기본 경로와, Hub에서 바꾼 설치 경로(secondaryInstallPath)를 모두 찾는다.
ROOTS=("/Applications/Unity/Hub/Editor")
SECONDARY="$HOME/Library/Application Support/UnityHub/secondaryInstallPath.json"
if [ -f "$SECONDARY" ]; then
  SECONDARY_PATH=$(sed -e 's/^"//' -e 's/"$//' "$SECONDARY")
  [ -n "$SECONDARY_PATH" ] && ROOTS+=("$SECONDARY_PATH")
fi

WANTED_VERSION=""
if [ -n "$PROJECT_PATH" ] && [ -f "$PROJECT_PATH/ProjectSettings/ProjectVersion.txt" ]; then
  WANTED_VERSION=$(sed -n 's/^m_EditorVersion: *//p' "$PROJECT_PATH/ProjectSettings/ProjectVersion.txt" | tr -d '\r')
fi

# "버전<TAB>경로" 목록을 만든다
CANDIDATES=$(for root in "${ROOTS[@]}"; do
  [ -d "$root" ] || continue
  for dir in "$root"/*/; do
    version=$(basename "$dir")
    tool="${dir%/}/Unity.app/Contents/Tools/UnityYAMLMerge"
    if [ -x "$tool" ]; then printf '%s\t%s\n' "$version" "$tool"; fi
  done
done)

YAML_MERGE=""
if [ -n "$WANTED_VERSION" ]; then
  YAML_MERGE=$(printf '%s\n' "$CANDIDATES" | awk -F'\t' -v v="$WANTED_VERSION" '$1 == v { print $2; exit }')
fi
if [ -z "$YAML_MERGE" ] && [ -n "$CANDIDATES" ]; then
  # 버전 숫자 순서로 정렬해서 가장 최신 것을 고른다 (문자열 순서가 아님)
  YAML_MERGE=$(printf '%s\n' "$CANDIDATES" | sort -t. -k1,1n -k2,2n -k3,3n | tail -n 1 | cut -f2)
fi

if [ -n "$YAML_MERGE" ]; then
  git config --global merge.unityyamlmerge.name "Unity SmartMerge"
  git config --global merge.unityyamlmerge.driver "'$YAML_MERGE' merge -p %O %B %A %A"
  git config --global merge.unityyamlmerge.recursive binary
  echo "[설정] Unity Smart Merge: $YAML_MERGE"
else
  echo "[건너뜀] UnityYAMLMerge를 찾지 못했습니다. Unity Hub로 에디터를 설치한 뒤 다시 실행하세요."
  echo "        Hub를 쓰지 않고 설치했다면 .gitattributes 상단 설명대로 경로를 직접 등록하세요."
fi

# 4. Unity 프로젝트에 pipeline 패키지 추가 (에디터를 열기 전에)
if [ -n "$PROJECT_PATH" ]; then
  unity pipeline install --project-path "$PROJECT_PATH"
  echo "[설정] com.unity.pipeline: $PROJECT_PATH"
fi

# 5. GitHub 로그인 확인
if gh auth status >/dev/null 2>&1; then
  echo "[있음] GitHub 로그인"
else
  echo "[필요] GitHub 로그인이 필요합니다. 직접 실행하세요: gh auth login"
fi

echo "완료"
