# 라벨 목록

이슈와 PR에 붙이는 라벨. 저장소에 없는 라벨은 Claude가 만들지 않는다. 처음 저장소를 만들 때 아래 라벨을 만들어 둔다.

## 종류 라벨 (커밋 타입과 같음)
| 라벨 | 용도 | 이슈 템플릿 | 색 (예시) |
|---|---|---|---|
| `feat` | 새 기능 | 기능 요청 | `#1D76DB` |
| `fix` | 버그 수정 | 버그 리포트 | `#D73A4A` |
| `refactor` | 동작 변화 없는 코드 개선 | | `#FBCA04` |
| `docs` | 문서 | | `#0075CA` |
| `chore` | 설정, 빌드, 기타 작업 | 작업 | `#CFD3D7` |
| `asset` | 씬, 프리팹, 리소스만 바꾼 경우 | | `#7057FF` |

## PR에 붙이는 라벨
| PR 종류 | 라벨 |
|---|---|
| 작업 PR | 제목의 타입과 같은 라벨. 예: `[feat] 플레이어 점프 추가` → `feat` |
| 릴리스 PR (`develop` → `prod`) | `release` |

## 기타 라벨
| 라벨 | 용도 | 색 (예시) |
|---|---|---|
| `release` | 릴리스 PR | `#0E8A16` |

## 추가 라벨 (직접 기입)
| 라벨 | 용도 | 색 |
|---|---|---|
| | | |

## 라벨 만들기 (저장소마다 한 번)
```bash
gh label create feat --color 1D76DB --description "새 기능"
gh label create fix --color D73A4A --description "버그 수정"
gh label create refactor --color FBCA04 --description "동작 변화 없는 코드 개선"
gh label create docs --color 0075CA --description "문서"
gh label create chore --color CFD3D7 --description "설정, 빌드, 기타 작업"
gh label create asset --color 7057FF --description "씬, 프리팹, 리소스"
gh label create release --color 0E8A16 --description "릴리스 PR"
```
