using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AnimateLineTexture : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = -2f;
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        // Tính toán độ dịch chuyển dựa trên thời gian và tốc độ
        float offset = Time.time * scrollSpeed;

        // Áp dụng độ dịch chuyển cho material của Line Renderer
        // "_MainTex" là tên mặc định cho texture chính trong hầu hết các shader
        lineRenderer.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }
}