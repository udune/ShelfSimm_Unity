using UI;
using UnityEngine;

namespace Example
{
    public class GridRendererExample : MonoBehaviour
    {
        [SerializeField] private GridRenderer gridRenderer;
        [SerializeField] private bool autoSimulate = true;
        [SerializeField] private float simulateInterval = 0.5f;
    
        private float timer;
        private Vector2Int lastRobotPos = new Vector2Int(-1, -1);
    
        void Start()
        {
            gridRenderer.Init(50, 50);
            InitializeGrid();
        }
    
        void InitializeGrid()
        {
            // 장애물
            for (int x = 5; x <= 7; x++)
            {
                for (int y = 5; y <= 7; y++)
                {
                    gridRenderer.UpdateCell(x, y, "obstacle");
                }
            }
        
            // 책장
            for (int x = 10; x <= 15; x++)
            {
                gridRenderer.UpdateCell(x, 10, "bookshelf");
            }
        
            gridRenderer.RenderChanges();
        }
    
        void Update()
        {
            if (!autoSimulate)
            {
                return;
            }
        
            timer += Time.deltaTime;
            if (timer >= simulateInterval)
            {
                timer = 0;
                SimulateRobotMovement();
            }
        }
    
        void SimulateRobotMovement()
        {
            if (lastRobotPos.x >= 0)
            {
                gridRenderer.UpdateCell(lastRobotPos.x, lastRobotPos.y, "empty");
            }
    
            int dx = Random.Range(-1, 2);
            int dy = Random.Range(-1, 2);
    
            int newX = Mathf.Clamp(lastRobotPos.x + dx, 0, 49);
            int newY = Mathf.Clamp(lastRobotPos.y + dy, 0, 49);
    
            // 장애물이면 이동 안함
            if (gridRenderer.GetCellType(newX, newY) == "obstacle")
            {
                Debug.Log("장애물로 인해 이동 불가");
                newX = lastRobotPos.x;
                newY = lastRobotPos.y;
            }
    
            gridRenderer.UpdateCell(newX, newY, "robot");
            gridRenderer.RenderChanges();
    
            lastRobotPos = new Vector2Int(newX, newY);
        }
    }
}