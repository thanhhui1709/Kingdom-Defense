// File: LightningBeam.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningBeam : MonoBehaviour
{
   
    [Header("Visual Effects")]
    [SerializeField] int segments = 20;            // Số đoạn của tia sét (>= 2)
    [SerializeField] float amplitude = 0.5f;       // Biên độ cong của tia sét
    [SerializeField] float frequency = 1.5f;       // Tần số nhiễu (noise)
    [SerializeField] float speed = 3f;             // Tốc độ thay đổi của hiệu ứng theo thời gian
    [SerializeField] Vector3 up = Vector3.up;      // Vector dùng để tính phương vuông góc
    [SerializeField] bool usePerpendicular = true; // Nếu false, tia sét sẽ cong trong không gian 3D
    [SerializeField] bool useWorldSpace = true;    // Nên để true để tia sét nối đúng giữa 2 vật thể
    
    // Các biến trạng thái của tia sét
    private LineRenderer lr;
    private Vector3 shooter;
    private Transform target;
    private EnemyHealth targetHealth; // Thay "EnemyHealth" bằng tên script máu của đối thủ
    private float damagePerSecond;
    private float seed; // Hạt giống ngẫu nhiên để mỗi tia sét có hình dạng khác nhau

    void Awake()
    {
        // Lấy component LineRenderer khi đối tượng được tạo
        lr = GetComponent<LineRenderer>();
        lr.positionCount = Mathf.Max(2, segments);
        lr.useWorldSpace = useWorldSpace;

        // Tạo một seed ngẫu nhiên để hiệu ứng Perlin Noise không bị trùng lặp giữa các tia sét
        seed = Random.Range(-1000f, 1000f);
    }

    private void OnDisable()
    {
        // Rất quan trọng cho Object Pooling:
        // Reset lại trạng thái khi tia sét được trả về pool để sẵn sàng cho lần sử dụng sau.
        shooter = Vector3.zero;
        target = null;
        targetHealth = null;
    }

    /// <summary>
    /// Hàm khởi động, được gọi bởi ContinuousBeamManager.
    /// </summary>
    /// <param name="launchPoint">Vị trí bắn (thường là một đối tượng con của trụ).</param>
    /// <param name="singleTarget">Mục tiêu duy nhất mà tia sét này sẽ tấn công.</param>
    /// <param name="damageFromTower">Sát thương mỗi giây được truyền từ chỉ số của trụ.</param>
    public void Launch(Vector3 launchPoint, GameObject singleTarget, float damageFromTower)
    {
        this.shooter = launchPoint;
        this.target = singleTarget.transform;
        this.damagePerSecond = damageFromTower;
        this.targetHealth = singleTarget.GetComponent<EnemyHealth>(); // Lấy component máu của mục tiêu

       
    }

    void Update()
    {
        // Nếu không có trụ hoặc mục tiêu, ngừng xử lý.
        // ContinuousBeamManager sẽ chịu trách nhiệm dọn dẹp và trả tia sét này về pool.
        if (shooter == null || target == null)
        {
            return;
        }

        // Cập nhật hiệu ứng hình ảnh và gây sát thương trong mỗi frame
        UpdateVisuals();
        DealContinuousDamage();
    }

    /// <summary>
    /// Cập nhật vị trí các điểm của LineRenderer để tạo hiệu ứng sét giật.
    /// </summary>
    private void UpdateVisuals()
    {
        Vector3 start = shooter;
        Vector3 end = target.position;

        // Đảm bảo LineRenderer có đủ số điểm
        int seg = Mathf.Max(2, segments);
        if (lr.positionCount != seg) lr.positionCount = seg;

        // Hướng chính và phương vuông góc
        Vector3 dir = (end - start);
        Vector3 forward = dir.normalized;
        Vector3 perp = Vector3.Cross(forward, up).normalized;
        if (perp.sqrMagnitude < 0.0001f) // Xử lý trường hợp hướng bắn song song với vector "up"
            perp = Vector3.Cross(forward, Vector3.right).normalized;

        // Tạo một mảng để chứa vị trí các điểm (hiệu năng tốt hơn gọi SetPosition nhiều lần)
        Vector3[] positions = new Vector3[seg];

        for (int i = 0; i < seg; i++)
        {
            float t = (float)i / (seg - 1);                 // Tỷ lệ vị trí từ 0 đến 1
            Vector3 basePos = Vector3.Lerp(start, end, t);  // Vị trí cơ bản trên đường thẳng

            // Sử dụng Perlin Noise để tạo ra giá trị ngẫu nhiên mượt mà
            float nx = seed + i * frequency;
            float ny = Time.time * speed;
            float p = Mathf.PerlinNoise(nx, ny);            // Giá trị từ 0..1
            float n = (p - 0.5f) * 2f;                      // Chuyển về khoảng -1..1

            // Tính toán độ lệch so với đường thẳng
            Vector3 offset = Vector3.zero;
            if (usePerpendicular)
            {
                offset = perp * n * amplitude;
            }
            else
            {
                // Jitter 3D (ít được dùng hơn vì có thể gây rung lắc khó kiểm soát)
                offset = new Vector3(Mathf.PerlinNoise(nx, ny) - 0.5f, Mathf.PerlinNoise(ny, nx) - 0.5f, 0) * (2f * amplitude);
            }

            // Giảm độ lệch ở hai đầu để tia sét luôn nối chính xác vào trụ và mục tiêu
            float edgeFade = 1f;
            float edgeDist = 0.15f; // 15% ở mỗi đầu sẽ bị giảm hiệu ứng
            if (t < edgeDist) edgeFade = Mathf.InverseLerp(0f, edgeDist, t);
            else if (t > 1f - edgeDist) edgeFade = Mathf.InverseLerp(1f, 1f - edgeDist, t);

            positions[i] = basePos + offset * edgeFade;
        }

        lr.SetPositions(positions);
    }

    /// <summary>
    /// Gây sát thương liên tục cho mục tiêu.
    /// </summary>
    private void DealContinuousDamage()
    {
        if (targetHealth != null)
        {
            // Sát thương trong frame này = Sát thương mỗi giây * thời gian của frame
            targetHealth.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}