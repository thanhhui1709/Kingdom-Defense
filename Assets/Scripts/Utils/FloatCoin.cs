using DG.Tweening;
using TMPro;
using UnityEngine;
using System.Collections;

public class FloatCoin : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh; // Kéo TextMeshPro vào đây

    [Header("Animation")]
    [SerializeField] private float moveAmount = 1.5f; // Bay cao bao nhiêu
    [SerializeField] private float duration = 1f;     // Thời gian tồn tại

    private Camera mainCamera;
    private Transform myTransform;

    private void Awake()
    {
        mainCamera = Camera.main;
        myTransform = transform;
    }

    /// <summary>
    /// Bắt đầu hiệu ứng
    /// </summary>
    public void Launch(string text)
    {
        // 1. Reset
        textMesh.text = text;
        textMesh.alpha = 1f; // Đảm bảo text hiện rõ

        // 2. Xoay về phía camera
        if (mainCamera != null)
        {
            myTransform.rotation = mainCamera.transform.rotation;
        }

        // 3. Dùng DOTween để bay và mờ

        // Bay lên
        myTransform.DOMoveY(myTransform.position.y + moveAmount, duration)
            .SetEase(Ease.OutQuad); // Bay nhanh rồi chậm dần

        // Mờ dần (Fade out)
        textMesh.DOFade(0f, duration)
            .SetEase(Ease.InQuad); // Mờ chậm rồi nhanh dần

        // 4. Tự trả về Pool sau khi xong
        StartCoroutine(ReturnToPool(duration));
    }

    private IEnumerator ReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.ReturnObject(gameObject);
    }
}
