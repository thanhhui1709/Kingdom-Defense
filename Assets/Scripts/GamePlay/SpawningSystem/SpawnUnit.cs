using UnityEngine;

public class SpawnUnit : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("Vị trí mà lính sẽ được spawn ra")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 offset;

    /// <summary>
    /// Hàm này được gọi bởi các nút UI (do InGameUIManager tạo ra)
    /// </summary>
    public void AttemptToSpawnUnit(UnitData unit)
    {
        // 1. Kiểm tra tiền
        if (Currency.Instance.SubMoney(unit.cost)) // SubMoney đã bao gồm cả kiểm tra
        {
            // 2. Đủ tiền -> Spawn
            Debug.Log("Đã mua lính: " + unit.unitName);

            // Bạn có thể dùng ObjectPoolManager nếu có
            ObjectPoolManager.SpawnObject(
                unit.unitPrefab,
                spawnPoint.position+offset,
                spawnPoint.rotation,
                ObjectPoolManager.PoolType.Unit // Giả sử bạn có PoolType này
            );
        }
        else
        {
            // 3. Không đủ tiền
            Debug.Log("Không đủ tiền để mua " + unit.unitName);
            // TODO: Phát âm thanh "không đủ tiền"
        }
    }
}