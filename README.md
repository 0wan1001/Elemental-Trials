# ClaudeKit

Claude Code 설정 모음. 팀 저장소용 설정과 개인 설정이 함께 들어 있다.

## 구성
```
ClaudeKit/
├─ CLAUDE.md                        팀 공통 규칙 (Claude가 자동으로 읽음)
├─ .gitignore                       Unity 저장소 제외 목록
├─ .gitattributes                   줄바꿈, LFS, Unity Smart Merge 설정
├─ Tools/
│  ├─ setup.ps1                     사전 준비 스크립트 (Windows)
│  └─ setup.sh                      사전 준비 스크립트 (macOS)
├─ Docs/Conventions/
│  ├─ code-convention.md            C# 코드 컨벤션
│  ├─ git.md                        브랜치·커밋·머지 규칙
│  ├─ labels.md                     이슈·PR 라벨 목록
│  └─ scene-prefab.md               씬·프리팹 규칙
├─ .claude/settings.json            명령 허용·금지 목록 (확인 질문을 줄이고 위험한 명령을 막음)
├─ .claude/skills/                  작업별 절차 (/이름 으로 실행)
│  ├─ commit/                       커밋
│  ├─ issue/                        이슈 작성
│  ├─ pr/                           PR 작성
│  ├─ sync/                         최신화·충돌 해결
│  ├─ merge/                        PR 머지
│  └─ unity-feature/                Unity 기능 구현
├─ .github/                         GitHub 이슈(기능·버그·작업)·PR 템플릿
└─ Personal/                        개인 규칙 (팀 저장소에 복사하지 않음)
   └─ CLAUDE.md                     → ~/.claude/CLAUDE.md
```

## 원본과 적용 위치
이 폴더(ClaudeKit)가 모든 설정의 원본이다. 고칠 때는 항상 여기서 고친 뒤 적용 위치로 복사한다.

| 원본 | 적용 위치 | 범위 |
|---|---|---|
| `CLAUDE.md`, `Docs/`, `.claude/`, `.github/`, `Tools/`, `.gitignore`, `.gitattributes` | 각 팀 저장소 루트 | 팀 전체. 그 저장소에서 작업할 때만 적용 |
| `Personal/CLAUDE.md` | `~/.claude/CLAUDE.md` | 나 혼자. 모든 프로젝트에 적용 |

- Claude는 두 CLAUDE.md를 함께 읽는다. 팀 규칙은 팀 파일에, 말투·보고 방식 같은 개인 취향은 개인 파일에 둔다.
- 두 파일이 부딪히면 프로젝트 작업 규칙은 팀 파일을, 소통 방식은 개인 파일을 따른다 (개인 파일에 적혀 있음).
- 스킬은 팀 저장소의 `.claude/skills/`만 쓴다. `~/.claude/skills/`에는 같은 이름의 스킬을 두지 않는다.
  같은 이름이 있으면 개인 스킬이 저장소 스킬보다 먼저 적용되어, 팀이 고친 내용이 반영되지 않는다.
  `~/.claude/skills/`에는 팀과 상관없는 나만의 스킬만 둔다.

## 사전 준비 (컴퓨터마다 한 번)
| 도구 | 하는 일 | 쓰는 곳 |
|---|---|---|
| Git | 버전 관리. Windows용 Git에는 Git LFS가 포함돼 있다 | 모든 스킬 |
| Git LFS | 이미지·모델·사운드 같은 큰 파일을 저장소 밖에 따로 보관 | 커밋, push |
| gh (GitHub CLI) | 터미널에서 GitHub 이슈·PR을 만들고 머지 | /issue, /pr, /merge |
| unity CLI | 터미널에서 열려 있는 Unity 에디터를 조작 | /unity-feature, /sync 검증 |

설치 스크립트가 없는 도구를 설치하고 Git LFS, Unity Smart Merge 설정까지 끝낸다. 여러 번 실행해도 안전하다.
Claude도 도구가 없으면 이 스크립트를 실행한다.
```powershell
# Windows (저장소 루트에서)
powershell -ExecutionPolicy Bypass -File Tools/setup.ps1
# Unity 프로젝트에 com.unity.pipeline 패키지까지 추가할 때 (에디터를 열기 전에)
powershell -ExecutionPolicy Bypass -File Tools/setup.ps1 -ProjectPath "C:\경로\Unity프로젝트"
```
```bash
# macOS (Homebrew 필요)
bash Tools/setup.sh
bash Tools/setup.sh "/경로/Unity프로젝트"
```
GitHub 로그인은 스크립트가 대신할 수 없다. 처음 한 번 직접 실행한다.
```bash
gh auth login
```
Linux는 스크립트를 지원하지 않는다. 위 표의 도구를 배포판 패키지 관리자로 설치하고, `git lfs install`과
`.gitattributes` 상단의 Smart Merge 등록 명령을 직접 실행한다.

## 저장소 조건
- Unity 프로젝트 폴더(Assets, Packages, ProjectSettings가 있는 폴더)가 저장소 루트여야 한다.
  하위 폴더에 있으면 `.gitignore`의 `/Library/` 같은 경로가 동작하지 않아 수 GB가 커밋될 수 있다.
  이때는 `.gitignore`의 각 경로 앞에 폴더 이름을 붙인다 (예: `/Game/Library/`).
- `develop` 브랜치가 없는 저장소(개인·사이드 프로젝트)는 `main`으로 바로 PR을 보내는 흐름을 쓴다 (git.md "develop이 없는 저장소").

## 권한 설정 (.claude/settings.json)
- 자주 쓰는 읽기 명령(git status, gh pr view 등)은 묻지 않고 실행한다.
- 강제 push, 브랜치 강제 삭제, `prod`/`develop`/`main` 원격 삭제, 리뷰를 건너뛰는 머지를 1차로 막는다.
- **완전한 차단은 아니다.** 아래 경우는 이 목록으로 막지 못한다.
  - 옵션 순서를 바꾼 명령 (예: `git push origin feat --force`, `git push origin +feat`)
  - Windows에서 Claude가 Bash가 아닌 PowerShell 도구로 실행한 명령 (`Bash(...)` 규칙은 Bash 도구에만 적용된다)
- 그래서 실제 보호는 두 겹으로 한다.
  1. 스킬과 CLAUDE.md의 "하지 않을 것" 규칙
  2. GitHub 저장소의 브랜치 보호 설정 (`prod`, `develop` 직접 push·강제 push·삭제 금지). 이것이 가장 확실하다.
- 개인적으로 더 허용하고 싶은 명령은 저장소의 `.claude/settings.local.json`에 적는다 (커밋되지 않음).

## 팀 저장소에 적용
1. `Personal/`을 뺀 나머지를 팀 저장소 루트에 복사한다 (ClaudeKit 폴더 자체가 아니라 안의 내용).
   - 저장소에 이미 `.gitignore`, `.gitattributes`가 있으면 덮어쓰지 않는다.
     두 파일을 비교해서 키트에만 있는 줄을 기존 파일 끝에 추가한다. 기존 줄은 지우지 않는다.
   - 이미 커밋된 이미지·모델은 LFS로 자동으로 옮겨지지 않는다. 팀 합의 후 아래처럼 옮긴다.
     ```bash
     # 작업 브랜치에서 실행하고, PR로 develop에 합친다 (prod, develop은 직접 push 금지)
     git lfs migrate import --no-rewrite "*.png" "*.fbx" "*.wav"   # 옮길 확장자를 나열
     ```
     - `--no-rewrite`는 커밋 기록을 바꾸지 않고, 현재 파일만 LFS로 옮기는 커밋 하나를 새로 만든다.
       팀원은 평소처럼 pull만 하면 된다.
     - 기록 전체를 다시 쓰는 방식(`--everything`)은 쓰지 않는다. 강제 push가 필요하고, 모든 팀원이 저장소를 새로 받아야 한다.
2. 팀 상황에 맞게 CLAUDE.md(프로젝트 설정 표)와 Docs/Conventions를 고친다.
3. GitHub 저장소 설정
   - 기본 브랜치를 `develop`으로 바꾼다 (Settings → General → Default branch).
   - `prod`, `develop`에 브랜치 보호를 건다 (Settings → Branches → Branch protection). 아래를 켠다.
     - PR 없이 push 금지 (Require a pull request before merging)
     - 강제 push 금지, 브랜치 삭제 금지 (Allow force pushes, Allow deletions를 끈 상태로 둔다)
   - `Docs/Conventions/labels.md`의 라벨 만들기 명령을 실행한다.
4. 커밋해서 팀원과 공유한다.
5. 팀원 각자 설치 스크립트를 한 번 실행한다.

## 개인 규칙 적용
```bash
cp Personal/CLAUDE.md ~/.claude/CLAUDE.md
```
