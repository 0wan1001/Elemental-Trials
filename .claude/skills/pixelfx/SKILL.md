---
name: pixelfx
description: Elemental Trials에서 pixelfx(코드로 그리는 픽셀 이펙트 라이브러리, 고정 버전)로 이펙트를 찾고, 렌더하고, 스프라이트 시트(PNG+JSON)로 뽑아 Unity로 가져온다. "이펙트 뽑아줘", "낙뢰 이펙트 가져와", "pixelfx", "스프라이트 시트", 픽셀 VFX 요청에 사용.
---

# pixelfx (Elemental Trials)

pixelfx 원본 스킬을 이 프로젝트의 고정 버전과 Unity 가져오기 흐름에 맞춘 안내다.
원본 스킬(이펙트 찾기, 옵션, 새 이펙트 만들기)은 1단계에서 받는 체크아웃 안에 있다.

## 1. 준비 (매 세션 처음 한 번)

1. `node Tools/pixelfx-setup.mjs` 를 실행한다. 고정 커밋을 `External/pixelfx/`에 받는다 (git 무시 폴더, 여러 번 실행해도 안전).
   - Node 18 이상이 필요하다. 없으면 사용자에게 알린다.
   - 버전은 스크립트의 `PIXELFX_COMMIT`으로 고정한다. 사용자가 요청할 때만 올리고, 올리면 `THIRD_PARTY_NOTICES.md`의 커밋도 같이 고친다.
2. 원본 스킬을 읽는다: `External/pixelfx/skills/pixelfx/SKILL.md`
   - 원본의 "Before anything" 중 `git clone` 단계는 건너뛴다. 1번이 대신한다.
   - pixelfx 도구(`tools/*.mjs`)는 `External/pixelfx/` 안에서 실행한다.

## 2. 이펙트 찾기와 확인

- 검색: `node tools/list.mjs <단어>` (예: `lightning`, `explosion`, `steam`)
- 렌더: `node tools/render.mjs <id>` → `records/<id>/contact.png`를 이미지로 열어 직접 본다.
- 옵션 조정은 코드 수정 없이 `--opts '{"palette": [...], "seed": 3}'`로 먼저 해 본다.
- 크기 주의: 게임 화면은 320 × 180 px다. pixelfx 캔버스는 대부분 이보다 크니 실제로 그려지는 범위를 contact.png로 확인한다.

## 3. Unity로 가져오기

에디터 메뉴 `Tools > pixelfx` (스크립트: `Assets/Editor/Pixelfx/PixelfxImporter.cs`)

| 메뉴 | 하는 일 |
|---|---|
| `Render && Import...` | 이펙트 id를 입력받아 `render.mjs <id> --sheet`를 실행하고 바로 가져온다 |
| `Import Sheet...` | 이미 렌더한 PNG+JSON을 골라 가져온다 |

에이전트는 unity CLI로 같은 기능을 호출한다 (에디터가 켜져 있을 때):
```
unity command script eval --caller plugin --skill pixelfx --code 'return ElementalTrials.Pixelfx.Editor.PixelfxImporter.RenderAndImport("lightning/lightning-strike", null);'
```
- 두 번째 인자는 `--opts` JSON 문자열이다 (없으면 `null`). 세 번째 인자(선택)는 렌더 fps다 (예: `12`, 0이면 기본값).
- 도트 이펙트는 12~15 fps면 충분하다. 30 fps 그대로 뽑으면 시트가 두 배로 커진다.

가져올 때 시트를 다시 배치한다 (Unity 메모리 최적화).
- 투명도 16/255 미만 픽셀을 지우고, 모든 프레임에서 그림이 있는 영역만 남긴다.
- 끝의 빈 프레임을 지운다.
- 프레임 크기와 시트 크기를 2의 거듭제곱으로 맞춘다 (예: 136×56 → 128×64, 시트 512×512).
- 결과 문자열의 `content`(실제 그림 크기)가 256을 조금 넘으면 셀이 512로 커진다. 이때는 `size` 옵션이나 fps를 줄여 본다.

가져오면 `Assets/VFX/pixelfx/<이름>/`에 아래가 생긴다.
- `<이름>.png`, `<이름>.json`: 다시 배치한 시트와 그 프레임 정보 (JSON은 기록용)
- 텍스처 설정: PPU 16, Point 필터, 압축 없음, 프레임마다 스프라이트로 잘림, pivot은 `meta.pixelfx.pivot`
- `<이름>.anim`: `meta.pixelfx.fps`, `loop` 반영
- `<이름>.controller`: 이 클립 하나를 재생하는 Animator Controller

## 이 프로젝트의 규칙

- pixelfx는 "한 자리에서 터지는 이펙트"만 담당한다: 불덩이 폭발, 낙뢰, 물결 튀김, 증기 퍼프, 감전 스파크, 과부하 폭발.
- 번개 전도 경로, 바닥 상태 표시, 히트 플래시, 흔들림, 데미지 숫자는 pixelfx로 만들지 않는다 (코드로 직접 구현).
- Patch Pixel 폰트는 쓰지 않는다. 체크아웃에서 `gallery/`를 받지 않는다. 폰트 파일을 Assets에 넣지 않는다.
- 이펙트 적용(게임에 붙이기)은 3주차 일정이다. 그 전에는 사용자가 요청할 때만 뽑는다.
- 이펙트를 재생하는 게임플레이 코드(풀링, 스폰 시점)는 사용자가 직접 작성한다. 이 스킬은 에셋까지만 만든다.
- 새 이펙트를 추가하면 `THIRD_PARTY_NOTICES.md`는 그대로 두면 된다 (같은 pixelfx 버전이면 고지는 하나).
