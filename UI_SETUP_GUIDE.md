# UIManager UI 요소 자동 생성 가이드

## 📋 개요

`UIManagerSetupEditor.cs` 스크립트는 UIManager에 필요한 모든 UI 요소들을 **자동으로 생성하고 연결**하는 Unity Editor 도구입니다.

## 🚀 사용 방법

### 1단계: Unity Editor 열기
1. Unity Hub에서 ShelfSimm_Unity 프로젝트 열기
2. MainScene.unity 열기 (Assets/Scenes/MainScene.unity)

### 2단계: Editor Script 실행
1. Unity 상단 메뉴에서 **Tools → Setup UIManager UI Elements** 클릭
2. Console 창에서 결과 확인:
   ```
   [UIManagerSetup] UIManager UI 요소 생성 시작...
   [UIManagerSetup] ✅ UIManager UI 요소 생성 및 연결 완료!
     - SummaryPanel: SummaryPanel
     - ErrorPanel: UIErrorPanel
     - CompletedCountText: CompletedCountText
   ```

### 3단계: 결과 확인
Hierarchy 창에서 Canvas 하위에 다음 요소들이 생성되었는지 확인:
- ✅ **SummaryPanel** (비활성화 상태)
  - ContentPanel
    - TitleText ("시뮬레이션 결과")
    - SummaryText (빈 텍스트)
    - CloseButton ("닫기")
- ✅ **UIErrorPanel** (비활성화 상태)
  - ContentPanel
    - TitleText ("오류 발생")
    - ErrorText (빈 텍스트)
    - CloseButton ("확인")
- ✅ **CompletedCountText** (OverlayUI 또는 SafeArea 하위)

### 4단계: UIManager 연결 확인
1. Hierarchy에서 Canvas 선택
2. Inspector에서 UIManager 컴포넌트 확인
3. 다음 필드들이 자동으로 연결되어 있는지 확인:
   - Summary Panel → SummaryPanel
   - Summary Text → SummaryText
   - Completed Count Text → CompletedCountText
   - Error Panel → UIErrorPanel
   - Error Text → ErrorText

### 5단계: Scene 저장
- Ctrl+S (Windows/Linux) 또는 Cmd+S (macOS)로 Scene 저장

## 🎨 생성된 UI 구조

### SummaryPanel
- **위치**: Canvas 하위 (전체 화면)
- **배경**: 반투명 검정 (0, 0, 0, 0.7)
- **크기**: 전체 화면
- **ContentPanel**:
  - 크기: 800x600
  - 위치: 화면 중앙
  - 배경: 흰색
- **SummaryText**:
  - 폰트 크기: 20
  - 정렬: 왼쪽 상단
  - 자동 줄바꿈: 활성화
- **CloseButton**:
  - 크기: 200x50
  - 위치: 하단 중앙
  - 배경: 파란색
  - 클릭 이벤트: `UIManager.CloseSummary()`

### UIErrorPanel
- **위치**: Canvas 하위 (전체 화면)
- **배경**: 반투명 어두운 빨강 (0.3, 0, 0, 0.7)
- **크기**: 전체 화면
- **ContentPanel**:
  - 크기: 600x400
  - 위치: 화면 중앙
  - 배경: 흰색
- **ErrorText**:
  - 폰트 크기: 18
  - 정렬: 왼쪽 상단
  - 자동 줄바꿈: 활성화
- **CloseButton**:
  - 크기: 200x50
  - 위치: 하단 중앙
  - 배경: 빨간색
  - 클릭 이벤트: `UIManager.CloseError()`

### CompletedCountText
- **위치**: OverlayUI 또는 SafeArea 하위
- **위치**: 화면 좌상단 (160, -30)
- **크기**: 300x50
- **폰트 크기**: 20
- **색상**: 흰색 (외곽선: 검정)
- **기본 텍스트**: "완료 건수: 0"

## 🔧 커스터마이징

생성된 UI 요소들을 수정하려면:

1. **색상 변경**:
   - Hierarchy에서 Panel 선택
   - Inspector → Image → Color 수정

2. **크기 변경**:
   - RectTransform → Size Delta 수정

3. **위치 변경**:
   - RectTransform → Anchored Position 수정

4. **폰트 변경**:
   - TextMeshProUGUI → Font Asset 수정

## ⚠️ 주의사항

1. **기존 요소 삭제**: 스크립트 실행 시 기존의 SummaryPanel, UIErrorPanel, CompletedCountText는 자동으로 삭제됩니다.

2. **백업**: 만약의 경우를 대비하여 Scene 파일 백업이 생성되어 있습니다:
   - 위치: `Assets/Scenes/MainScene.unity.backup`

3. **재실행**: 필요시 언제든지 메뉴에서 다시 실행 가능합니다.

4. **Canvas 필수**: Scene에 Canvas가 반드시 있어야 합니다.

5. **UIManager 필수**: Scene에 UIManager 컴포넌트가 있어야 합니다.

## 🐛 문제 해결

### "Canvas를 찾을 수 없습니다" 에러
- Scene에 Canvas가 있는지 확인
- Canvas가 활성화되어 있는지 확인

### "UIManager를 찾을 수 없습니다" 에러
- Canvas GameObject에 UIManager 컴포넌트가 있는지 확인
- UIManager 스크립트가 컴파일 에러 없이 정상인지 확인

### UI가 보이지 않음
- Panel들은 초기에 **비활성화** 상태입니다
- Play 모드에서 시뮬레이션 완료 시 자동으로 표시됩니다
- 테스트하려면 Hierarchy에서 수동으로 활성화할 수 있습니다

### 버튼이 동작하지 않음
- UIManager의 CloseSummary() 또는 CloseError() 메서드가 public인지 확인
- Button의 OnClick 이벤트가 제대로 연결되었는지 Inspector에서 확인

## 📝 스크립트 위치

- **Editor Script**: `/Assets/Scripts/Editor/UIManagerSetupEditor.cs`
- **UIManager**: `/Assets/Scripts/Managers/UIManager.cs`
- **MainScene**: `/Assets/Scenes/MainScene.unity`

## ✅ 완료 체크리스트

- [ ] Unity Editor 열기
- [ ] MainScene.unity 열기
- [ ] Tools → Setup UIManager UI Elements 실행
- [ ] Console 창에서 성공 메시지 확인
- [ ] Hierarchy에서 생성된 UI 요소 확인
- [ ] UIManager Inspector에서 연결 상태 확인
- [ ] Scene 저장 (Ctrl+S / Cmd+S)
- [ ] Play 모드로 테스트

---

**작성일**: 2025-11-18
**버전**: 1.0
**관련 태스크**: Unity Editor UI Panels 설정
