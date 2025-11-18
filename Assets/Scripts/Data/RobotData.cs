using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Data
{
    // 로봇의 상태를 나타내는 열거형
    [Serializable]
    public class RobotData
    {
        public string id; // 로봇ID
        public string name; // 로봇이름
        public Vector2Int position; // 로봇현재위치
        public RobotState state; // 로봇상태
        public Vector2Int? targetPosition; // 로봇목표위치
        public List<Vector2Int> path; // 로봇경로
        public int pathIndex; // 현재경로인덱스

        public float moveStartTime; // 이동시작시간
        public float moveTimeoutSec; // 이동타임아웃초

        public ErrorCode? errorCode; // 오류코드
        
        // 생성자
        public RobotData(string id, string name, Vector2Int position, float timeout = 30f)
        {
            this.id = id;
            this.name = name;
            this.position = position;
            this.state = RobotState.IDLE;
            this.targetPosition = null;
            this.path = null;
            this.pathIndex = 0;
            this.moveStartTime = 0f;
            this.moveTimeoutSec = timeout;
            this.errorCode = null;
        }
    }
}