// File: Scripts/Projectile/PoisonDamageProjectile.cs

using UnityEngine;
using System.Collections.Generic;

public class PoisonDamageProjectile : MonoBehaviour, IProjectile
{
    [Header("Thông số Đạn")]
    public float speed = 20f;
    private Transform target;

    [Header("Thông số Hiệu ứng Độc")]
    [Tooltip("Thời gian hiệu lực của mỗi lần độc.")]
    public float effectDuration = 3f;

    [Tooltip("Hệ số làm chậm (0 = đứng yên, 1 = không chậm).")]
    [Range(0, 1)]
    public float slowFactor = 0.3f;

    [Tooltip("Sát thương mỗi lần tick.")]
    private float damagePerTick = 2f; // Sát thương mỗi lần độc

    [Tooltip("Khoảng thời gian giữa các lần gây sát thương (tick).")]
    public float damageTickRate = 0.5f; // Tần suất gây sát thương

    [Tooltip("Số lần độc tối đa có thể cộng dồn.")]
    public int maxStacks = 3; // Giới hạn cộng dồn

    public Color debuffColor = Color.green;
    public AudioClip burnSound;

    public void Launch(List<GameObject> targets,float damage)
    {
        if (targets != null && targets.Count > 0 && targets[0] != null)
        {
            this.target = targets[0].transform;
            damagePerTick = damage;
        }
        else
        {
            // Sử dụng Object Pool hoặc Destroy tùy thuộc vào thiết lập của bạn
            ObjectPoolManager.ReturnObject(gameObject);
        }
    }

    void Update()
    {
        // ... (Logic di chuyển vẫn giữ nguyên)
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            ObjectPoolManager.ReturnObject(gameObject);
            return;
        }

        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
    }

    // Trong class PoisonDamageProjectile

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            PoisonDamageEffect effect = other.GetComponent<PoisonDamageEffect>();

            if (effect == null)
            {
                // ... (Phần thêm component như cũ, gán tất cả các thông số tĩnh)
                effect = other.gameObject.AddComponent<PoisonDamageEffect>();
                effect.burnSound = this.burnSound;
                effect.damageTickRate = this.damageTickRate;
                effect.maxStacks = this.maxStacks;
                effect.poisonColor = this.debuffColor;
            }

            // GỌI HÀM VỚI THÔNG SỐ TƯƠNG ỨNG CỦA VIÊN ĐẠN NÀY
            effect.ApplyStack(this.effectDuration, this.slowFactor, this.damagePerTick);

            ObjectPoolManager.ReturnObject(gameObject);
        }
    }
}