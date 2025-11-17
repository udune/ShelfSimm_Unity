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
        [Header("API 설정")]
        [SerializeField] private string baseUrl = "https://shelfsim-api-190183336439.asia-northeast3.run.app/api";
        [SerializeField] private bool logRequests = true;
    
        private string currentRunId;

        public IEnumerator CreateRun(CreateRunRequest request, Action<RunResponse> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] POST {url} - Body: {json}");

            using (UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json"))
            {
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
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke(www.error);
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
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke(www.error);
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
                www.certificateHandler = new BypassCertificate();

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[API] Job Updated: {jobId}");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke(www.error);
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
                www.certificateHandler = new BypassCertificate();

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[API] Run Status Updated: {runId}");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke(www.error);
                }
            }
        }

        public IEnumerator GetRunResultsCsv(string runId, Action<string> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs/{runId}/results.csv";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.certificateHandler = new BypassCertificate();
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[API] CSV Data Received for Run: {runId}");
                    onSuccess?.Invoke(www.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[API] Error: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke(www.error);
                }
            }
        }

        public IEnumerator GetAllBooks(Action<List<BookDto>> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Books";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.certificateHandler = new BypassCertificate();
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = "{\"items\":" + www.downloadHandler.text + "}";
                    BookListDto bookList = JsonUtility.FromJson<BookListDto>(jsonResponse);
                    
                    Debug.Log($"[API] Books Received: {bookList.items.Count} items");
                    onSuccess?.Invoke(bookList.items);
                }
                else
                {
                    Debug.LogError($"[API] Error getting books: {www.error}\n{www.downloadHandler.text}");
                    onError?.Invoke(www.error);
                }
            }
        }

        public IEnumerator GetRunDetails(string runId, Action<RunDetailsDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Runs/{runId}";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.certificateHandler = new BypassCertificate();
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
                    onError?.Invoke(www.error);
                }
            }
        }
    
        public string GetCurrentRunId()
        {
            return currentRunId;
        }
    }
    
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}
