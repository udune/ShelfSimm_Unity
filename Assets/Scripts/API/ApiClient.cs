using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    // ===== DTO 클래스들 =====
    [Serializable]
    public class CreateRunRequest
    {
        public int randomSeed;
        public float handleTimeSec = 2f;
        public float robotSpeedCellsPerSec = 3f;
        public int topN = 3;
    }
    
    [Serializable]
    public class RunResponse
    {
        public string id;
        public int randomSeed;
        public string status;
        public string createdAt;
    }
    
    [Serializable]
    public class JobDto
    {
        public string action; // PUT, PICK
        public string cellCode; // D20, A15
        public string bookTitle;
        public int quantity;
    }
    
    [Serializable]
    public class CreateJobsBatchRequest
    {
        public string runId;
        public JobDto[] jobs;
    }
    
    [Serializable]
    public class JobsBatchResponse
    {
        public int accepted;
        public string runId;
    }
    
    [Serializable]
    public class UpdateJobResultRequest
    {
        public string startTs; // ISO 8601
        public string endTs;
        public float travelTimeSec;
        public float handleTimeSec;
        public float totalTimeSec;
        public int pathLengthCells;
        public string result; // SUCCESS, FAIL
        public string failReason;
        public string robotName;
    }

    [Serializable]
    public class UpdateRunStatusRequest
    {
        public string status; // COMPLETED, FAILED 등
    }
    
    // ===== API 클라이언트 =====
    public class ApiClient : MonoBehaviour
    {
        [Header("API 설정")]
        [SerializeField] private string baseUrl = "https://shelfsim-api-190183336439.asia-northeast3.run.app/api";
        [SerializeField] private bool logRequests = true;
    
        private string currentRunId;
    
        // ===== 1. Run 생성 =====
        public IEnumerator CreateRun(CreateRunRequest request, Action<RunResponse> onSuccess, Action<string> onError = null)
        {
            string json = JsonUtility.ToJson(request);
            if (logRequests)
                Debug.Log($"[API] POST /runs - Body: {json}");
    
            using (UnityWebRequest www = new UnityWebRequest($"{baseUrl}/runs", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.certificateHandler = new BypassCertificate();
    
                yield return www.SendWebRequest();
    
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<RunResponse>(www.downloadHandler.text);
                    currentRunId = response.id;
                    Debug.Log($"[API] Run Created: {response.id}");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}\\n{www.downloadHandler.text}");
                    onError?.Invoke(www.error);
                }
            }
        }
    
        // ===== 2. Job 일괄 생성 =====
        public IEnumerator CreateJobsBatch(CreateJobsBatchRequest request, Action<JobsBatchResponse> onSuccess, Action<string> onError = null)
        {
            string json = JsonUtility.ToJson(request);
            if (logRequests)
                Debug.Log($"[API] POST /jobs/batch - Body: {json}");
    
            using (UnityWebRequest www = new UnityWebRequest($"{baseUrl}/jobs/batch", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.certificateHandler = new BypassCertificate();
    
                yield return www.SendWebRequest();
    
                if (www.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<JobsBatchResponse>(www.downloadHandler.text);
                    Debug.Log($"[API] Jobs Created: {response.accepted} jobs");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}");
                    onError?.Invoke(www.error);
                }
            }
        }
    
        // ===== 3. Job 결과 업데이트 =====
        public IEnumerator UpdateJobResult(string jobId, UpdateJobResultRequest request, Action onSuccess = null, Action<string> onError = null)
        {
            string json = JsonUtility.ToJson(request);
            if (logRequests)
                Debug.Log($"[API] PATCH /jobs/{jobId}/result - Body: {json}");
    
            using (UnityWebRequest www = new UnityWebRequest($"{baseUrl}/jobs/{jobId}/result", "PATCH"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.certificateHandler = new BypassCertificate();
    
                yield return www.SendWebRequest();
    
                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[API] Job Updated: {jobId}");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}");
                    onError?.Invoke(www.error);
                }
            }
        }

        // ===== 4. Run 상태 업데이트 =====
        public IEnumerator UpdateRunStatus(string runId, UpdateRunStatusRequest request, Action onSuccess = null, Action<string> onError = null)
        {
            string json = JsonUtility.ToJson(request);
            if (logRequests)
                Debug.Log($"[API] PATCH /runs/{runId}/status - Body: {json}");

            using (UnityWebRequest www = new UnityWebRequest($"{baseUrl}/runs/{runId}/status", "PATCH"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.certificateHandler = new BypassCertificate();

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[API] Run Status Updated: {runId}");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}");
                    onError?.Invoke(www.error);
                }
            }
        }

        // ===== 5. Run 결과 CSV 다운로드 =====
        public IEnumerator GetRunResultsCsv(string runId, Action<string> onSuccess, Action<string> onError = null)
        {
            if (logRequests)
                Debug.Log($"[API] GET /runs/{runId}/results.csv");

            using (UnityWebRequest www = UnityWebRequest.Get($"{baseUrl}/runs/{runId}/results.csv"))
            {
                www.certificateHandler = new BypassCertificate();
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string csvData = www.downloadHandler.text;
                    Debug.Log($"[API] CSV Data Received for Run: {runId}");
                    onSuccess?.Invoke(csvData);
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}");
                    onError?.Invoke(www.error);
                }
            }
        }
    
        // ===== 현재 Run ID 가져오기 =====
        public string GetCurrentRunId() => currentRunId;
    }
    
    // HTTPS 인증서 검증 우회 (개발용만 사용!)
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }
}
