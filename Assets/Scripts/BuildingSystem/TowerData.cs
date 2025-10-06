using UnityEngine;

[CreateAssetMenu(fileName = "New Tower", menuName = "Towers/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName; // Tên trụ (ví dụ: "Archer")
    public GameObject towerPrefab; // Prefab của trụ
    public Sprite towerIcon; // Icon để hiển thị trên nút UI
    public int buildCost; // Giá tiền để xây
                          // Bạn có thể thêm các thuộc tính khác như mô tả, sát thương, tầm bắn...
}
