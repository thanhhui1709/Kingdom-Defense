using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util 
{
    public static HashSet<GameObject> FindGameObjectInRange(HashSet<GameObject> existedTarget,Transform center, float range,string tag)
    {
        
        Collider[] hits = Physics.OverlapSphere(center.transform.position, range);
        foreach (var hit in hits)
        {
            if(hit.gameObject.CompareTag(tag) && hit.gameObject != center.gameObject)
            {
                if (!existedTarget.Contains(hit.gameObject))
                {
                    existedTarget.Add(hit.gameObject);
                }
            }         
        }

        return existedTarget;
    }
    public static void FindTargetsWithChildColliders<T>(HashSet<GameObject> results, Vector3 origin, float range, LayerMask targetLayer) where T : Component
    {
        // 1. Dùng OverlapSphere để lấy tất cả collider trong tầm
        Collider[] collidersInRange = Physics.OverlapSphere(origin, range, targetLayer);

        // 2. Duyệt qua từng collider tìm được
        foreach (var col in collidersInRange)
        {
            Debug.Log("Found Collider: " + col.name);   
            // 3. Dùng GetComponentInParent để tìm component T
            // Nó sẽ kiểm tra object có collider, sau đó đi ngược lên cây thư mục để tìm.
            T targetComponent = col.GetComponentInParent<T>();

            // 4. Nếu tìm thấy component T
            if (targetComponent != null)
            {
                // Thêm GameObject chứa component T (là object cha) vào danh sách kết quả.
                // HashSet sẽ tự động xử lý việc không thêm trùng lặp.
                results.Add(targetComponent.gameObject);

            }
        }
    }
    public static void RotateToward(Transform from, Vector3 to, float speed)
    {
        Vector3 direction = (to - from.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction,from.up);
        from.rotation = Quaternion.Lerp(from.rotation, lookRotation, Time.deltaTime * speed);
    }
    public static Vector3 MoveToward(Rigidbody from, Transform to, float speed)
    {
        Vector3 direction = (to.position - from.position).normalized;
        from.linearVelocity = direction * speed;


        return direction;
    }
    public static float CalculateDamage(float initialDamage, float armor)
    {
        float damageMultiplier;

        if (armor >= 0)
        {
            // CÔNG THỨC GIẢM SÁT THƯƠNG (DIMINISHING RETURNS)
            // Khi giáp = 100, sát thương giảm 50%.
            // Khi giáp tăng lên 200, sát thương chỉ giảm thêm ~16.7%, tổng là 66.7%.
            // Giáp càng cao, hiệu quả giảm trừ càng ít đi.
            damageMultiplier = 100f / (100f + armor);
        }
        else
        {
            // CÔNG THỨC KHUẾCH ĐẠI SÁT THƯƠNG (GIÁP ÂM)
            // Mỗi 1 điểm giáp âm sẽ tăng sát thương nhận vào thêm 1%.
            // Ví dụ: -50 giáp sẽ khiến mục tiêu nhận thêm 50% sát thương (nhân 1.5 lần).
            damageMultiplier = 1f - (armor * 0.01f);
        }

        float finalDamage = initialDamage * damageMultiplier;

        // Đảm bảo sát thương cuối cùng không bao giờ là số âm
        return Mathf.Max(0, finalDamage);
    }
}
