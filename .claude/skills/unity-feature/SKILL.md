---
name: unity-feature
description: Unity 프로젝트에서 기능을 구현할 때 확인 → 브리핑 → 구현 → 검증 → 보고 절차를 따른다. 오브젝트 생성, 컴포넌트 설정, C# 스크립트 작성, 씬/프리팹 수정 요청에 사용.
---

# Unity 기능 구현

## 1. 확인
- 브랜치: 현재 브랜치만 확인한다. 작업 브랜치 생성은 브리핑 승인 후 한다.
- `unity status`로 에디터 연결(ready)과 열린 씬, 저장 안 된 변경 여부를 본다.
  - 에디터가 연결되지 않으면 YAML을 직접 고치지 말고, 사용자에게 에디터를 열어 달라고 요청한다.
  - 에디터가 켜져 있는데 연결되지 않으면 `unity pipeline list`로 Safe Mode인지 확인한다. Safe Mode면 컴파일 오류부터 고친다 (스크립트 수정은 허용).
  - `unity` 명령이 없으면 CLAUDE.md "필요한 도구"의 설치 스크립트를 실행한다.
- 관련 씬, 프리팹, 스크립트를 찾아 읽는다. 재사용할 기존 구현이 있는지 본다.
- 코드 컨벤션(`Docs/Conventions/code-convention.md`)을 확인한다.
- 대상 씬이나 요구 사항이 불분명하면 먼저 묻는다.
- 씬이나 프리팹을 고쳐야 하면 `Docs/Conventions/scene-prefab.md`의 "씬·프리팹 수정 전 확인"을 하고 보고한다.
  **사용자가 진행하라고 할 때까지 씬·프리팹을 수정하지 않는다.** 확인 결과는 브리핑에 함께 적는다.

## 2. 브리핑
- CLAUDE.md "브리핑과 승인"의 형식으로 브리핑하고, 승인받을 때까지 기다린다.
- 수정할 씬·프리팹이 있으면 **생성/수정 파일**에 전체 경로를 적는다.

## 3. 구현
- 브랜치: CLAUDE.md의 "작업 시작" 절차대로, `prod`/`main`/`develop`이면 작업 브랜치를 먼저 만든다.
- 스크립트를 작성한 뒤 `unity recompile`로 컴파일하고, 새 타입이 로드됐는지 확인한다.
- 오브젝트와 컴포넌트는 `unity command eval`로 실행 중인 에디터에 만든다.
  - Windows에서는 eval을 Bash에서 실행한다. PowerShell에서는 따옴표가 깨진다.
  - 코드가 길면 파일로 저장하고 `"$(cat 파일)"`로 넘긴다.
- 씬·프리팹 YAML은 직접 수정하지 않는다.
- Inspector 참조는 SerializedObject로 연결한다.
- 작업이 끝나면 씬을 저장한다.

## 4. 검증
- 콘솔에 컴파일 오류가 없는지 본다.
- Play 모드로 동작을 확인한다.
  1. `set_autotick` → `editor_play` → `unity status --until-ready`
  2. `wait_for`로 Time.frameCount가 바뀌는지 확인한다 (게임이 멈춰 있지 않은지).
  3. 입력은 가상 장치(`InputSystem.AddDevice<Keyboard>`)로 넣고, 위치·상태값을 읽어서 확인한다.
  4. 필요하면 `capture_game_view`로 화면을 캡처해서 본다.
  5. 가상 장치를 제거하고 `editor_stop` 후 씬 저장 상태를 확인한다.

## 5. 보고
- 변경 사항: 스크립트, 씬 오브젝트, 에셋
- 검증 결과: 무엇을 어떻게 확인했고 수치가 어땠는지
- 남은 작업: 직접 확인이 필요한 것, 하지 않은 것
