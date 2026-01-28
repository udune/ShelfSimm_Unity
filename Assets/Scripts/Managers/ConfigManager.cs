using System;
using System.Collections;
using API;
using Core;
using Data;
using UnityEngine;

namespace Managers
{
    public class ConfigManager : MonoBehaviour
    {
        #region Singleton
        public static ConfigManager Instance { get; private set; }
        #endregion

        #region Config References
        [Header("Core Configurations")]
        [SerializeField] private SimulationConfig simulationConfig;

        [Header("Data Configurations")]
        [SerializeField] private CellsLayoutSO cellsLayout;

        [Header("API Settings")]
        [SerializeField] private bool loadFromApiOnStart = true;
        [SerializeField] private float apiRetryDelay = 1.0f;
        #endregion

        #region State
        private string currentConfigId;
        private string currentLayoutId;
        private bool isInitialized;
        #endregion

        #region Events
        public event Action OnInitialized;
        #endregion

        #region Public Properties
        public SimulationConfig SimulationConfig => simulationConfig;
        public CellsLayoutSO CellsLayout => cellsLayout;
        public string CurrentConfigId => currentConfigId;
        public string CurrentLayoutId => currentLayoutId;
        public bool IsInitialized => isInitialized;
        public bool LoadFromApiOnStart => loadFromApiOnStart;
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (loadFromApiOnStart)
            {
                StartCoroutine(InitializeFromApiCoroutine());
            }
            else
            {
                isInitialized = true;
                OnInitialized?.Invoke();
            }
        }

        private IEnumerator InitializeFromApiCoroutine()
        {
            Debug.Log("[ConfigManager] Starting API initialization...");

            // Wait for ApiClient to be ready
            float waitTime = 0f;
            while (ApiClient.Instance == null && waitTime < 5f)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }

            if (ApiClient.Instance == null)
            {
                Debug.LogWarning("[ConfigManager] ApiClient not available, using local ScriptableObject values");
                FinalizeInitialization(false, false);
                yield break;
            }

            Debug.Log("[ConfigManager] ApiClient ready, loading defaults from API...");

            bool configLoaded = false;
            bool layoutLoaded = false;
            bool configRequestDone = false;
            bool layoutRequestDone = false;

            // Load config
            yield return ApiClient.Instance.GetDefaultConfig(
                config =>
                {
                    if (config != null)
                    {
                        ApplyConfig(config);
                        currentConfigId = config.id;
                        configLoaded = true;
                        Debug.Log($"[ConfigManager] Config loaded: {config.name}");
                    }
                    configRequestDone = true;
                },
                error =>
                {
                    Debug.LogWarning($"[ConfigManager] Config load failed: {error}");
                    configRequestDone = true;
                }
            );

            // Wait for config request to complete
            while (!configRequestDone)
            {
                yield return null;
            }

            // Load layout
            yield return ApiClient.Instance.GetDefaultLayout(
                layout =>
                {
                    if (layout != null)
                    {
                        ApplyLayout(layout);
                        currentLayoutId = layout.id;
                        layoutLoaded = true;
                        Debug.Log($"[ConfigManager] Layout loaded: {layout.name} ({layout.cells?.Length ?? 0} cells)");
                    }
                    layoutRequestDone = true;
                },
                error =>
                {
                    Debug.LogWarning($"[ConfigManager] Layout load failed: {error}");
                    layoutRequestDone = true;
                }
            );

            // Wait for layout request to complete
            while (!layoutRequestDone)
            {
                yield return null;
            }

            FinalizeInitialization(configLoaded, layoutLoaded);
        }

        private void FinalizeInitialization(bool configFromApi, bool layoutFromApi)
        {
            isInitialized = true;

            // Log current state
            Debug.Log($"[ConfigManager] === Initialization Complete ===");
            Debug.Log($"[ConfigManager] Config source: {(configFromApi ? "API" : "Local ScriptableObject")}");
            Debug.Log($"[ConfigManager] Layout source: {(layoutFromApi ? "API" : "Local ScriptableObject")}");
            Debug.Log($"[ConfigManager] Current cells count: {cellsLayout?.cells?.Count ?? 0}");

            if (cellsLayout != null && cellsLayout.cells != null)
            {
                foreach (var cell in cellsLayout.cells)
                {
                    Debug.Log($"[ConfigManager]   - Cell: {cell.code} at ({cell.X}, {cell.Y})");
                }
            }

            OnInitialized?.Invoke();
        }
        #endregion

        #region Apply Methods

        public void ApplyConfig(ConfigDto config)
        {
            if (simulationConfig == null)
            {
                Debug.LogError("[ConfigManager] SimulationConfig is not assigned");
                return;
            }

            simulationConfig.handleTime = config.handleTime;
            simulationConfig.robotSpeed = config.robotSpeed;
            simulationConfig.moveTimeoutSec = config.moveTimeoutSec;
            simulationConfig.topN = config.topN;
            simulationConfig.randomSeed = config.randomSeed;
            simulationConfig.warehousePos = new Vector2Int(config.warehousePosX, config.warehousePosY);
        }

        public void ApplyLayout(LayoutDetailDto layout)
        {
            if (cellsLayout == null)
            {
                Debug.LogError("[ConfigManager] CellsLayout is not assigned");
                return;
            }

            cellsLayout.warehouse = new Vector2Int(layout.warehouseX, layout.warehouseY);
            cellsLayout.cells.Clear();

            if (layout.cells != null)
            {
                foreach (var cellDto in layout.cells)
                {
                    var cellDef = new CellDef(
                        cellDto.code,
                        cellDto.width,
                        cellDto.height,
                        cellDto.orientation
                    );
                    cellsLayout.cells.Add(cellDef);
                }
            }

            cellsLayout.UpdateCellPositionsFromCodes();
        }

        #endregion
    }
}
