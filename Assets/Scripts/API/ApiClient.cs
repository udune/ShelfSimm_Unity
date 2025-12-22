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
    public class RunListResponse
    {
        public RunResponse[] items;
        public int totalCount;
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
        public string layoutId;
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
        public int id;
        public string title;
        public string author;
        public int thicknessMn;
        public int heightMm;
        public string sku;
        public string createdAt;
        public int stockQuantity;
    }

    [Serializable]
    public class JobDetailsDto
    {
        public string id;
        public string action;
        public string cellCode;
        public string bookTitle;
        public int quantity;
        public string result; // 작업 결과 (Success/Fail)
    }

    [Serializable]
    public class RunDetailsDto
    {
        public string id;
        public string status;
        public List<JobDetailsDto> jobs;
    }

    [Serializable]
    public class BookListDto
    {
        public BookDto[] items;
    }

    [Serializable]
    public class JobListDto
    {
        public JobDetailsDto[] items;
    }

    #endregion

    public class ApiClient : MonoBehaviour
    {
        public static ApiClient Instance { get; private set; }

        [Header("API 설정")]
        [SerializeField] private string baseUrl = "https://shelfsim-api-190183336439.asia-northeast3.run.app/api";
        [SerializeField] private bool logRequests = true;

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
            if (logRequests) Debug.Log($"[API] POST {url}");

            using UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<RunResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }
    
        public IEnumerator CreateJobsBatch(CreateJobsBatchRequest request, Action<JobsBatchResponse> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Jobs/batch";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] POST {url}");

            using UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<JobsBatchResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }
    
        public IEnumerator UpdateJobResult(string jobId, UpdateJobResultRequest request, Action onSuccess = null, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Jobs/{jobId}/result";
            string json = JsonUtility.ToJson(request);
            if (logRequests)
            {
                Debug.Log($"[API] PATCH {url}");
                Debug.Log($"[API] Body: {json}");
            }

            using UnityWebRequest www = new UnityWebRequest(url, "PATCH");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError($"[API] Error on PATCH {url}: {www.error}. Response Code: {www.responseCode}");
                Debug.LogError($"[API] Response: {www.downloadHandler.text}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator UpdateRunStatus(string runId, UpdateRunStatusRequest request, Action onSuccess = null, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs/{runId}/status";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] PATCH {url}");

            using UnityWebRequest www = new UnityWebRequest(url, "PATCH");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator GetRunResultsCsv(string runId, Action<string> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs/{runId}/results.csv";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(www.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator GetAllBooks(Action<List<BookDto>> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Books";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                if (logRequests)
                {
                    Debug.Log($"[API] Books response: {jsonResponse}");
                }

                string dtoJson = $"{{\"items\":{jsonResponse}}}";
                BookListDto dto = JsonUtility.FromJson<BookListDto>(dtoJson);
                List<BookDto> bookList = new List<BookDto>(dto.items);

                if (logRequests)
                {
                    Debug.Log($"[API] Loaded {bookList.Count} books");
                }
                onSuccess?.Invoke(bookList);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator GetRunDetails(string runId, Action<RunDetailsDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs/{runId}";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                RunDetailsDto runDetails = JsonUtility.FromJson<RunDetailsDto>(www.downloadHandler.text);
                onSuccess?.Invoke(runDetails);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator GetRuns(int page, int pageSize, Action<RunListResponse> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs?page={page}&pageSize={pageSize}";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // API 응답이 배열 형태인지 객체 형태인지 확인 필요.
                // 명세서에는 페이징 지원이라고 되어 있으므로 { items: [], totalCount: 0 } 형태일 가능성이 높음.
                // 하지만 현재 RunResponse[] 형태로 올 수도 있으므로 확인 필요.
                // 여기서는 일단 JSON을 그대로 파싱 시도.
                    
                // 만약 배열로 온다면:
                string json = www.downloadHandler.text;
                if (json.TrimStart().StartsWith("["))
                {
                    json = $"{{\"items\":{json}, \"totalCount\":0}}";
                }
                    
                var response = JsonUtility.FromJson<RunListResponse>(json);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator GetJobsByRunId(string runId, Action<List<JobDetailsDto>> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Jobs?runId={runId}";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                // 배열로 온다고 가정
                if (json.TrimStart().StartsWith("["))
                {
                    json = $"{{\"items\":{json}}}";
                }
                    
                var dto = JsonUtility.FromJson<JobListDto>(json);
                var jobList = new List<JobDetailsDto>(dto.items);
                onSuccess?.Invoke(jobList);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }
    }
}
