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
    public class RunListMeta
    {
        public int page;
        public int pageSize;
        public int totalCount;
        public int totalPages;
    }

    [Serializable]
    public class RunListResponse
    {
        public RunResponse[] data;
        public RunListMeta meta;

        // 하위 호환성을 위한 속성
        public RunResponse[] items => data;
        public int totalCount => meta?.totalCount ?? 0;
    }

    [Serializable]
    public class JobDto
    {
        public string action;
        public string cellCode;
        public string materialName;
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
        public string workerId;
        public float snapshotTemp;
        public float snapshotHumid;
        public bool snapshotLightLeak;
    }

    [Serializable]
    public class UpdateRunStatusRequest
    {
        public string status;
    }

    [Serializable]
    public class MaterialDto
    {
        public string id;
        public string name;
        public string vendor;
        public string lotId;
        public int stockQty;
        public string type;
        public string expiryDate;
        public string createdAt;
    }

    [Serializable]
    public class JobDetailsDto
    {
        public string id;
        public string action;
        public string cellCode;
        public string materialName;
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
    public class MaterialListDto
    {
        public MaterialDto[] items;
    }

    [Serializable]
    public class JobListDto
    {
        public JobDetailsDto[] items;
    }

    #endregion

    #region Configuration API DTOs

    [Serializable]
    public class ConfigDto
    {
        public string id;
        public string name;
        public float handleTime;
        public float robotSpeed;
        public float moveTimeoutSec;
        public int topN;
        public int randomSeed;
        public int warehousePosX;
        public int warehousePosY;
        public bool isDefault;
        public string createdAt;
        public string updatedAt;
    }

    [Serializable]
    public class ConfigListResponse
    {
        public ConfigDto[] data;
        public int totalCount;
    }

    [Serializable]
    public class CreateConfigRequest
    {
        public string name;
        public float handleTime = 2.0f;
        public float robotSpeed = 3.0f;
        public float moveTimeoutSec = 30.0f;
        public int topN = 3;
        public int randomSeed = 42;
        public int warehousePosX = 0;
        public int warehousePosY = 0;
    }

    [Serializable]
    public class UpdateConfigRequest
    {
        public string name;
        public float handleTime;
        public float robotSpeed;
        public float moveTimeoutSec;
        public int topN;
        public int randomSeed;
        public int warehousePosX;
        public int warehousePosY;
    }

    #endregion

    #region CellsLayout API DTOs

    [Serializable]
    public class LayoutListItemDto
    {
        public string id;
        public string name;
        public int warehouseX;
        public int warehouseY;
        public int cellCount;
        public bool isDefault;
        public string createdAt;
        public string updatedAt;
    }

    [Serializable]
    public class LayoutListResponse
    {
        public LayoutListItemDto[] data;
        public int totalCount;
    }

    [Serializable]
    public class CellDefDto
    {
        public string id;
        public string code;
        public int width;
        public int height;
        public string orientation;
    }

    [Serializable]
    public class LayoutDetailDto
    {
        public string id;
        public string name;
        public int warehouseX;
        public int warehouseY;
        public string layoutHash;
        public bool isDefault;
        public string createdAt;
        public string updatedAt;
        public CellDefDto[] cells;
    }

    [Serializable]
    public class CreateCellDefRequest
    {
        public string code;
        public int width = 90;
        public int height = 200;
        public string orientation = "N";
    }

    [Serializable]
    public class CreateLayoutRequest
    {
        public string name;
        public int warehouseX = 0;
        public int warehouseY = 0;
        public CreateCellDefRequest[] cells;
    }

    [Serializable]
    public class UpdateLayoutRequest
    {
        public string name;
        public int warehouseX;
        public int warehouseY;
        public CreateCellDefRequest[] cells;
    }

    [Serializable]
    public class BatchCellsRequest
    {
        public CreateCellDefRequest[] cells;
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

        public IEnumerator GetAllMaterials(Action<List<MaterialDto>> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Materials";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                if (logRequests)
                {
                    Debug.Log($"[API] Materials response: {jsonResponse}");
                }

                string dtoJson = $"{{\"items\":{jsonResponse}}}";
                MaterialListDto dto = JsonUtility.FromJson<MaterialListDto>(dtoJson);
                List<MaterialDto> materialList = new List<MaterialDto>(dto.items);

                if (logRequests)
                {
                    Debug.Log($"[API] Loaded {materialList.Count} materials");
                }
                onSuccess?.Invoke(materialList);
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
                Debug.Log($"[API] GetRuns 응답: {json}");

                if (json.TrimStart().StartsWith("["))
                {
                    json = $"{{\"items\":{json}, \"totalCount\":0}}";
                }

                var response = JsonUtility.FromJson<RunListResponse>(json);
                Debug.Log($"[API] GetRuns 파싱 결과: items={response?.items?.Length ?? 0}개, totalCount={response?.totalCount ?? 0}");
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

        #region Configuration API

        public IEnumerator GetConfigs(Action<ConfigListResponse> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Configs";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ConfigListResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator GetConfigById(string configId, Action<ConfigDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Configs/{configId}";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ConfigDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator GetDefaultConfig(Action<ConfigDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Configs/default";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ConfigDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator CreateConfig(CreateConfigRequest request, Action<ConfigDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Configs";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] POST {url}");

            using UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ConfigDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator UpdateConfig(string configId, UpdateConfigRequest request, Action<ConfigDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Configs/{configId}";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] PUT {url}");

            using UnityWebRequest www = UnityWebRequest.Put(url, json);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ConfigDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator DeleteConfig(string configId, Action onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Configs/{configId}";
            if (logRequests) Debug.Log($"[API] DELETE {url}");

            using UnityWebRequest www = UnityWebRequest.Delete(url);
            www.downloadHandler = new DownloadHandlerBuffer();
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

        public IEnumerator SetDefaultConfig(string configId, Action<ConfigDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/Configs/{configId}/set-default";
            if (logRequests) Debug.Log($"[API] POST {url}");

            using UnityWebRequest www = UnityWebRequest.Post(url, "", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<ConfigDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        #endregion

        #region CellsLayout API

        public IEnumerator GetLayouts(Action<LayoutListResponse> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/CellsLayouts";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LayoutListResponse>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator GetLayoutById(string layoutId, Action<LayoutDetailDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/CellsLayouts/{layoutId}";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LayoutDetailDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator GetDefaultLayout(Action<LayoutDetailDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/CellsLayouts/default";
            if (logRequests) Debug.Log($"[API] GET {url}");

            using UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LayoutDetailDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator CreateLayout(CreateLayoutRequest request, Action<LayoutDetailDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/CellsLayouts";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] POST {url}");

            using UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LayoutDetailDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator UpdateLayout(string layoutId, UpdateLayoutRequest request, Action<LayoutDetailDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/CellsLayouts/{layoutId}";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] PUT {url}");

            using UnityWebRequest www = UnityWebRequest.Put(url, json);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LayoutDetailDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator DeleteLayout(string layoutId, Action onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/CellsLayouts/{layoutId}";
            if (logRequests) Debug.Log($"[API] DELETE {url}");

            using UnityWebRequest www = UnityWebRequest.Delete(url);
            www.downloadHandler = new DownloadHandlerBuffer();
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

        public IEnumerator SetDefaultLayout(string layoutId, Action<LayoutDetailDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/CellsLayouts/{layoutId}/set-default";
            if (logRequests) Debug.Log($"[API] POST {url}");

            using UnityWebRequest www = UnityWebRequest.Post(url, "", "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LayoutDetailDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        public IEnumerator AddCellsBatch(string layoutId, BatchCellsRequest request, Action<LayoutDetailDto> onSuccess, Action<string> onError = null)
        {
            string url = $"{baseUrl}/CellsLayouts/{layoutId}/cells/batch";
            string json = JsonUtility.ToJson(request);
            if (logRequests) Debug.Log($"[API] POST {url}");

            using UnityWebRequest www = UnityWebRequest.Post(url, json, "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LayoutDetailDto>(www.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"[API] Error: {www.error}");
                onError?.Invoke($"{www.error}\n{www.downloadHandler.text}");
            }
        }

        #endregion
    }
}
