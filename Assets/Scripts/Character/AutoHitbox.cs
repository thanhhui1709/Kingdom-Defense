using UnityEngine;
using System.Linq; // Cần thêm Linq để dùng OrderBy

[RequireComponent(typeof(Collider))]
public class AutoHitbox : MonoBehaviour
{
    // Sử dụng [ContextMenu] để tạo một nút trong Inspector có thể nhấn được
    // Giúp bạn chạy logic này bất cứ khi nào bạn muốn trong Editor mode.
    [ContextMenu("Adjust Collider to Fit All Meshes")]
    private void AdjustColliderToBounds()
    {
        // Lấy tất cả SkinnedMeshRenderer trong các object con
        var skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedRenderers.Length == 0)
        {
            Debug.LogWarning("No SkinnedMeshRenderer found on this object or its children.", this);
            return;
        }

        // Tạo một Bounds bao quanh tất cả các mesh
        // Bắt đầu với bounds của mesh đầu tiên
        Bounds combinedBounds = skinnedRenderers[0].bounds;
        for (int i = 1; i < skinnedRenderers.Length; i++)
        {
            // Mở rộng Bounds để nó bao gồm cả bounds của các mesh tiếp theo
            combinedBounds.Encapsulate(skinnedRenderers[i].bounds);
        }

        Collider col = GetComponent<Collider>();

        // Chuyển đổi center từ world space về local space của object
        Vector3 localCenter = transform.InverseTransformPoint(combinedBounds.center);

        // Chuyển đổi size từ world space về local space.
        // Đây là cách đơn giản và hoạt động tốt nếu object không bị xoay lệch trục.
        Vector3 localSize = transform.InverseTransformVector(combinedBounds.size);
        // Lấy giá trị tuyệt đối vì scale âm có thể tạo size âm
        localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));


        if (col is BoxCollider box)
        {
            box.center = localCenter;
            box.size = localSize;
            Debug.Log("BoxCollider adjusted.", this);
        }
        else if (col is CapsuleCollider capsule)
        {
            capsule.center = localCenter;
            capsule.height = localSize.y;
            capsule.radius = Mathf.Max(localSize.x, localSize.z) / 2f;
            Debug.Log("CapsuleCollider adjusted.", this);
        }
    }

    // Sửa lại Gizmos để vẽ đúng hình dạng và dễ nhìn hơn
    void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = Color.cyan; // Đổi màu cho dễ thấy
        Gizmos.matrix = transform.localToWorldMatrix; // Áp dụng transform của object cho Gizmos

        if (col is BoxCollider box)
        {
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is CapsuleCollider capsule)
        {
            // Vẽ Capsule phức tạp hơn, ta có thể tạm vẽ một khối hộp để hình dung
            Vector3 capsuleBoundsSize = new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2);
            Gizmos.DrawWireCube(capsule.center, capsuleBoundsSize);
        }
    }
}