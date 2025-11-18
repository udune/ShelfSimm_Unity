using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    #region DTO Classes

    [Serializable]
    public class CreateRunRequest
    {
        public int randomSeed;
        public float handleTimeSec;
        public float robotSpeedCellsPerSec;
        public int topN;
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
        public string action;
        public string cellCode;
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
        // TODO: 서버 API가 Job ID 목록을 반환하도록 수정되면 아래 필드 활성화
        // public string[] jobIds; 
    }
    
    [Serializable]
    public class UpdateJobResultRequest
    {
        public string startTs;
        public string endTs;
        public float travelTimeSec;
        public float handleTimeSec;
        public float totalTimeSec;
        public int pathLengthCells;
        public string result;
        public string failReason;
        public string robotName;
    }

    [Serializable]
    public class UpdateRunStatusRequest
    {
        public string status;
    }

    [Serializable]
    public class BookDto
    {
        public string id;
        public string title;
        public int thicknessMm;
        public int heightMm;
    }

    [Serializable]
    public class BookListDto
    {
        public List<BookDto> items;
    }

    [Serializable]
    public class JobDetailsDto
    {
        public string id;
        public string action;
        public string cellCode;
        public string bookTitle;
        public int quantity;
    }

    [Serializable]
    public class RunDetailsDto
    {
        public string id;
        public string status;
        public List<JobDetailsDto> jobs;
    }

    #endregion

    public class ApiClient : MonoBehaviour
    {
        public static ApiClient Instance { get; private set; }

        [Header("API 설정")]
        [SerializeField] private string baseUrl = "https://shelfsim-api-190183336439.asia-northeast3.run.app/api";
        [SerializeField] private bool logRequests = true;

        [Header("보안 설정 (주의)")]
        [Tooltip("경고: 개발/테스트 전용. 프로덕션에서는 반드시 false로 설정하세요!")]
        [SerializeField] private bool bypassSslValidation = false;

        private string currentRunId;
        private bool hasShownSecurityWarning = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public IEnumerator CreateRun(CreateRunRequest request, Action<RunResponse> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] POST {url} - Body: {json}");

            using (UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json"))
            {
                ConfigureCertificateHandler(www);
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
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
                }
            }
        }
    
        public IEnumerator CreateJobsBatch(CreateJobsBatchRequest request, Action<JobsBatchResponse> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Jobs/batch";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] POST {url} - Body: {json}");

            using (UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json"))
            {
                ConfigureCertificateHandler(www);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<JobsBatchResponse>(www.downloadHandler.text);
                    Debug.Log($"[API] Jobs Created: {response.accepted} jobs");
                    onSuccess?.Invoke(response);
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
                }
            }
        }
    
        public IEnumerator UpdateJobResult(string jobId, UpdateJobResultRequest request, Action onSuccess = null, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Jobs/{jobId}/result";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] PATCH {url} - Body: {json}");

            using (UnityWebRequest www = new UnityWebRequest(url, "PATCH"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                ConfigureCertificateHandler(www);

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[API] Job Updated: {jobId}");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
                }
            }
        }

        public IEnumerator UpdateRunStatus(string runId, UpdateRunStatusRequest request, Action onSuccess = null, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs/{runId}/status";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] PATCH {url} - Body: {json}");

            using (UnityWebRequest www = new UnityWebRequest(url, "PATCH"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                ConfigureCertificateHandler(www);

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[API] Run Status Updated: {runId}");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
                }
            }
        }

        public IEnumerator GetRunResultsCsv(string runId, Action<string> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs/{runId}/results.csv";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                ConfigureCertificateHandler(www);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[API] CSV Data Received for Run: {runId}");
                    onSuccess?.Invoke(www.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
                }
            }
        }

        public IEnumerator GetAllBooks(Action<List<BookDto>> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Books";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                ConfigureCertificateHandler(www);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = WrapJsonArrayIfNeeded(www.downloadHandler.text, "items");
                    BookListDto bookList = JsonUtility.FromJson<BookListDto>(jsonResponse);

                    Debug.Log($"[API] Books Received: {bookList.items.Count} items");
                    onSuccess?.Invoke(bookList.items);
                }
                else
                {
                    Debug.LogError($"[API] Error getting books: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
                }
            }
        }

        public IEnumerator GetRunDetails(string runId, Action<RunDetailsDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs/{runId}";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                ConfigureCertificateHandler(www);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    RunDetailsDto runDetails = JsonUtility.FromJson<RunDetailsDto>(www.downloadHandler.text);
                    Debug.Log($"[API] Run Details Received: {runDetails.jobs.Count} jobs found.");
                    onSuccess?.Invoke(runDetails);
                }
                else
                {
                    Debug.LogError($"[API] Error getting run details: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
                }
            }
        }

        public string GetCurrentRunId()
        {
            return currentRunId;
        }

        /// <summary>
        /// Unity JsonUtility는 최상위 배열을 파싱할 수 없으므로,
        /// 배열 응답을 객체로 래핑하는 헬퍼 메서드
        /// </summary>
        private string WrapJsonArrayIfNeeded(string json, string wrapperKey = "items")
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            // 공백 제거 후 첫 문자 확인
            string trimmed = json.TrimStart();

            // 배열로 시작하면 객체로 래핑
            if (trimmed.StartsWith("["))
            {
                return $"{{\"{wrapperKey}\":{json}}}";
            }

            // 이미 객체 형태면 그대로 반환
            return json;
        }

        /// <summary>
        /// SSL 인증서 검증 우회 여부에 따라 적절한 CertificateHandler 설정
        /// 보안 경고: 프로덕션에서는 절대 우회하지 말 것!
        /// </summary>
        private void ConfigureCertificateHandler(UnityWebRequest request)
        {
            if (bypassSslValidation)
            {
                // 첫 번째 사용 시에만 경고 표시
                if (!hasShownSecurityWarning)
                {
                    Debug.LogWarning("[보안 경고] SSL 인증서 검증이 우회되었습니다. 이는 중간자 공격(MITM)에 취약합니다. 개발/테스트 환경에서만 사용하세요!");
                    hasShownSecurityWarning = true;
                }
                request.certificateHandler = new BypassCertificate();
            }
            // bypassSslValidation이 false면 Unity의 기본 인증서 검증 사용
        }
    }
    
    /// <summary>
    /// 보안 경고: 이 클래스는 모든 SSL 인증서를 승인합니다.
    /// 중간자 공격(MITM)에 취약하므로 개발/테스트 환경에서만 사용하세요!
    /// 프로덕션에서는 절대 사용하지 마세요!
    /// </summary>
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // 경고: 모든 인증서를 무조건 승인 - 보안 위험!
            return true;
        }
    }
}
