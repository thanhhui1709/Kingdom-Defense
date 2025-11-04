using UnityEngine;

public class SpawnUnit : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("Vị trí mà lính sẽ được spawn ra")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float minXOffset=-10;
    [SerializeField] private float maxXOffset=-4;
    [SerializeField] private float minZOffset=2;
    [SerializeField] private float maxZOffset=6;

    /// <summary>
    /// Hàm này được gọi bởi các nút UI (do InGameUIManager tạo ra)
    /// </summary>
    public void AttemptToSpawnUnit(UnitData unit)
    {
        // 1. Kiểm tra tiền
        if (InGameMoney.Instance.SubMoney(unit.cost)) // SubMoney đã bao gồm cả kiểm tra
        {
            // 2. Đủ tiền -> Spawn
            Debug.Log("Đã mua lính: " + unit.unitName);

            Vector3 spawnPos = spawnPoint.position+new Vector3(Random.Range(minXOffset,maxXOffset),0,Random.Range(minZOffset,maxZOffset));
            // Bạn có thể dùng ObjectPoolManager nếu có
            ObjectPoolManager.SpawnObject(
                unit.unitPrefab,
                spawnPos,
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