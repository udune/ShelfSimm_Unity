using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class RobotData
    {
        public string id;
        public string name;
        public Vector2Int position;
        public RobotState state;
        public Vector2Int? targetPosition;
        public List<Vector2Int> path;
        public int pathIndex;

        public float moveStartTime;
        public float moveTimeoutSec;

        public ErrorCode? errorCode;

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