# C# 코드 컨벤션

## 네이밍
| 대상 | 규칙 | 예시 |
|---|---|---|
| 네임스페이스, 클래스, 구조체, 메서드, 프로퍼티 | PascalCase | `PlayerMover`, `TakeDamage()` |
| private 필드 (static 포함) | `_camelCase` | `_moveSpeed`, `_monsterKillCount` |
| public 필드 (static 포함) | PascalCase | `Count`, `Name` |
| 상수 (`const`) | UPPER_SNAKE_CASE | `MAX_HEALTH` |
| `static readonly` | 상수처럼 바뀌지 않는 값이면 UPPER_SNAKE_CASE, 아니면 위 필드 규칙 | `DEFAULT_COLOR` |
| 지역 변수, 매개변수 | camelCase | `moveInput` |
| 인터페이스 | `I` + PascalCase | `IDamageable` |
| 열거형 타입, 멤버 | PascalCase | `PlayerState.Idle` |
| 이벤트 | `On` + 동사 과거형/명사 | `OnDied`, `OnHealthChanged` |
| 이벤트 핸들러 메서드 | 이벤트·대상 이름 + `Handler` (`On`은 이벤트에만 쓴다) | `DiedHandler()`, `MoveInputHandler()` |
| 코루틴 | 동작 + `Routine` | `CannonFireRoutine()`, `RespawnRoutine()` |
| ScriptableObject 클래스 | `~Data`, `~Config` | `EnemyData`, `AudioConfig` |

클래스 이름 끝말 (역할을 이름으로 구분한다)
| 끝말 | 역할 | 예시 |
|---|---|---|
| `~Manager` | 씬에 하나만 있고 전체를 관리 | `GameManager`, `UiManager` |
| `~Controller` | 특정 오브젝트를 조작 | `PlayerController` |
| `~View` | UI 표시만 담당 | `HpBarView` |
| `~Data`, `~Config` | ScriptableObject 데이터 | `EnemyData` |
- `~Handler`는 이벤트 핸들러 **메서드에만** 쓴다. 클래스 이름에는 쓰지 않는다.

- bool은 질문처럼 읽히게 쓴다: `isGrounded`, `hasKey`, `canJump`
- 메서드는 동사로 시작한다: `Move()`, `ApplyDamage()`, `GetMoveDirection()`
  - 예외: 이벤트 핸들러(`~Handler`), 코루틴(`~Routine`)은 위 표의 형식을 따른다.
- 줄임말은 피한다: `pos` 대신 `position`, `cnt` 대신 `count`. 단, UI, ID, HP처럼 널리 쓰는 말은 허용한다.
- 이름 안의 약어는 한 단어처럼 첫 글자만 대문자로 쓴다: `UiManager`, `PlayerId`, `HpBar`
  - 상수(UPPER_SNAKE_CASE)는 예외: `MAX_HP`
- public 필드는 되도록 쓰지 않는다 (아래 "필드" 참고).

## 적용 범위 (네임스페이스, asmdef)
- 아래 "네임스페이스", "어셈블리 정의" 규칙은 **새로 만드는 스크립트**부터 적용한다.
- 기존 스크립트에 네임스페이스를 붙이거나 asmdef를 새로 만드는 일은, 요청받았을 때만 별도 작업(이슈)으로 한다.
  기존 코드를 고치다가 겸사겸사 바꾸지 않는다 (참조가 대량으로 깨질 수 있다).
- 기존 프로젝트에 asmdef가 하나도 없으면, 새 스크립트도 asmdef 없이 네임스페이스만 붙인다.
  asmdef 없는 코드는 asmdef 안의 코드를 참조할 수 있지만, 반대는 안 되기 때문이다.

## 네임스페이스
- 모든 스크립트에 네임스페이스를 쓴다: `<프로젝트>.<기능>` (예: `Fourbeat.Player`, `Fourbeat.Ui`)
- 네임스페이스는 폴더 구조를 따른다: `Assets/Scripts/Player/` → `Fourbeat.Player`
- 에디터 전용 코드는 `.Editor`를 붙인다: `Fourbeat.Player.Editor`
- 파일 범위 네임스페이스(`namespace Fourbeat.Player;`)를 쓸 수 있다. 한 프로젝트 안에서는 한 가지 방식만 쓴다.

## 어셈블리 정의 (asmdef)
- 스크립트 폴더의 최상위에 asmdef를 둔다. 이름은 네임스페이스와 같게 한다: `Fourbeat.asmdef`
- 기능이 커져서 컴파일 시간이 길어지거나 의존을 분리해야 할 때 기능별 asmdef를 추가한다: `Fourbeat.Player.asmdef`
- 에디터 전용 코드는 별도 asmdef를 만든다.
  - asmdef가 있는 폴더 아래에서는 `Editor` 폴더라도 빌드에서 자동으로 빠지지 않는다.
    상위 asmdef(런타임)에 포함돼서 `UnityEditor`를 쓰는 코드 때문에 빌드가 실패한다.
  - 그래서 asmdef 아래의 `Editor` 폴더마다 Editor 전용 asmdef를 하나씩 둔다.
  - 이름: `<상위 asmdef 이름>.Editor` (예: `Fourbeat.Editor`, `Fourbeat.Player.Editor`). asmdef 이름은 프로젝트 안에서 겹치면 안 된다.
  - 설정: Platforms는 Editor만 선택하고, References에 상위 asmdef를 추가한다.
- 테스트는 별도 asmdef에 둔다: `Fourbeat.Tests.asmdef`
- 참조는 필요한 것만 추가한다. 서로 참조(순환 참조)하지 않는다.
- asmdef를 추가하면 그 폴더 밖의 코드가 이 코드를 쓰려면 참조를 추가해야 한다. 추가·변경하면 PR에 적는다.

## 클래스 내부 순서
1. 상수 (`const`, 상수처럼 쓰는 `static readonly`)
2. static 필드 (public → private)
3. `[SerializeField]` 필드
4. private 필드
5. 프로퍼티, 이벤트
6. 생성자 (일반 C# 클래스), static 생성자
7. Unity 생명주기 메서드 (`Awake` → `OnEnable` → `Start` → `Update` → `FixedUpdate` → `LateUpdate` → `OnDisable` → `OnDestroy`)
8. public 메서드 (static 포함)
9. private 메서드 (static 포함, 이벤트 핸들러와 코루틴도 여기)
10. 중첩 타입 (클래스 안에 선언한 class, struct, enum)

## 필드
- Inspector에 노출할 값은 `public` 대신 `[SerializeField] private`로 쓴다.
- 다른 스크립트에서 읽어야 하면 프로퍼티로 공개한다: `public float MoveAmount { get; private set; }`
- 접근 제한자를 생략하지 않는다: `void Move()`가 아니라 `private void Move()`
- 의미 있는 조절 값은 코드에 숫자로 바로 쓰지 않는다. Inspector에서 조절할 값은 `[SerializeField]`로, 고정 값은 상수로 뺀다.
  ```csharp
  // 나쁨: 4가 무엇인지 알 수 없고 조절할 수 없다
  transform.position += dir * 4f * Time.deltaTime;
  // 좋음
  [SerializeField] private float _moveSpeed = 4f;
  ```
  - 예외: 수식에 쓰는 0, 1, -1, 2, 0.5 등은 그대로 써도 된다. 예: `Mathf.PI * 2f`, `value * 0.5f`, `-2f * gravity`
- Inspector 필드가 많으면 `[Header("...")]`로 묶는다.

## 열거형
- 타입 이름은 단수형으로 쓴다: `PlayerState` (`PlayerStates` 아님)
- 멤버에 타입 이름을 반복하지 않는다: `PlayerState.Idle` (`PlayerState.PlayerIdle` 아님)
- 비트 플래그로 쓸 때만 `[Flags]`를 붙이고 이름을 복수형으로 쓴다. 값은 2의 거듭제곱으로 정하고 `None = 0`을 둔다.
- 저장하거나 직렬화하는 열거형은 값을 명시한다. 중간에 멤버를 추가해도 기존 데이터가 깨지지 않게 하기 위함이다.
  ```csharp
  public enum ItemType
  {
      None = 0,
      Weapon = 1,
      Armor = 2
  }
  ```

## 메서드
- Unity 생명주기 메서드는 한 줄로 쓰고, 실제 처리는 이름 있는 메서드로 분리한다.
  ```csharp
  private void Awake() => CacheComponents();
  private void OnEnable() => BindInputEvents();
  private void OnDisable() => UnbindInputEvents();
  ```
- 메서드 하나는 한 가지 일만 한다. 30줄을 넘으면 나눌 수 있는지 본다.
- 매개변수가 4개를 넘으면 구조체나 클래스로 묶는다.
- 조건이 맞지 않으면 일찍 반환해서 중첩을 줄인다.
  ```csharp
  if (!_controller.isGrounded) return;
  ```

## 이벤트
- 이벤트를 OnEnable에서 구독했다면 OnDisable에서 해제한다.
- 클래스 간 알림은 `event Action<T>`를 쓴다. 외부에서 호출하지 못하게 `event` 키워드를 붙인다.
- 호출할 때 구독자가 없을 수 있으므로 `?.Invoke()`로 호출한다. 이벤트(델리게이트)는 Unity 오브젝트가 아니므로 `?.`를 써도 된다 (아래 "null 처리" 참고).

## 성능
- `GetComponent`, `Camera.main`, `Find` 계열은 `Awake`에서 한 번만 찾아 필드에 저장한다. `Update`에서 호출하지 않는다.
- `Update`에서 참조 타입(클래스, 배열, `List` 등)을 `new`로 만들지 않는다. 필요하면 필드로 만들어 재사용한다.
  `Vector3`, `Quaternion` 같은 구조체는 `new`로 만들어도 된다.
- 태그 비교는 `CompareTag("Player")`를 쓴다 (`tag == "Player"`는 매번 문자열을 만든다).
- 물리 처리는 `FixedUpdate`, 카메라·애니메이션 후처리는 `LateUpdate`에서 한다.
- 자주 생성·삭제하는 오브젝트(총알, 이펙트)는 오브젝트 풀을 쓴다.

## null 처리
- Unity 오브젝트(`GameObject`, `Component`, `ScriptableObject` 등 `UnityEngine.Object`를 상속한 것)에는 `?.`, `??`, `is null`을 쓰지 않는다.
  파괴된 오브젝트를 null로 판단하지 못한다.
  ```csharp
  // 나쁨
  _target?.TakeDamage(1);
  // 좋음
  if (_target != null) _target.TakeDamage(1);
  ```
- 일반 C# 클래스, 델리게이트, 이벤트에는 `?.`, `??`를 써도 된다.
- 반드시 연결돼야 하는 Inspector 참조는 null 체크로 숨기지 말고, 비어 있으면 오류가 나게 둔다. 문제를 빨리 찾기 위함이다.

## 입력
- `InputActionReference`를 Inspector로 받고, `performed` / `canceled` 이벤트에 바인딩한다.
- 구 `Input` 클래스(`Input.GetKey` 등)는 쓰지 않는다.

## 코루틴 · 비동기
어떤 방식을 쓸지 아래 기준으로 고른다. 한 기능 안에서 섞어 쓰지 않는다.

| 방식 | 쓰는 경우 | 사용 여부 |
|---|---|---|
| 코루틴 (`IEnumerator`) | 간단한 시간 대기, 연출 순서 | 사용 |
| `async/await` + Unity `Awaitable` | 결과를 돌려받는 비동기 작업(로딩, 네트워크), 여러 단계 대기 | 사용 (Unity 2023.1 이상) |
| UniTask | `Awaitable`보다 많은 기능이나 성능이 필요할 때 | 프로젝트에 UniTask가 설치돼 있을 때만 사용. 설치는 팀 합의 후 |
| `Task`, `Task.Run` | - | 쓰지 않는다. Unity API는 메인 스레드에서만 호출할 수 있다 |

코루틴
- 메서드 이름 끝에 `Routine`을 붙인다: `CannonFireRoutine()`
- 멈출 수 있어야 하면 `Coroutine` 변수에 저장하고 `StopCoroutine`으로 멈춘다.
- `WaitForSeconds`처럼 반복해서 쓰는 대기 객체는 필드에 캐싱한다. 매번 `new`로 만들지 않는다.
  ```csharp
  [SerializeField] private float _rayShotDelay;
  private WaitForSeconds _waitRayShot;

  private void Awake() => _waitRayShot = new WaitForSeconds(_rayShotDelay);

  private IEnumerator RayShotRoutine()
  {
      while (_isRayActive)
      {
          RayShotToPlayer();
          yield return _waitRayShot;
      }
  }
  ```
- 오브젝트가 비활성화되면 코루틴도 멈춘다는 점을 고려한다.

async/await
- 메서드 이름 끝에 `Async`를 붙인다: `LoadStageAsync()`
- `async void`는 쓰지 않는다. 예외를 잡을 수 없다. 이벤트 핸들러처럼 어쩔 수 없을 때만 쓰고, 안에서 try/catch로 감싼다.
- 오브젝트가 파괴되면 작업이 멈추도록 `destroyCancellationToken`을 넘긴다.
  ```csharp
  private async Awaitable LoadStageAsync()
  {
      await Awaitable.WaitForSecondsAsync(1f, destroyCancellationToken);
  }
  ```

## 서식
- 중괄호는 새 줄에 쓴다 (Allman 스타일).
- 한 줄짜리 `if`는 중괄호 없이 한 줄로 쓸 수 있다. 두 줄 이상이면 중괄호를 쓴다.
- 들여쓰기는 공백 4칸.
- `var`는 오른쪽에서 타입이 분명할 때만 쓴다: `var list = new List<int>();`
- 사용하지 않는 `using`은 지운다.

## 파일과 폴더
- 파일 하나에 클래스 하나. 파일 이름과 클래스 이름을 같게 한다.
- 스크립트는 `Assets/Scripts/<기능>/`에 둔다.
- 에디터 전용 코드는 `Editor` 폴더에 둔다. 빌드에 포함되지 않게 하기 위함이다.
  - asmdef가 없는 곳: `Editor` 폴더만으로 빌드에서 빠진다.
  - asmdef가 있는 곳: `Editor` 폴더에 Editor 전용 asmdef도 함께 둬야 빠진다 (위 "어셈블리 정의" 참고).
- 머티리얼은 `Assets/Materials/<기능>/`에 둔다.

## 주석
- 코드에 나오는 이름과 용어(클래스, 메서드, 변수 이름, Unity 용어)는 영어 그대로 쓰고, 설명은 한국어로 쓴다.
  ```csharp
  // Jump는 isGrounded일 때만 허용한다. 공중 점프를 막기 위함
  ```
- 코드가 무엇을 하는지가 아니라, 왜 그렇게 했는지를 적는다.
- 사용하지 않는 코드는 주석 처리하지 말고 지운다. 이 규칙은 지금 작성하거나 수정하는 코드에만 적용한다.
  관련 없는 곳에서 발견한 죽은 코드는 먼저 알리고, 사용자가 지우라고 하면 지운다.
- `Debug.Log`는 작업이 끝나면 지운다. 남겨야 하면 이유를 주석으로 적는다.
- 할 일은 `// TODO: 내용` 형식으로 쓴다.
