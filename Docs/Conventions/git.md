# 브랜치 · 커밋 · 머지 규칙

## 브랜치 구성
| 브랜치 | 역할 | 규칙 |
|---|---|---|
| `prod` | 빌드·배포 | 배포 가능한 안정 버전. 새 커밋이 들어오면 CI가 자동 빌드한다. 직접 push 금지, `develop`에서 오는 릴리스 PR로만 변경 |
| `develop` | 개발 통합·검증 | 작업 브랜치를 합쳐서 기능 동작과 충돌을 검증한다. 직접 push 금지, PR로만 변경. **GitHub 저장소의 기본 브랜치로 설정한다** |
| 작업 브랜치 | 개인 개발 | 항상 최신 `prod`에서 만든다. 이슈 단위로 개발하고, `develop`으로 PR을 보낸다. 머지 후 삭제 |

- `prod`는 다른 프로젝트에서 `main`일 수 있다. 그때는 이 문서의 `prod`를 `main`으로 읽는다.
- `develop`을 기본 브랜치로 두는 이유: PR 본문의 `closes #번호`는 기본 브랜치에 머지될 때만 이슈를 닫는다.

## 흐름
### 기본 흐름
```
prod ──(분기)──▶ 작업 브랜치 ──(PR)──▶ develop ──(검증 후 릴리스 PR)──▶ prod ──▶ 자동 빌드
```
1. 최신 `prod`에서 작업 브랜치를 만든다.
2. 개발과 로컬 테스트가 끝나면 `develop`으로 PR을 만든다.
3. `develop`에서 통합 검증을 한다.
4. 검증이 끝나면 `develop` → `prod` PR(릴리스 PR)을 만든다.
5. `prod`에 머지되면 CI가 빌드한다.

### 이 문서에 없는 흐름
- 급한 수정(핫픽스), 다른 사람의 미배포 기능에 이어서 작업하기 같은 별도 흐름은 아직 정하지 않았다.
- 급한 수정도 기본 흐름을 따른다 (`develop`에서 검증한 뒤 릴리스 PR).
- 기본 흐름으로 처리할 수 없는 상황이면, 임의로 방법을 만들지 말고 멈추고 사용자에게 알린다. 필요해지면 팀 합의 후 이 문서에 추가한다.

### develop이 없는 저장소 (개인·사이드 프로젝트)
```
main ──(분기)──▶ 작업 브랜치 ──(PR)──▶ main
```
- `git ls-remote --heads origin develop`으로 `develop`이 있는지 확인한다. 없으면 이 흐름을 쓴다.
- 릴리스 PR은 없다.

## 작업 브랜치 이름
```
<타입>/<이슈키>-<짧은-영어-설명>
```
- 예: `feat/FB-12-player-jump`, `fix/FB-15-landing-check`, `docs/FB-20-daily-log`
- 타입은 아래 커밋 타입과 같다.
- 이슈 키는 이슈 관리 도구의 번호를 그대로 쓴다 (Jira `FB-12`, GitHub 이슈 `12`). 이슈가 없으면 생략한다: `chore/update-gitignore`
- 설명은 영어 소문자와 하이픈만 쓴다.
- **예외: 프로젝트에 정해진 브랜치 이름 규칙이 있으면 그 규칙을 따른다.**
  그 규칙은 CLAUDE.md "프로젝트 설정" 표의 "브랜치 이름 규칙"에 적는다. 예: Fourbeat는 Jira 이슈 번호만 쓴다 → `FB-12`
  - 그 규칙이 이슈 번호를 요구하는데 이슈가 없으면, 이름을 지어내지 말고 이슈를 먼저 만들지 사용자에게 묻는다.

## 커밋 메시지
```
[타입] 요약

필요하면 본문: 왜 바꿨는지
```

| 타입 | 용도 |
|---|---|
| `feat` | 새 기능 |
| `fix` | 버그 수정 |
| `refactor` | 동작 변화 없는 코드 개선 |
| `docs` | 문서 |
| `chore` | 설정, 빌드, 기타 |
| `asset` | 씬, 프리팹, 리소스만 바꾼 경우 |

- 요약은 한국어로 50자 이내, 마침표 없이 쓴다. 예: `[feat] 플레이어 점프 추가`
- 커밋 하나에는 관련된 변경만 담는다.
- 스크립트와 씬·프리팹을 함께 바꿨다면, 기능을 만든 것이면 `feat`, 버그를 고친 것이면 `fix`로 쓴다. 리소스만 바꿨을 때만 `asset`을 쓴다.
- 예외: 머지 커밋은 이 형식을 따르지 않는다.
  - PR 머지 커밋: 아래 "머지" 형식
  - `/sync`로 만든 머지 커밋: git 기본 메시지 (`Merge remote-tracking branch 'origin/develop' into ...`)

## PR 제목
- 커밋 메시지와 같은 `[타입] 요약` 형식을 쓴다.
- 예외: 릴리스 PR은 `[release] 요약`

## 머지
- PR은 머지 커밋 방식으로 합친다 (squash 아님).
- 머지 커밋 메시지 형식:
  ```
  Merge pull request #<PR번호> <기준 브랜치> ← <보낸 브랜치>
  ```
  | PR 종류 | 예 | 머지 후 보낸 브랜치 |
  |---|---|---|
  | 작업 PR | `Merge pull request #246 develop ← feat/FB-12-player-jump` | 삭제 |
  | 릴리스 PR | `Merge pull request #250 prod ← develop` | 삭제 안 함 |
- `prod`, `main`, `develop`은 어떤 경우에도 삭제하지 않는다.

## 라벨
- 라벨 목록은 `Docs/Conventions/labels.md`를 따른다.

## Unity 저장소
- Unity 프로젝트 폴더(Assets, Packages, ProjectSettings가 있는 폴더)가 저장소 루트여야 한다.
  하위 폴더에 있으면 `.gitignore`의 `/Library/` 같은 경로가 동작하지 않는다. 이때는 경로 앞에 폴더 이름을 붙인다 (예: `/Game/Library/`).
- `.gitignore`로 `Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`를 제외한다.
- `.meta` 파일은 항상 원본 파일과 함께 커밋한다.
- 큰 리소스(텍스처, 사운드, 모델)는 Git LFS로 관리한다. Git LFS가 설치되지 않은 컴퓨터에서 커밋하지 않는다.
- 씬·프리팹 충돌은 Unity Smart Merge(UnityYAMLMerge)로 처리한다.
- 위 설정은 `.gitignore`, `.gitattributes`에 있다. 컴퓨터마다 처음 한 번 설치 스크립트를 실행한다.
  - Windows: `powershell -ExecutionPolicy Bypass -File Tools/setup.ps1`
  - macOS: `bash Tools/setup.sh`
