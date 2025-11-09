using UnityEngine;
using UnityEngine.UI;

public class CellView : MonoBehaviour
{
    private Image image;
    private string currentType = "empty";
    
    // 셀 초기화
    public void Init(int x, int y)
    {
        image = GetComponent<Image>();
        Render(); // 초기 렌더링
    }
    
    // 상태가 변경되었는지 여부를 반환
    public bool UpdateCell(string type, string info)
    {
        if (currentType != type) // 상태가 변경되었는지 확인
        {
            currentType = type; // 상태 업데이트
            return true; // 상태가 변경됨
        }
        
        return false; // 상태가 변경되지 않음
    }

    public void Render() // 셀의 시각적 상태를 업데이트
    {
        image.color = currentType switch
        {
            "empty" => new Color(0.8f, 0.8f, 0.8f),
            "partial" => Color.yellow,
            "full" => Color.red,
            "obstacle" => new Color(0.2f, 0.2f, 0.2f),
            "bookshelf" => new Color(0.55f, 0.43f, 0.39f),
            "robot" => Color.blue,
            _ => Color.white
        };
    }
}
