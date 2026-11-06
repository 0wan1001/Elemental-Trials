---
name: pr
description: 현재 브랜치의 변경 내용으로 팀 템플릿에 맞는 PR 본문을 작성하고 생성한다. 작업 PR과 릴리스 PR에 사용. "PR 만들어", "PR 올려" 같은 요청에 사용.
---

# PR 작성

1. 상태 확인
   - 커밋 안 된 변경이 있으면 먼저 알린다.
   - `develop`이 있는지 확인한다: `git ls-remote --heads origin develop`
   - PR 종류와 기준 브랜치를 정한다 (`Docs/Conventions/git.md` "흐름" 참고).

     | PR 종류 | 보내는 브랜치 → 기준 브랜치 | 조건 |
     |---|---|---|
     | 작업 PR | 작업 브랜치 → `develop` | 기본 |
     | 작업 PR (develop 없는 저장소) | 작업 브랜치 → `main` | `develop`이 없을 때 |
     | 릴리스 PR | `develop` → `prod`(또는 `main`) | 사용자가 릴리스를 요청했을 때만 |

   - 위 표에 없는 조합이면 멈추고 사용자에게 알린다.
2. 변경 파악: `git fetch origin` 후 `git log origin/<기준>..<보내는 브랜치>`와
   `git diff origin/<기준>...<보내는 브랜치> --stat`으로 전체 변경을 본다. 마지막 커밋만 보고 쓰지 않는다.
   - 작업 PR: `<보내는 브랜치>`는 로컬 `HEAD`
   - 릴리스 PR: `<보내는 브랜치>`는 `origin/develop`
3. 셀프 리뷰: 아래 체크리스트를 확인하고, 걸리는 항목은 PR 본문에 적는다.
   - [ ] 요청과 무관한 변경이 섞이지 않았다
   - [ ] 디버그 로그나 주석 처리된 코드가 남아 있지 않다
   - [ ] 컴파일 오류가 없다
   - [ ] 씬·프리팹, 프로젝트 설정(Packages, ProjectSettings)을 바꿨다면 본문에 적었다
4. 본문 작성: `.github/pull_request_template.md`의 형식을 따른다.
   - 템플릿이 없으면 사용자에게 알리고, 어떤 형식으로 쓸지 묻는다.
   - 관련 이슈
     - GitHub 이슈가 있으면 `closes #번호`
     - 다른 도구의 이슈(Jira 등)면 이슈 키만 적는다. 예: `FB-12`
     - 이슈가 없으면 `없음`이라고 적는다. 빈 `closes #`를 남기지 않는다.
   - 릴리스 PR이면 포함된 작업 PR 목록을 적는다.
     1. 마지막 릴리스 시각: `gh pr list --base prod --state merged --limit 1 --json mergedAt`
     2. 그 뒤 develop에 머지된 PR: `gh pr list --base develop --state merged --search "merged:>=<시각>" --json number,title`
     (릴리스가 처음이면 2번에서 `--search`를 빼고 전체를 본다.)
   - 스크린샷은 파일 경로를 알려주고, 사용자가 PR 화면에 끌어다 놓게 한다 (gh로는 이미지를 올릴 수 없다).
5. 제목: `Docs/Conventions/git.md` "PR 제목"을 따른다.
   - 작업 PR: `[타입] 요약`
   - 릴리스 PR: `[release] 요약`
6. 라벨, 담당자, 리뷰어
   - 라벨: `Docs/Conventions/labels.md`를 따른다.
   - 담당자: 실행한 사람 (`--assignee @me`)
   - 리뷰어: 사용자에게 누구에게 리뷰를 받을지 묻는다. 작성자는 자기 PR을 승인할 수 없으므로 다른 사람이어야 한다.
     혼자 쓰는 저장소처럼 리뷰어가 없으면 "없음"으로 두고 `--reviewer`를 뺀다.
   - 저장소에 해당 라벨이 없으면 만들지 말고 사용자에게 알린다 (`gh label list`로 확인).
7. 생성 전에 제목, 본문, 라벨, 담당자, 리뷰어를 사용자에게 보여주고 확인을 받는다.
8. 확인을 받으면 아래 순서로 생성하고 링크를 알린다.
   - 작업 PR: 작업 브랜치를 push(`git push -u origin <브랜치>`)한다. 릴리스 PR은 push하지 않는다.
   - `gh pr create --base <기준> --head <보내는 브랜치> --title "..." --body "..." --label <라벨> --assignee @me --reviewer <리뷰어>`

## 하지 않을 것
- `prod`, `main`, `develop`에 직접 push하지 않는다.
