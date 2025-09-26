# ShelfSim Unity

Unity 기반 서점 자동화 로봇 시뮬레이터

## 프로젝트 소개

ShelfSim은 서점에서 책을 자동으로 넣고 빼는 로봇을 시뮬레이션하는 프로그램입니다. 
로봇이 어떻게 움직이는지, 얼마나 빠르게 일을 처리하는지 미리 테스트해볼 수 있어요!

### 주요 기능

- **자동 순서 결정**: 로봇이 가장 가까운 칸부터 자동으로 찾아가요
- **실시간 시뮬레이션**: 로봇의 움직임을 눈으로 직접 확인할 수 있어요
- **칸 용량 관리**: 각 칸에 들어갈 수 있는 책의 개수를 자동으로 계산해요
- **결과 분석**: 작업이 끝나면 Excel로 결과를 확인할 수 있어요

## 빠른 시작

### 1. 필요한 것들
- Unit6 이상

### 2. 프로젝트 실행하기

```bash
# 1. 프로젝트 다운로드
git clone https://github.com/yourusername/ShelfSimm_Unity.git

# 2. Unity에서 프로젝트 열기
# Unity Hub → 열기 → ShelfSimm_Unity 폴더 선택

# 3. 첫 번째 씬 실행
# Assets/Scenes/MainScene.unity 더블클릭 후 Play 버튼
```

### 3. 첫 번째 시뮬레이션

1. **칸 코드 입력**: `D20, A15, B03` 처럼 입력해보세요
2. **작업 선택**: PUT(입고) 또는 PICK(출고) 선택
3. **책 선택**: 드롭다운에서 책을 골라주세요
4. **실행**: `시작` 버튼을 눌러보세요!

## 사용법

### 기본 조작
- **칸 클릭**: 오른쪽에서 칸 정보 확인
- **Pause/Resume**: 시뮬레이션 일시정지/재개
- **Stop**: 시뮬레이션 중단

### 입력 형식
```
칸 코드: D20, A15, B03 (쉼표나 스페이스로 구분)
수량: 1, 2, 3... (숫자만 입력)
```

### 결과 확인
시뮬레이션이 끝나면 `results_날짜_시간.csv` 파일이 자동으로 저장돼요!

## 📊 화면 구성

```
┌─────────────────┬─────────────┬─────────────────┐
│   입력 패널      │             │   정보 패널     │
│                 │             │                 │
│ • 칸 코드 입력   │             │ • 선택된 칸 정보 │
│ • 작업 타입      │   그리드    │ • 용량/재고      │
│ • 책 선택        │   화면      │ • 에러 메시지    │
│ • 실행 버튼      │             │                 │
└─────────────────┼─────────────┼─────────────────┤
│                   상단 대시보드                  │
│               (완료수, 시간, 제어)               │
└─────────────────────────────────────────────────┘
```

## 설정하기

### 기본 설정값
```json
{
  "handle_time": 2,        // 작업 처리 시간 (초)
  "robot_speed": 3,        // 로봇 이동 속도 (칸/초)
  "move_timeout_sec": 30,  // 이동 타임아웃 (초)
  "topN": 3,              // A* 재평가 후보 수
  "random_seed": 42       // 랜덤 시드
}
```

### 칸 레이아웃 설정
`Assets/Data/cells.json` 파일을 수정해서 칸 배치를 바꿀 수 있어요:

```json
{
  "cells": [
    {
      "code": "D20",
      "x": 12, "y": 5,
      "width": 90, "height": 200,
      "orientation": "N"
    }
  ]
}
```

## 결과 분석

### CSV 파일 구조
```csv
job_id,robot_name,action,cell_code,book_title,quantity,
start_timestamp,end_timestamp,travel_time_sec,handle_time_sec,
total_time_sec,path_length,result,fail_reason
```

### 성공/실패 이유
- **SUCCESS**: 작업 성공
- **CAPACITY_FULL**: 칸이 가득 참
- **HEIGHT_LIMIT**: 책이 너무 높음
- **ROUTE_BLOCKED**: 경로가 막힘
- **BOOK_MISMATCH**: 다른 책이 들어있음

## 개발자 정보

### 프로젝트 구조
```
Assets/
├── Scripts/          # 핵심 로직
│   ├── Core/         # FSM, A* 알고리즘
│   ├── UI/           # 사용자 인터페이스
│   └── Data/         # 데이터 관리
├── Scenes/           # 게임 씬
├── Data/             # 설정 파일
└── Prefabs/          # 프리팹
```

### 주요 컴포넌트
- **RobotController**: 로봇 움직임 제어
- **PathFinder**: A* 경로 탐색
- **InventoryManager**: 재고 관리
- **UIManager**: 사용자 인터페이스

## 라이선스

MIT License - 자유롭게 사용하세요!

## 기여하기

1. Fork 하기
2. 기능 브랜치 만들기 (`git checkout -b feature/amazing-feature`)
3. 커밋하기 (`git commit -m 'Add some amazing feature'`)
4. 푸시하기 (`git push origin feature/amazing-feature`)
5. Pull Request 열기

---
