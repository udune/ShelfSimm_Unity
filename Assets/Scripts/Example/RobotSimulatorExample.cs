using System.Collections.Generic;
using Data.Data;
using Managers.Managers;
using UnityEngine;

namespace Example
{
    /// <summary>
    /// SimulationManager를 사용하여 시뮬레이션을 시작하는 예제 스크립트입니다.
    /// </summary>
    public class RobotSimulatorExample : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private SimulationManager simulationManager;

        void Start()
        {
            // SimulationManager 인스턴스를 자동으로 찾기
            if (simulationManager == null)
            {
                simulationManager = FindObjectOfType<SimulationManager>();
            }

            if (simulationManager == null)
            {
                Debug.LogError("씬에 SimulationManager가 존재하지 않습니다. 시뮬레이션을 시작할 수 없습니다.");
                return;
            }

            // 시뮬레이션을 시작할 작업 목록 생성
            var jobsToRun = new List<Job>
            {
                new Job(JobAction.PUT, "A01", "Test Book A", 2),
                new Job(JobAction.PUT, "A01", "Test Book A", 1), // 성공 (3/3)
                new Job(JobAction.PUT, "A01", "Test Book A", 1), // 실패 (용량 초과)
                new Job(JobAction.PICK, "A01", "Test Book A", 3), // 성공
                new Job(JobAction.PICK, "A01", "Test Book A", 1), // 실패 (재고 부족)
                new Job(JobAction.PUT, "B02", "Test Book B", 4), // 실패 (용량 초과)
            };

            // SimulationManager를 통해 시뮬레이션 시작
            Debug.Log("예제 스크립트에서 시뮬레이션을 시작합니다...");
            simulationManager.StartSimulationWithJobs(jobsToRun);
        }
    }
}
