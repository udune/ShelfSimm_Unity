using System.Collections;
using UnityEngine;
using API;
using API.API;

public class TestApiController : MonoBehaviour
{
    [SerializeField] private ApiClient apiClient;

    void Start()
    {
        // API 클라이언트 자동 찾기
        if (apiClient == null)
            apiClient = FindObjectOfType<ApiClient>();

        // 테스트 시작
        StartCoroutine(RunFullTest());
    }

    IEnumerator RunFullTest()
    {
        Debug.Log("API 테스트 시작...");

        // 1. Run 생성
        var createRunReq = new CreateRunRequest
        {
            randomSeed = 42,
            handleTimeSec = 2f,
            robotSpeedCellsPerSec = 3f,
            topN = 3
        };

        bool runCreated = false;
        string runId = null;
        yield return apiClient.CreateRun(createRunReq,
            onSuccess: (response) => {
                Debug.Log($"Run ID: {response.id}");
                runId = response.id;
                runCreated = true;
            },
            onError: (error) => {
                Debug.LogError($"Run 생성 실패: {error}");
            }
        );

        if (!runCreated)
            yield break;

        yield return new WaitForSeconds(1f);

        // 2. Jobs 생성
        var createJobsReq = new CreateJobsBatchRequest
        {
            runId = runId, // 이전 단계에서 받은 runId 사용
            jobs = new JobDto[]
            {
                new JobDto { action = "PUT", cellCode = "D20", bookTitle = "Unity in Action", quantity = 1 },
                new JobDto { action = "PICK", cellCode = "A15", bookTitle = "C# Best Practices", quantity = 2 }
            }
        };

        yield return apiClient.CreateJobsBatch(createJobsReq,
            onSuccess: (response) => {
                Debug.Log($"{response.accepted}개 Job 생성 완료");
            }
        );

        Debug.Log("API 테스트 완료!");
    }
}
