using UnityEngine;

public class TowerUpgrade : MonoBehaviour
{
    [Header("Level hiện tại của prefab này (1..4)")]
    public int level = 1;

    [Header("Prefab level kế tiếp (để trống nếu đã là max)")]
    public GameObject nextLevelPrefab; // null ở Lv4

    // Được gọi khi bấm nút LevelUp
    public TowerUpgrade Upgrade()
    {
        if (nextLevelPrefab == null)
        {
            Debug.Log("Tower đang ở cấp tối đa!");
            return this;
        }

        Transform parent = transform.parent;
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        GameObject newGo = Instantiate(nextLevelPrefab, pos, rot, parent);
        var newTower = newGo.GetComponent<TowerUpgrade>();
        if (newTower == null)
        {
            Debug.LogError("Prefab next level chưa gắn TowerUpgrade!");
            return this;
        }

        // Hủy tháp cũ
        Destroy(gameObject);

        return newTower;
    }

    // Được gọi khi bấm nút Remove
    public void Remove()
    {
        Destroy(gameObject);
    }
}
