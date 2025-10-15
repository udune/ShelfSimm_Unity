using UnityEngine;
using UI;

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
        if (!autoSimulate) return;
        
        timer += Time.deltaTime;
        if (timer >= simulateInterval)
        {
            timer = 0;
            SimulateRobotMovement();
        }
    }
    
    void SimulateRobotMovement()
    {
        // 이전 위치 지우기
        if (lastRobotPos.x >= 0)
        {
            gridRenderer.UpdateCell(lastRobotPos.x, lastRobotPos.y, "empty");
        }
        
        // 새 위치
        int x = Random.Range(0, 50);
        int y = Random.Range(0, 50);
        
        gridRenderer.UpdateCell(x, y, "robot");
        gridRenderer.RenderChanges();
        
        lastRobotPos = new Vector2Int(x, y);
    }
}