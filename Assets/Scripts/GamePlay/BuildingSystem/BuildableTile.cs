using UnityEngine;

public class BuildableTile : MonoBehaviour
{
    // Biến này sẽ lưu trữ tham chiếu đến cái trụ đang được xây trên ô đất này.
    public GameObject towerOnTile = null;

    private void Start()
    {
        InvokeRepeating(nameof(CheckTowerDie), 1f, 0.1f); // Kiểm tra mỗi giây
    }
    private void CheckTowerDie()
    {
        if (towerOnTile == null) return;
        TowerHealth health = towerOnTile.GetComponent<TowerHealth>();
        if (health.HasDie())
        {
            towerOnTile = null; // Xóa tham chiếu khi trụ chết
        }
    }
}

