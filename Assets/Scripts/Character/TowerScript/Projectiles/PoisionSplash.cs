using UnityEngine;
using DG.Tweening; 

public class PoisionSplash : MonoBehaviour
{
    public Transform parent;
    [Header("Thông số Đạn")]
    public float existTime = 5f;
    public float effectDuration = 3f;

    [Header("Effect")]
    public GameObject splashEffect;

    [Tooltip("Kích thước cuối cùng (Scale) sau khi to dần.")]
    public Vector3 targetScale = Vector3.one;

    [Tooltip("Thời gian để đạt đến kích thước cuối cùng.")]
    public float scaleUpDuration = 0.5f;


    [Tooltip("Hệ số làm chậm (0 = đứng yên, 1 = không chậm).")]
    [Range(0, 1)]
    public float slowFactor = 0.5f;
    public float damagePerTick = 5f;
    public float damageTickRate = 0.5f;
    public int maxStacks = 4;
    public Color debuffColor = Color.green;
    public AudioClip poisonSound;
    public AudioClip burnSound;
    private float remainingExistTime;

  
    private void OnEnable()
    {
        remainingExistTime = existTime;

        // 1. Lưu kích thước mục tiêu
        Vector3 finalScale = transform.localScale;

        // 2. Thiết lập kích thước ban đầu về 0
        transform.localScale = Vector3.zero;

        // 3. Sử dụng DOTween để to dần
        transform.DOScale(finalScale, scaleUpDuration)
                 .SetEase(Ease.OutBack); // Hiệu ứng nảy nhẹ cho đẹp

        // Tùy chọn: Bật hiệu ứng Particle nếu có
        if (splashEffect != null)
        {
            ObjectPoolManager.SpawnObject(splashEffect, transform.position+new Vector3(0,2,0), Quaternion.identity, ObjectPoolManager.PoolType.Particle);
        }
         InvokeRepeating("PlayAxitSound", 0f, 2.5f);

    }

    // ... (Hàm Update() giữ nguyên)
    void FixedUpdate()
    {
        remainingExistTime -= Time.deltaTime;
        if (remainingExistTime <= 0f)
        {
            

            // Dừng DOTween để tránh lỗi khi trả về Pool
            transform.DOKill();
            ObjectPoolManager.ReturnObject(parent.gameObject);
            CancelInvoke("PlayAxitSound");
        }
    }

    // --- SỬA ĐỔI LOGIC VA CHẠM ---
    // Vì đây là hiệu ứng tồn tại (Splash), ta không được hủy nó trong hàm va chạm.
    private void OnTriggerStay(Collider collision)
    {
      
        if (collision.CompareTag("Enemy"))
        {
           

            PoisonDamageEffect effect = collision.GetComponent<PoisonDamageEffect>();

            if (effect == null)
            {
                // Thêm component lần đầu
                effect = collision.gameObject.AddComponent<PoisonDamageEffect>();
                effect.burnSound = this.burnSound;
                effect.damageTickRate = this.damageTickRate;
                effect.maxStacks = this.maxStacks;
                effect.poisonColor = this.debuffColor;

                // Áp dụng Stack đầu tiên
                effect.ApplyStack(this.effectDuration, this.slowFactor, this.damagePerTick);
            
            }
            else
            {
                
                effect.RefreshEffect();
            }

        
        }
    }
    private void PlayAxitSound()
    {
        ObjectPoolManager.PlayAudio(poisonSound, transform.position, 1.0f);
    }
}