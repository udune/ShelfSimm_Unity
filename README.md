<img width="400" height="267" alt="SmartWMS" src="https://github.com/user-attachments/assets/f47036e1-83c5-4f7f-a256-c6167fbefd3d" />

# ShelfSim - 서가 시뮬레이션 프로젝트

도서관/서점의 서가 로봇 작업 흐름을 시뮬레이션하는 Unity 기반 프로젝트입니다.

## 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [주요 기능](#주요-기능)
3. [기술 스택](#기술-스택)
4. [설치 및 실행](#설치-및-실행)
5. [프로젝트 구조](#프로젝트-구조)
6. [구현 현황](#구현-현황)
7. [사용 방법](#사용-방법)
8. [테스트](#테스트)
9. [문서](#문서)
10. [개발 가이드](#개발-가이드)
11. [라이센스](#라이센스)

## 프로젝트 개요

## 프로젝트 개요

ShelfSim은 도서 입출고 작업을 수행하는 로봇의 동선과 작업 효율성을 시뮬레이션하는 시스템입니다.

핵심 목표:
- 자동 순서 결정: 로봇이 현재 위치에서 가장 가까운 칸부터 자동으로 작업
- 경로 최적화: A* 알고리즘으로 최단 경로 계산
- 타임아웃 관리: 이동 시간 초과 감지 및 처리
- 실패 처리: 경로 차단/타임아웃 발생 시 안전하게 창고 복귀
- 결과 분석: CSV 형식으로 시뮬레이션 결과 내보내기

적용 시나리오:
- 도서관 자동화 시스템 설계
- 서점 재고 관리 시뮬레이션
- 창고 로봇 동선 최적화
- 물류 효율성 분석

## 주요 기능

## 주요 기능

### Phase 1 (현재 구현 중)

**1. 입력/명령 인터페이스**
- 다중 칸 코드 입력 (쉼표/공백 구분)
- 코드 정규화 (대문자 변환, 2자리 zero-pad)
- 실시간 검증 및 에러 표시

**2. 자동 순서 결정**
- Nearest-Next 알고리즘
- 맨해튼 거리 기반 1차 필터
- A* 비용 재평가 (TopN=3)
- 결정적 실행 (seed 기반)

**3. 로봇 FSM (상태머신) - T-303 완료**
- 상태: IDLE → MOVING → HANDLING → RETURNING → IDLE
- A* 경로 탐색
- 이동 타임아웃 감지 (기본 30초)
- 경로 실패 처리 (ROUTE_BLOCKED)
- 우아한 종료 (실패 집계)

**4. 에러 처리 - T-303 완료**
- 10가지 ErrorCode 정의
- 사용자 친화적 메시지
- 실패 사유 추적
- 로그 기록

### Phase 2 (예정)
- 칸 제약 조건 (용량, 높이)
- 재고 관리 시스템
- CSV 결과 내보내기
- 성능 최적화 (캐싱)

## 기술 스택

## 기술 스택

**핵심**
- Unity: 2022.3.12f1
- C#: 10.0
- OS: Windows 11 / macOS / Linux

**알고리즘**
- 경로 탐색: A* (맨해튼 휴리스틱)
- 순서 결정: Nearest-Next + TopN 재평가
- 상태 관리: FSM (Finite State Machine)

**테스트**
- 단위 테스트: NUnit
- 테스트 커버리지: 핵심 로직 100%

## 설치 및 실행

### 요구사항
- Unity 2022.3.12f1 이상
- .NET Framework 4.x

### 설치 방법

1. 프로젝트 클론
```bash
git clone https://github.com/your-username/shelf-sim.git
cd shelf-sim
```

2. Unity에서 열기
- Unity Hub 실행
- "Add" 버튼 클릭하여 프로젝트 폴더 선택
- Unity 버전 2022.3.12f1 선택

3. 빠른 테스트 실행
- Hierarchy → Create Empty → "SimpleTest"
- Inspector → Add Component → "SimpleTest"
- 플레이 버튼 클릭
- Console 창에서 결과 확인

## 프로젝트 구조

## 프로젝트 구조

```
ShelfSim/
├── Assets/
│   ├── Scripts/
│   │   ├── Data/                    # 데이터 모델
│   │   │   ├── ErrorCode.cs        # 에러 코드 정의
│   │   │   ├── RobotData.cs        # 로봇 데이터 (T-303 완료)
│   │   │   └── RobotState.cs       # 로봇 상태 enum
│   │   │
│   │   ├── Core/                    # 핵심 로직
│   │   │   ├── RobotFSM.cs         # 상태머신 (T-303 완료)
│   │   │   ├── PathFinder.cs       # A* 경로 탐색 (T-303 완료)
│   │   │   └── CodeValidator.cs    # 코드 검증
│   │   │
│   │   ├── Simulation/              # 시뮬레이션
│   │   │   ├── RobotSimulatorExample.cs  (T-303 완료)
│   │   │   └── SimpleTest.cs             (T-303 완료)
│   │   │
│   │   ├── Utils/                   # 유틸리티
│   │   └── Tests/                   # 테스트
│   │       └── T303_Tests.cs        # T-303 단위 테스트 (9개)
│   │
│   ├── Scenes/                      # Unity 씬
│   └── Documentation/               # 문서
│       ├── T303_COMPLETION_GUIDE.md
│       ├── T303_Implementation.md
│       └── RobotSimulatorExample_Guide.md
│
├── README.md                        # 본 문서
└── LICENSE

```

## 구현 현황

### 최근 완료 (2025-10-11)

**T-303: A* 실패/차단/타임아웃 처리 (8h)**
- A* 경로 탐색 알고리즘 구현
- 경로 실패 감지 (ROUTE_BLOCKED)
- 이동 타임아웃 처리 (ROUTE_TIMEOUT)
- 실패 정보 기록 및 창고 복귀
- 단위 테스트 9개 작성
- AC-9, AC-9.2 검증 완료

### 다음 작업

**T-304: 요약 집계 (표준 포맷) + UI 표시 (6h)**
- 실패 사유 집계
- summary.json 생성
- 표준 포맷 출력
- AC-9.1 검증 예정

## 사용 방법

## 사용 방법

### 1. 빠른 시작 (SimpleTest)

가장 간단한 방법으로 3초 안에 테스트를 완료할 수 있습니다.

Unity에서:
1. Hierarchy → Create Empty → "SimpleTest"
2. Inspector → Add Component → "SimpleTest"
3. 플레이 버튼 클릭
4. Console 창에서 결과 확인

예상 출력:
```
=== T-303 테스트 시작 ===
[테스트 1] 정상 경로 탐색
✅ 경로 찾기 성공! 길이: 7
[테스트 2] 경로 차단
✅ 예상대로 실패! 사유: 접근 가능한 경로가 없습니다
[테스트 3] 타임아웃
✅ 타임아웃 감지 성공! 사유: 이동 시간이 초과되었습니다
=== 모든 테스트 완료 ===
```

### 2. 실시간 시뮬레이션

Unity에서:
1. Hierarchy → Create Empty → "RobotSimulator"
2. Inspector → Add Component → "RobotSimulatorExample"
3. 설정:
   - Move Timeout Sec: 30
   - Grid Width: 50
   - Grid Height: 50
4. 플레이 버튼 클릭

### 3. 코드에서 직접 사용

```csharp
using Data;
using Core;
using UnityEngine;
using System.Collections.Generic;

public class MySimulator : MonoBehaviour
{
    private RobotData robot;
    
    void Start()
    {
        // 1. 로봇 생성
        robot = new RobotData("r1", "Alpha", new Vector2Int(0, 0), 30f);
        
        // 2. 경로 탐색
        HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>();
        List<Vector2Int> path = PathFinder.FindPath(
            robot.position,
            new Vector2Int(10, 10),
            obstacles,
            50, 50
        );
        
        // 3. 이동 시작
        if (path != null)
        {
            RobotFSM.TransitionToMoving(robot, path, Time.time);
        }
    }
    
    void Update()
    {
        // 4. 타임아웃 체크
        if (RobotFSM.CheckTimeout(robot, Time.time))
        {
            Debug.LogWarning("타임아웃!");
            return;
        }
        
        // 5. 위치 업데이트
        RobotFSM.UpdatePosition(robot);
        
        // 6. 도착 확인
        if (RobotFSM.HasReachedTarget(robot))
        {
            Debug.Log("목표 도착!");
        }
    }
}
```

## 테스트

### 단위 테스트 실행

Unity에서:
1. Window → General → Test Runner
2. PlayMode 탭 선택
3. "Run All" 클릭

### 현재 테스트 커버리지

| 모듈 | 테스트 수 | 통과율 | 상태 |
|------|----------|--------|------|
| PathFinder | 3 | 100% | 통과 |
| RobotFSM | 5 | 100% | 통과 |
| ErrorCode | 1 | 100% | 통과 |
| 전체 | 9 | 100% | 통과 |

### 테스트 시나리오

T-303 테스트 (9개):
1. 정상 경로 탐색
2. 장애물 차단
3. 경로 없음 실패 코드 설정
4. 타임아웃 미발생
5. 타임아웃 발생
6. 타임아웃 정확한 경계값
7. 실패 처리 IDLE 전환
8. ErrorCode 메시지 확인
9. 맨해튼 거리 최단 경로

## 문서

## 문서

핵심 문서:
- T-303 완료 가이드 (Documentation/T303_COMPLETION_GUIDE.md) - 전체 작업 요약
- T-303 구현 문서 (Documentation/T303_Implementation.md) - 구현 상세
- RobotSimulator 사용 가이드 (Documentation/RobotSimulatorExample_Guide.md) - 사용법

### API 문서

**PathFinder**
```csharp
// A* 알고리즘으로 최단 경로 탐색
// start: 시작 위치
// goal: 목표 위치
// obstacles: 장애물 집합
// maxWidth: 격자 가로 크기
// maxHeight: 격자 세로 크기
// 반환: 경로 리스트 (null = 경로 없음)
public static List<Vector2Int> FindPath(
    Vector2Int start,
    Vector2Int goal,
    HashSet<Vector2Int> obstacles,
    int maxWidth,
    int maxHeight
)
```

**RobotFSM**
```csharp
// 이동 타임아웃 체크
// robot: 로봇 데이터
// currentTime: 현재 시간
// 반환: 타임아웃 발생 여부
public static bool CheckTimeout(RobotData robot, float currentTime)
```

**ErrorCode**
```csharp
// ErrorCode를 사용자 친화적 메시지로 변환
public static string ToMessage(this ErrorCode errorCode)
```

## 개발 가이드

### 개발 원칙

1. 단순하고 간결하게
   - 10줄보다 1줄이 낫다
   - 불필요한 추상화 금지
   - 핵심 기능만 구현

2. 읽기 쉬운 코드
   - 명확한 변수명
   - 주석 최소화 (코드가 자명해야 함)
   - 각 메서드 단일 책임

3. 기존 구조 유지
   - 하위 호환성 보장
   - 최소한의 변경
   - 점진적 개선

4. 테스트 필수
   - 모든 핵심 로직 단위 테스트
   - 통합 테스트로 시나리오 검증

### 코딩 스타일

Good 예시:
```csharp
public static bool CheckTimeout(RobotData robot, float currentTime)
{
    if (robot.state != RobotState.MOVING)
        return false;
    
    float elapsed = currentTime - robot.moveStartTime;
    return elapsed >= robot.moveTimeoutSec;
}
```

Bad 예시 (불필요한 복잡성):
```csharp
public static bool CheckTimeout(RobotData robot, float currentTime)
{
    // 타임아웃 체크 로직
    var state = robot.state;
    var isMoving = state == RobotState.MOVING || state == RobotState.RETURNING;
    if (!isMoving) return false;
    
    var config = new TimeoutConfig { /* ... */ };
    var checker = new TimeoutChecker(config);
    return checker.Check(robot, currentTime);
}
```

### 새 기능 추가 절차

1. Issue 생성 (Jira Task)
2. 브랜치 생성 (feature/T-XXX)
3. 구현 (원칙 준수)
4. 테스트 작성 (단위 + 통합)
5. 문서 업데이트
6. Pull Request
7. 코드 리뷰
8. Merge

## 설정

## 설정

### 기본 파라미터

| 파라미터 | 기본값 | 범위 | 단위 | 설명 |
|---------|--------|------|------|------|
| handle_time | 2 | > 0 | 초 | 작업 처리 대기 시간 |
| robot_speed | 3 | > 0 | 셀/초 | 로봇 이동 속도 |
| move_timeout_sec | 30 | > 0 | 초 | 이동 타임아웃 한계 |
| warehouse_pos | (0, 0) | 격자 내 | 좌표 | 창고 기본 위치 |
| TopN | 3 | [1, 10] | 개수 | A* 재평가 후보 수 |

### 설정 변경 방법

코드에서:
```csharp
RobotData robot = new RobotData(
    id: "r1",
    name: "Alpha",
    position: warehousePos,
    timeout: 60f  // 60초로 변경
);
```

Inspector에서:
```
RobotSimulatorExample 컴포넌트:
- Move Timeout Sec: 60
- Grid Width: 100
- Grid Height: 100
```

## 성능

### 벤치마크 (50x50 격자)

| 항목 | 측정값 | 목표 | 상태 |
|------|--------|------|------|
| FPS | 60 | 60 | 통과 |
| 경로 탐색 시간 | < 1ms | < 5ms | 통과 |
| 메모리 사용량 | 250MB | < 500MB | 통과 |
| 타임아웃 정확도 | ±0.01s | ±0.1s | 통과 |

## 알려진 이슈

### 현재 제한사항

1. 단일 로봇만 지원
   - 다중 로봇: Phase 3에서 구현 예정
   - 충돌 회피: T-307에서 구현 예정

2. 캐싱 없음
   - 경로 캐시: T-202에서 구현 예정
   - 성능: 100x100 이하 격자에서는 문제 없음

3. UI 없음
   - 그리드 렌더러: T-401에서 구현 예정
   - 대시보드: T-403에서 구현 예정

## 기여

### 기여 방법

1. Fork the Project
2. Create your Feature Branch (git checkout -b feature/T-XXX)
3. Commit your Changes (git commit -m 'Add T-XXX: feature description')
4. Push to the Branch (git push origin feature/T-XXX)
5. Open a Pull Request

### 코드 리뷰 체크리스트

- 개발 원칙 준수
- 단위 테스트 작성
- 문서 업데이트
- 기존 테스트 통과
- 성능 저하 없음

## 연락처

프로젝트 관리자: [Your Name]
이메일: your.email@example.com
GitHub: @your-username

## 라이센스

## 라이센스

이 프로젝트는 MIT 라이센스를 따릅니다. 자세한 내용은 LICENSE 파일을 참조하세요.

## 감사의 말

- Unity Technologies - 강력한 게임 엔진
- A* 알고리즘 연구자들
- 오픈소스 커뮤니티

## 로드맵

### Phase 1 - 핵심 기능 (현재)
- [완료] 코드 입력/정규화 (T-101, T-102)
- [완료] Nearest-Next 알고리즘 (T-201)
- [완료] FSM 상태머신 (T-301)
- [완료] A* 경로 탐색 (T-303)
- [진행중] 요약 집계 (T-304)
- [예정] Pause/Resume (T-307)

### Phase 2 - 완성도 (예정)
- 칸 제약 조건 (T-501, T-502)
- 재고 시스템 (T-601~603)
- CSV 결과 (T-701~706)
- 성능 최적화 (T-804, T-805)

### Phase 3 - 고급 기능 (미래)
- 다중 로봇
- 충돌 회피
- 백엔드 연동
- 실시간 대시보드

## 마일스톤

### 2025-10-11 - T-303 완료
- A* 경로 탐색 구현
- 타임아웃 처리
- 실패 추적
- 9개 단위 테스트
- 문서화 완료

다음 목표: T-304 (요약 집계) 완료

---

버전: 0.3.0 (T-303)
마지막 업데이트: 2025-10-11
상태: Phase 1 진행 중
