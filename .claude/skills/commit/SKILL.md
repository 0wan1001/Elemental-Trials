---
name: commit
description: 변경 내용을 확인하고 팀 규칙에 맞는 커밋 메시지로 커밋한다. "커밋해", "커밋 메시지 만들어" 같은 요청에 사용.
---

# 커밋하기

1. 규칙 확인: `Docs/Conventions/git.md`가 있으면 읽고 따른다.
2. 브랜치 확인: 현재 브랜치가 `prod`(또는 `main`), `develop`이면 멈추고 알린다. 변경을 어떻게 옮길지 사용자에게 묻는다 (CLAUDE.md "작업 시작" 3번).
3. 변경 확인: `git status`와 `git diff`로 무엇이 바뀌었는지 본다.
4. 범위 나누기: 서로 관련 없는 변경이 섞여 있으면 커밋을 나누자고 제안한다.
5. 스테이징: 관련 파일만 이름을 지정해서 `git add`한다. `git add -A`는 쓰지 않는다.
   - Unity: `.meta` 파일을 원본과 함께 추가한다.
   - 절대 추가하지 않는 것: `Library/`, `Temp/`, 비밀 정보(.env, API 키, Unity Services 키, 서명 키스토어 `.keystore`/`.jks`)
   - LFS 확인: `.gitattributes`에서 LFS로 지정된 파일(png, fbx, wav 등)이 있으면 `git lfs version`으로 Git LFS가 설치됐는지 확인한다.
     설치돼 있지 않으면 커밋하지 말고, CLAUDE.md "필요한 도구"대로 설치한 뒤 다시 `git add`한다.
     스테이징 후 `git lfs status`에서 해당 파일이 LFS로 잡혔는지 확인한다.
6. 메시지 작성: `[타입] 요약` 형식, 요약은 한국어 50자 이내
   - 타입: feat, fix, refactor, docs, chore, asset
   - 스크립트와 씬을 함께 바꿨다면 기능은 `feat`, 버그 수정은 `fix`. 리소스만 바꿨을 때만 `asset`
   - 왜 바꿨는지 설명이 필요하면 본문에 적는다.
7. 커밋 후 `git log -1`로 결과를 보여준다.

## 하지 않을 것
- 요청 없이 push하지 않는다.
- `--no-verify`로 훅을 건너뛰지 않는다. 훅이 실패하면 원인을 고친다.
- 이미 push된 커밋을 amend하거나 rebase하지 않는다.
