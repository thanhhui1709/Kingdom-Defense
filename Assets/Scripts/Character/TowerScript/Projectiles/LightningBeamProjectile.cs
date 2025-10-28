using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningBeam : MonoBehaviour
{
    [SerializeField] int segments = 20;            // >= 2
    [SerializeField] float amplitude = 0.5f;       // biên độ jitter
    [SerializeField] float frequency = 1.5f;       // tần số noise theo không gian
    [SerializeField] float speed = 3f;             // speed của noise theo thời gian
    [SerializeField] Vector3 up = Vector3.up;      // dùng để tính perpendicular
    [SerializeField] bool usePerpendicular = true; // nếu false thì jitter trong 3D
    [SerializeField] bool useWorldSpace = true;

    private LineRenderer lr;
    private Transform shooter;
    private Transform target;
    private float seed;
    private float damagePerSecond;
    private EnemyHealth targetHealth;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = Mathf.Max(2, segments);
        lr.useWorldSpace = useWorldSpace;
        seed = Random.Range(-1000f, 1000f);
    }

    public void Launch(Transform launchPoint, List<GameObject> targets, float damage)
    {

        this.shooter = launchPoint;
        this.target = targets.FirstOrDefault().transform;
        this.damagePerSecond = damage;
        this.targetHealth = target.GetComponent<EnemyHealth>();

    }

    void Update()
    {
        if (shooter == null || target == null) return;
        if (!target.gameObject.activeInHierarchy)
        {
            ObjectPoolManager.ReturnObject(gameObject);

            return;
        }

        Vector3 start = shooter.position;
        Vector3 end = target.position;

        // đảm bảo line renderer có đủ điểm
        int seg = Mathf.Max(2, segments);
        if (lr.positionCount != seg) lr.positionCount = seg;

        // hướng chính và perpendicular
        Vector3 dir = (end - start);
        float totalDist = dir.magnitude;
        Vector3 forward = dir.normalized;

        Vector3 perp = Vector3.Cross(forward, up).normalized;
        if (perp.sqrMagnitude < 0.0001f) // nếu forward trùng với up -> dùng khác
            perp = Vector3.Cross(forward, Vector3.forward).normalized;

        // build positions vào mảng (hiệu năng tốt hơn gọi SetPosition nhiều lần)
        Vector3[] positions = new Vector3[seg];

        for (int i = 0; i < seg; i++)
        {
            float t = (float)i / (seg - 1);                 // 0..1
            Vector3 basePos = Vector3.Lerp(start, end, t); // điểm dọc theo đường

            // noise: dùng perlin 2D (x = seed + i*frequency, y = Time*timeSpeed)
            float nx = seed + i * frequency;
            float ny = Time.time * speed;
            float p = Mathf.PerlinNoise(nx, ny);           // 0..1
            float n = (p - 0.5f) * 2f;                     // -1..1

            // tính offset vuông góc (càng xa seed có thể khác)
            Vector3 offset = Vector3.zero;
            if (usePerpendicular)
            {
                offset = perp * n * amplitude;
            }
            else
            {
                // jitter 3D nhỏ
                offset = Random.onUnitSphere * amplitude * 0.3f; // nếu muốn 3D, nhưng Random gây flicker -> tránh
            }

            // giảm jitter ở 2 đầu (start/end) để giữ chắc điểm nối
            float edgeFade = 1f;
            float edgeDist = 0.15f; // phần tỷ lệ hai đầu giảm dần
            if (t < edgeDist) edgeFade = Mathf.InverseLerp(0f, edgeDist, t);           // ~0->1
            else if (t > 1f - edgeDist) edgeFade = Mathf.InverseLerp(1f, 1f - edgeDist, t);

            positions[i] = basePos + offset * edgeFade;
        }

        lr.SetPositions(positions);
    }
}
