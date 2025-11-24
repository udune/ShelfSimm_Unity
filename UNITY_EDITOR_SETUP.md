# Unity Editor 설정 가이드

## 필수 작업 순서

### 1. Manager GameObject 설정
**메뉴**: `Tools → ShelfSim → Setup Managers`

이 도구는 다음 Manager GameObject들을 자동으로 생성/확인합니다:
- Managers (부모 GameObject)
  - SimulationManager
  - BookRegistry
  - LayoutHashManager
  - CellHighlightManager
  - PathCache
  - APIManager
  - CodeManager (CodeValidator + CodeRegistry 컴포넌트 포함)

**주의**: 이 작업을 가장 먼저 실행하세요!

---

### 2. Scene 정리
**메뉴**: `Tools → ShelfSim → Clean Scene UI`

기존 UI 오브젝트들을 삭제합니다. Grid View는 유지됩니다.

---

### 3. 새 UI 생성
**메뉴**: `Tools → ShelfSim → Setup Simple UI`

간단하고 안정적인 UI를 자동으로 생성합니다:
- 좌측 60%: Grid View 영역 (로봇 시뮬레이션)
- 우측 40%: 제어 패널
  - 작업 입력 패널 (상단)
  - 작업 목록 패널 (중간)
  - 시뮬레이션 상태 패널 (하단)

---

### 4. SimulationManager 설정 확인

Hierarchy에서 `Managers → SimulationManager`를 선택하고 Inspector에서 다음을 확인:

#### 핵심 설정
- **Config**: `SimulationConfig` ScriptableObject 할당
- **Use Api Mode**: API 사용 여부 체크

#### 내부 컴포넌트 참조
- **Robot Controller**: `Managers → SimulationManager` (자기 자신에 RobotController 컴포넌트 있어야 함)
- **Api Client**: `Managers → APIManager` 할당
- **Path Finder**: `AStarPathFinder` GameObject 할당
- **Cells Layout**: `CellsLayoutSO` ScriptableObject 할당

#### 임시 데이터
- **All Cells**: 비워두기 (런타임에 자동 생성)
- **All Books**: 비워두기 (API에서 자동 로드)

---

### 5. BookRegistry 설정 확인

Hierarchy에서 `Managers → BookRegistry`를 선택하고 Inspector에서 확인:
- **Use Api Data**: 체크 (API에서 책 정보 로드)

---

### 6. AStarPathFinder 설정 확인

Hierarchy에서 `AStarPathFinder`를 선택하고 Inspector에서 확인:
- **Grid Width**: 50 (그리드 너비)
- **Grid Height**: 50 (그리드 높이)

---

### 7. NearestSelector 설정 확인

Hierarchy에서 `NearestSelector`를 선택하고 Inspector에서 확인:
- **Top N**: 3 (가장 가까운 N개 셀 선택)
- **Path Finder**: `AStarPathFinder` GameObject 할당
- **Tiebreaker Config**: `TiebreakerConfig` ScriptableObject 할당

---

### 8. Grid View 설정 (선택사항)

Grid View 관련 GameObject들이 Scene에 있다면:

#### GridRenderer
- **Grid Image**: RawImage 컴포넌트 할당
- **Cell Size**: 10 (픽셀 단위)

#### GridClickHandler
- **Grid Renderer**: GridRenderer 컴포넌트 할당
- **Grid Image**: RawImage 컴포넌트 할당
- **Cell Size**: 10
- **Info Panel**: CellInfoPanel GameObject 할당
- **Highlight Manager**: CellHighlightManager 할당

---

## API 설정 (useApiMode = true인 경우)

### APIManager (ApiClient) 설정

Hierarchy에서 `Managers → APIManager`를 선택하고 Inspector에서:
- **Base URL**: 백엔드 API URL 입력 (예: `http://localhost:8000/api`)

### 테스트 방법

1. Play 모드 진입
2. Console에서 다음 메시지 확인:
   ```
   API 모드 초기화 중...
   책 정보 로드 성공
   JobInputController book dropdown 업데이트 완료
   API 초기화 완료. 책 정보 로드 완료.
   ```

---

## 주요 변경사항

### ✅ 완료된 작업

1. **API 책 목록 dropdown 연동**
   - API에서 책을 로드하면 자동으로 JobInputController의 dropdown 업데이트
   - `SimulationManager.InitializeAPI()` → `BookRegistry.LoadBooksFromApi()` → `JobInputController.RefreshBookDropdown()`

2. **불필요한 스크립트 파일 정리**
   - 다음 파일들이 `.backup_unused` 폴더로 이동됨:
     - BookDropdownController.cs
     - CodeInputHighlighter.cs
     - CellView.cs
     - CellRegistry.cs

3. **Grid Renderer 확인**
   - GridRenderer.cs: 정상 작동
   - GridClickHandler.cs: 정상 작동
   - CellInfoPanel.cs: 정상 작동

4. **전체 버그 체크**
   - TODO 주석 1개 발견 (ApiClient.cs 52줄 - 서버 API 수정 대기 중)
   - Null reference 위험 없음

---

## 문제 해결

### Manager GameObject들이 없는 경우
→ `Tools → ShelfSim → Setup Managers` 실행

### UI가 제대로 표시되지 않는 경우
1. `Tools → ShelfSim → Clean Scene UI` 실행
2. `Tools → ShelfSim → Setup Simple UI` 실행

### API 연결 실패
1. APIManager의 Base URL 확인
2. 백엔드 서버가 실행 중인지 확인
3. Console에서 에러 메시지 확인

### Dropdown에 책 목록이 안 나오는 경우
1. BookRegistry에서 `Use Api Data` 체크 확인
2. SimulationManager에서 `Use Api Mode` 체크 확인
3. Play 모드에서 Console 로그 확인

---

## 주요 API

### JobInputController
- `RefreshBookDropdown()`: API에서 책을 로드한 후 dropdown 업데이트

### BookRegistry
- `LoadBooksFromApi(List<BookDto>)`: API에서 받은 책 목록 로드
- `GetAllAvailableBooks()`: 모든 사용 가능한 책 목록 반환

### SimulationManager
- `InitializeAPI()`: API 초기화 및 책 정보 로드
- `PrepareSimulation(List<Job>)`: 시뮬레이션 준비
- `StartSimulation()`: 시뮬레이션 시작

---

## 버그 및 개선사항

### 알려진 이슈
- ApiClient.cs 52줄: JobsBatchResponse.jobIds 필드가 주석 처리됨
  - 서버 API가 Job ID 목록을 반환하도록 수정되면 활성화 필요

### 개선 제안
- UI 레이아웃을 더 세밀하게 조정 가능
- Grid View와 제어 패널 비율 조정 가능 (현재 60:40)

---

**작성일**: 2025-11-22
**버전**: v1.0
**작성자**: Claude AI Assistant
