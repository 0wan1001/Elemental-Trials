---
name: merge
description: 머지 조건(리뷰 승인, CI 통과, 충돌 없음)을 확인한 뒤 PR을 머지하고 브랜치를 정리한다. 작업 PR과 릴리스 PR에 사용. "머지해", "PR 합쳐" 같은 요청에 사용.
---

# PR 머지

1. 대상 PR 확인: PR 번호를 모르면 현재 브랜치의 PR을 찾는다 (`gh pr view`). 없으면 사용자에게 묻는다.
2. 상태 확인: `gh pr view <번호> --json state,baseRefName,headRefName,mergeable,reviewDecision,statusCheckRollup,body`
   - PR 종류를 정한다 (`Docs/Conventions/git.md` "머지" 표).

     | PR 종류 | 보낸 브랜치 → 기준 브랜치 | 머지 후 보낸 브랜치 | 진행 조건 |
     |---|---|---|---|
     | 작업 PR | 작업 브랜치 → `develop` (develop 없는 저장소는 `main`) | 삭제 | 기본 |
     | 릴리스 PR | `develop` → `prod`/`main` | 삭제 안 함 | 사용자가 릴리스를 명시적으로 요청 |

     위 표에 없는 조합이면 멈추고 사용자에게 알린다.
   - 아래 조건을 모두 만족해야 한다. 하나라도 안 되면 멈추고 무엇이 안 됐는지 보고한다.
     - [ ] 리뷰 승인 (`reviewDecision`이 `APPROVED`)
       - 값이 비어 있으면 저장소에 "리뷰 필수" 설정이 없는 것이다. 승인 리뷰가 있는지 `gh pr view <번호> --json reviews`로 확인하고, 없으면 리뷰 없이 머지할지 사용자에게 묻는다.
       - `CHANGES_REQUESTED`, `REVIEW_REQUIRED`이면 멈춘다.
     - [ ] CI 통과 (실패하거나 진행 중인 체크가 없음). 체크가 하나도 없으면 "CI 없음"으로 보고하고 진행한다.
     - [ ] 충돌 없음 (`mergeable`이 `MERGEABLE`). 충돌이 있으면 `/sync`로 해결하자고 안내한다.
       `UNKNOWN`이면 GitHub가 계산 중이므로 잠시 뒤 다시 확인한다.
3. 머지 전 확인: 머지 커밋 메시지, 머지 후 지울 브랜치를 사용자에게 보여주고 확인을 받는다.
4. 머지: 머지 커밋 방식(squash 아님)으로 아래 형식의 메시지를 쓴다.
   - 형식: `Merge pull request #<번호> <기준 브랜치> ← <보낸 브랜치>`
   - 작업 PR: `gh pr merge <번호> --merge --subject "Merge pull request #<번호> <기준> ← <보낸>" --delete-branch`
   - 릴리스 PR: 위 명령에서 `--delete-branch`를 뺀다.
5. 정리
   - 작업 PR
     - `--delete-branch`는 원격·로컬 브랜치를 지우고, 그 브랜치에 있었다면 기본 브랜치로 옮겨 준다.
     - 현재 브랜치가 기준 브랜치가 아니면 `git switch <기준 브랜치>`, 그다음 `git pull`
     - 로컬에 보낸 브랜치가 남아 있을 때만 지운다: `git branch --list <브랜치>`로 확인 후 `git branch -d <브랜치>`
   - 릴리스 PR
     - 브랜치를 지우지 않는다. `git fetch origin`으로 최신 상태만 받는다.
   - 이슈
     - GitHub 이슈: 기본 브랜치(`develop`, develop 없는 저장소는 `main`)로 머지된 PR은 `closes #번호`로 자동으로 닫힌다. 닫혔는지 확인한다.
     - Jira 등 다른 도구의 이슈: Claude가 닫을 수 없다. 이슈 키를 알려주고 사용자가 직접 상태를 바꾸게 한다.
6. 보고: 머지한 PR, 머지 커밋, 삭제한 브랜치, 이슈 상태

## 하지 않을 것
- `--admin`으로 리뷰나 CI 조건을 건너뛰지 않는다.
- 사용자 확인 없이 머지하지 않는다.
- `git branch -D`로 강제 삭제하지 않는다. `-d`가 실패하면 이유를 보고한다.
- `develop`, `prod`, `main` 브랜치는 어떤 경우에도 지우지 않는다.
