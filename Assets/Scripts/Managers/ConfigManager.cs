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
        [SerializeField] private TiebreakerConfig tiebreakerConfig;

        [Header("Data Configurations")]
        [SerializeField] private CellsLayoutSO cellsLayout;
        #endregion

        #region Public Properties
        public SimulationConfig SimulationConfig => simulationConfig;
        public TiebreakerConfig TiebreakerConfig => tiebreakerConfig;
        public CellsLayoutSO CellsLayout => cellsLayout;
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
        #endregion
    }
}
