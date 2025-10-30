using UnityEngine;
using System.Linq; // Cần thêm Linq để dùng OrderBy

[RequireComponent(typeof(Collider))]
public class AutoHitbox : MonoBehaviour
{
    [Tooltip("Bán kính tối thiểu để đảm bảo nhân vật ổn định về mặt vật lý.")]
    public float minRadius = 0.3f; // Giữ lại biến này, nó vẫn rất hữu ích
    // Sử dụng [ContextMenu] để tạo một nút trong Inspector có thể nhấn được
    // Giúp bạn chạy logic này bất cứ khi nào bạn muốn trong Editor mode.
    [ContextMenu("Adjust Collider to Fit All Meshes")]
    private void AdjustColliderToBounds()
    {
        var skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedRenderers.Length == 0)
        {
            Debug.LogWarning("No SkinnedMeshRenderer found on this object or its children.", this);
            return;
        }

        Bounds combinedBounds = skinnedRenderers[0].bounds;
        for (int i = 1; i < skinnedRenderers.Length; i++)
            combinedBounds.Encapsulate(skinnedRenderers[i].bounds);

        Collider col = GetComponent<Collider>();

        Vector3 localCenter = transform.InverseTransformPoint(combinedBounds.center);
        Vector3 localSize = transform.InverseTransformVector(combinedBounds.size);
        localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));


        if (col is BoxCollider box)
        {
            // BoxCollider có thể giữ nguyên logic cũ
            box.center = localCenter;
            box.size = localSize;
            Debug.Log("BoxCollider adjusted.", this);
        }
        else if (col is CapsuleCollider capsule)
        {
            // --- SỬA LỖI LOGIC TẠI ĐÂY ---

            // 1. Chiều cao vẫn lấy từ localSize
            capsule.height = localSize.y;

            // 2. Bán kính vẫn lấy từ localSize.x/z VÀ so sánh với minRadius
            float calculatedRadius = Mathf.Max(localSize.x, localSize.z) / 2f;
            capsule.radius = Mathf.Max(calculatedRadius, minRadius);

            // 3. Tính toán lại tâm (Center)
            // Bỏ qua localCenter.y!
            // Tâm của một capsule đứng trên đất phải ở Y = (Height / 2)
            // Giữ lại X và Z của localCenter để nó vừa khít theo chiều ngang
            capsule.center = new Vector3(
                localCenter.x,
                capsule.height / 2f, // Đây là mấu chốt!
                localCenter.z
            );

            // --- KẾT THÚC SỬA LỖI ---

            Debug.Log($"CapsuleCollider adjusted. Center: {capsule.center}, Height: {capsule.height}, Radius: {capsule.radius}");
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