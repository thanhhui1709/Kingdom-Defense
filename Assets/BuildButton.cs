using UnityEngine;

public class BuildButton : MonoBehaviour
{
    [Header("Kéo prefab đúng tower vào đây")]
    public GameObject towerPrefab;

    [Header("Vị trí/cha để spawn (tùy chọn)")]
    public Transform spawnParent;
    public Transform defaultBuildPoint; // điểm build mặc định nếu bạn chưa dùng raycast

    // Gọi hàm này từ OnClick của Button
    public void OnClickBuild()
    {
        if (towerPrefab == null) { Debug.LogWarning("Chưa gán towerPrefab!"); return; }

        Vector3 spawnPos = defaultBuildPoint != null ? defaultBuildPoint.position : Vector3.zero;
        var go = Instantiate(towerPrefab, spawnPos, Quaternion.identity, spawnParent);

        // (tuỳ chọn) đặt Layer/Tag nếu cần giống trong Prefab
        // go.tag = "Tower";
        // go.layer = LayerMask.NameToLayer("Target");
    }
}
