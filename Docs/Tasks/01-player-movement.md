# 작업 지시서 01: 플레이어 이동, 대시, 조준

| 항목 | 내용 |
|---|---|
| 일정 | 1주차 1일차 |
| 브랜치 | `feat/player-movement` (이미 만들어져 있음) |
| 담당 | 코드와 씬 배치 모두 직접 작성 |
| 완료 기준 | 색깔 사각형 플레이어가 WASD로 움직이고, 스페이스로 대시하고, 마우스 방향을 조준한다. 수치는 모두 PlayerData에서 읽는다 |
| 기준 문서 | `Docs/Elemental Trials 기획서.md` "조작", "플레이어 스펙" / `Docs/Conventions/code-convention.md` |

---

## 전체 순서

| 단계 | 할 일 | 예상 시간 | 확인 |
|---|---|---|---|
| 1 | PlayerData (ScriptableObject) 만들기 | 20분 | Inspector에 수치가 보인다 |
| 2 | 씬에 플레이어 오브젝트 놓기 | 15분 | Play하면 제자리에 떠 있다 (떨어지지 않음) |
| 3 | PlayerController 뼈대와 입력 연결 | 30분 | 키를 누르면 Console에 값이 찍힌다 |
| 4 | 이동 | 30분 | 2초 동안 D를 누르면 약 9유닛 이동 |
| 5 | 조준 | 30분 | Scene 뷰의 선이 마우스를 따라간다 |
| 6 | 대시 | 1~2시간 | 약 2.1유닛 이동, 0.8초 안에 다시 안 됨 |
| 7 | 정리, 리뷰 요청 | 30분 | Claude 리뷰 → 고치기 → 커밋 |

한 단계가 끝날 때마다 Play로 확인하고 다음 단계로 간다. 한 번에 다 짜고 나서 확인하지 않는다.

---

## 1단계: PlayerData 만들기

**개념: ScriptableObject**
- 씬에 놓지 않고 프로젝트에 파일(.asset)로 저장하는 데이터 묶음이다.
- 수치를 코드 밖에 두면 Play 중에도 Inspector에서 바꿔 보며 조절할 수 있다. 기획서 규칙("수치를 코드에 하드코딩하지 않는다")을 지키는 방법이다.

**만들 파일**
- 스크립트: `Assets/Scripts/Player/PlayerData.cs`
- `ScriptableObject`를 상속하고, `[CreateAssetMenu]`를 붙여 메뉴에서 만들 수 있게 한다.

**넣을 값** (기획서 px ÷ 16 = 유닛)

| 필드 | 값 | 기획서 |
|---|---|---|
| 이동 속도 | 4.5 | 72 px/초 |
| 대시 속도 | 15 | 240 px/초 |
| 대시 시간 | 0.14 | 0.14초 |
| 대시 쿨다운 | 0.8 | 0.8초 |
| 대시 무적 시간 | 0.22 | 0.22초 |

- 필드는 `[SerializeField] private`로 두고, 읽기 전용 프로퍼티로 공개한다 (컨벤션 "필드").
- 피격 판정 크기(0.375)와 체력(6)은 3단계 작업(피격)에서 추가한다. 지금 넣지 않는다.

**에셋 만들기**
- Project 창에서 `Assets/Data/Player/` 폴더를 만들고, 우클릭 → Create → (내가 정한 메뉴 이름)으로 `PlayerData.asset`을 만든다.

**확인**: 에셋을 클릭하면 Inspector에 5개 값이 보이고 수정된다.

---

## 2단계: 씬에 플레이어 놓기

Arena 씬(`Assets/Scenes/Arena.unity`)을 연다.

1. Hierarchy 우클릭 → 2D Object → Sprites → **Square**. 이름을 `Player`로 바꾼다.
   - 이 사각형은 1 × 1 유닛 = 16 × 16 px라 플레이어 셀 크기와 같다.
   - Sprite Renderer의 Color를 하늘색 등으로 바꿔 보스와 구별한다.
2. **Rigidbody2D** 추가

   | 설정 | 값 | 이유 |
   |---|---|---|
   | Body Type | Dynamic | 나중에 벽·보스와 부딪혀야 한다 |
   | Gravity Scale | 0 | 탑다운이라 아래로 떨어지면 안 된다 |
   | Constraints → Freeze Rotation Z | 켬 | 부딪혀도 빙글 돌지 않게 |
   | Interpolate | Interpolate | 물리 프레임 사이를 부드럽게 보여 준다 |

3. **BoxCollider2D** 추가, Size를 0.375 × 0.375로 한다 (기획서 피격 판정 6 × 6 px).
   - 그림(1 × 1)보다 작아도 정상이다. 탄막 게임은 판정이 그림보다 작아야 억울하지 않다.
4. 위치는 (0, 0, 0).

**확인**: Play를 눌러도 플레이어가 떨어지지 않고 제자리에 있다.

- 씬을 저장하면 PR 본문 "씬/프리팹 변경"에 적는다.
- 플레이어를 프리팹으로 만드는 건 3단계 작업(재시작)에서 한다. 지금은 씬에 바로 둔다.

---

## 3단계: PlayerController 뼈대와 입력 연결

**만들 파일**: `Assets/Scripts/Player/PlayerController.cs`

**필드**
- `[SerializeField]`: PlayerData 참조
- private: GameInput, Rigidbody2D, Camera

**생명주기에서 할 일** (컨벤션 "클래스 내부 순서", "성능")

| 메서드 | 할 일 |
|---|---|
| `Awake` | `GameInput`을 `new`로 만든다. Rigidbody2D(`GetComponent`)와 `Camera.main`을 찾아 필드에 저장한다 |
| `OnEnable` | 입력 켜기 (`Player` 맵 Enable) |
| `OnDisable` | 입력 끄기 |
| `OnDestroy` | GameInput `Dispose` |

**입력 읽는 법** (`GameInput` 클래스, 네임스페이스 `ElementalTrials.Input`)
- 계속 읽는 값: `Player.Move`, `Player.Aim` → `ReadValue<Vector2>()`
- 누른 순간: `Player.Dash` → `WasPressedThisFrame()`

**확인**: `Update`에서 이동 입력을 `Debug.Log`로 찍어 본다. WASD에 따라 (0,1), (-1,0) 등이 나오고, 대각선은 (0.71, 0.71)처럼 길이 1이다. 확인이 끝나면 로그를 지운다.

- 플레이어 오브젝트에 컴포넌트를 붙이고, Inspector에서 PlayerData 칸에 1단계 에셋을 끌어다 넣는 걸 잊지 않는다.

---

## 4단계: 이동

**개념: Update와 FixedUpdate**
- `Update`: 화면 프레임마다 (모니터에 따라 60~240번/초). 입력은 여기서 읽는다.
- `FixedUpdate`: 물리 계산마다 (기본 50번/초, 일정함). Rigidbody2D 이동은 여기서 한다.
- 입력을 FixedUpdate에서 읽으면 "누른 순간" 입력을 놓칠 수 있다. 그래서 Update에서 읽고 필드에 저장해 두고, FixedUpdate에서 쓴다.

**의사코드**
```
Update:
    이동입력 = Move 값 읽기 (필드에 저장)

FixedUpdate:
    속도 = 이동입력 × 이동속도
    Rigidbody2D 속도에 넣기
```

**주의**
- Unity 6에서는 Rigidbody2D 속도 이름이 `linearVelocity`다. 인터넷 예제의 `velocity`는 옛 이름이라 경고가 난다.
- `transform.position`을 직접 바꾸지 않는다. 물리 충돌을 무시하고 벽을 뚫는다.
- `Time.deltaTime`을 곱하지 않는다. 속도(유닛/초)를 넣으면 물리 엔진이 시간을 알아서 계산한다.

**확인**
- D를 2초 누르면 약 9유닛 이동한다 (4.5 × 2). 아레나 가로가 20유닛이라 반쯤 간다.
- 대각선도 같은 빠르기다 (입력 길이가 1이라서).
- Play 중에 PlayerData의 이동 속도를 바꾸면 바로 반영된다.

---

## 5단계: 조준

**개념: 화면 좌표와 월드 좌표**
- 마우스 위치(`Aim`)는 화면 픽셀 좌표다 (왼쪽 아래 (0,0), 오른쪽 위 (1920,1080) 같은 값).
- 게임 오브젝트는 월드 좌표(유닛)에 있다. `Camera.ScreenToWorldPoint`로 바꿔야 비교할 수 있다.

**의사코드**
```
Update:
    마우스 화면좌표 = Aim 값 읽기
    마우스 월드좌표 = 카메라.ScreenToWorldPoint(마우스 화면좌표)
    조준방향 = (마우스 월드좌표 - 내 위치)를 정규화   ← z는 무시하고 x, y만
    (확인용) Debug.DrawRay(내 위치, 조준방향 × 2)
```

**공개할 것**: 조준 방향을 읽기 전용 프로퍼티로 공개한다 (`AimDirection`). 다음 작업(기본 공격)에서 탄 방향으로 쓴다.

**주의**
- 마우스가 플레이어와 정확히 겹치면 길이가 0이라 정규화 결과가 (0,0)이다. 이때는 이전 조준 방향을 유지한다.
- 카메라 z는 -10이다. 2D에서는 x, y만 쓰면 된다.

**확인**: Play 중 Scene 뷰(Gizmos 켬)에서 플레이어에서 나온 선이 마우스 쪽을 가리킨다.

---

## 6단계: 대시

**개념: 상태(enum)**
- 플레이어는 지금 "보통"과 "대시 중" 두 상태만 있다. 상태에 따라 FixedUpdate가 다르게 움직인다.
- enum 하나와 switch로 충분하다. 클래스로 나누는 상태머신은 보스에서 제대로 만든다 (패턴 하나 = 상태 클래스 하나).

**필요한 값**
- 현재 상태 (enum: Normal, Dashing)
- 대시 방향
- 대시가 끝나는 시각, 다음 대시가 가능한 시각, 무적이 끝나는 시각
  - 타이머를 매 프레임 빼는 대신 "끝나는 시각"을 저장하고 `Time.time`과 비교하면 단순하다.

**의사코드**
```
Update:
    만약 Dash를 누름 && 상태가 Normal && 지금 >= 다음대시가능시각:
        대시방향 = 이동입력이 있으면 이동입력 방향, 없으면 조준방향
        상태 = Dashing
        대시끝시각 = 지금 + 대시시간
        다음대시가능시각 = 지금 + 대시쿨다운
        무적끝시각 = 지금 + 대시무적시간

FixedUpdate:
    Normal  → 속도 = 이동입력 × 이동속도
    Dashing → 속도 = 대시방향 × 대시속도
              지금 >= 대시끝시각이면 상태 = Normal

무적인가 = 지금 < 무적끝시각     ← 읽기 전용 프로퍼티로 공개 (IsInvincible). 피격 작업에서 쓴다
```

**생각해 볼 것 (면접 질문 거리)**
- 쿨다운은 대시 시작부터 셀까, 끝나고부터 셀까? 기획서는 정하지 않았다. 위 의사코드는 시작부터 센다. 직접 정하고 이유를 말할 수 있으면 된다.
- 대시 무적(0.22초)이 대시(0.14초)보다 긴 이유: 대시가 끝난 직후 0.08초 동안도 안전하게 해서 손맛을 준다.

**확인**
- 대시 시작·끝 위치를 `Debug.Log`로 찍어 거리가 약 2.1유닛인지 본다 (15 × 0.14).
- 스페이스를 연타해도 0.8초에 한 번만 대시한다.
- 가만히 서서 대시하면 마우스 방향으로 간다.
- 확인이 끝나면 로그를 지운다.

---

## 7단계: 정리와 리뷰

1. 셀프 체크
   - [ ] 숫자가 코드에 없다 (전부 PlayerData)
   - [ ] `GetComponent`, `Camera.main`을 Update에서 부르지 않는다
   - [ ] 접근 제한자를 다 썼다 (`private void Update()`)
   - [ ] 디버그 로그를 지웠다 (`Debug.DrawRay`는 남겨도 된다. 게임 화면에는 안 보인다)
   - [ ] Console에 오류·경고가 없다
2. Claude에게 "리뷰해줘"라고 한다. 버그·구조 문제를 짚어 주면 직접 고친다.
3. "커밋해 / PR 올려 / 머지해"
   - 이 브랜치에 이미 들어 있는 `GameInput.cs`와 입력 파일 변경도 같이 커밋된다.
   - 씬을 고쳤으니 PR 본문 "씬/프리팹 변경"에 Arena를 적는다.
4. 막혔다가 해결한 게 있으면 `Docs/troubleshooting.md`에 기록한다 (문제 / 원인 / 해결 / 배운 점).

---

## 자주 막히는 곳

| 증상 | 의심할 곳 |
|---|---|
| 플레이어가 아래로 떨어진다 | Gravity Scale이 0이 아니다 |
| 키를 눌러도 아무 일도 없다 | 입력을 Enable하지 않았다 / PlayerData 칸이 비어 있다 (NullReferenceException) |
| 부딪히면 빙글 돈다 | Freeze Rotation Z를 안 켰다 |
| 움직임이 떨린다 | Interpolate를 안 켰다. 그래도 떨리면 Pixel Perfect Camera의 Grid Snapping 옵션을 확인 |
| 조준 방향이 이상하다 | 마우스 화면좌표를 월드좌표로 안 바꿨다 / z를 같이 계산했다 |
| 대시가 끝나지 않는다 | 끝 시각 비교를 FixedUpdate에서 안 한다 / 상태를 Normal로 안 돌렸다 |
| `velocity` 경고 | Unity 6에서는 `linearVelocity` |

---

## 이번 작업에 넣지 않는 것

- 벽(아레나 경계): 다음 작업에서 보스·탄과 함께 물리 레이어를 정하면서 만든다. 지금은 화면 밖으로 나가도 된다.
- 체력, 피격, 깜빡임: 1주차 3번째 작업
- 대시 잔상, 걷기 애니메이션: 3주차 (그림이 들어온 뒤)
