// File: ContinuousBeamManager.cs (Phiên bản tối ưu)
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Cần thiết cho .ToList()

// Component này sẽ chủ động quản lý các tia sét mỗi frame
public class ContinuousBeamManager : MonoBehaviour
{
    // --- CÁC BIẾN THAM CHIẾU ---
    private TowerController towerController; // Tham chiếu đến script điều khiển chính
    private Stats stats;
    [SerializeField]
    private Transform shooterTransform; // Vị trí bắn, lấy từ TowerController

    // --- CÁC BIẾN TRẠNG THÁI ---
    private Dictionary<GameObject, GameObject> activeBeams = new Dictionary<GameObject, GameObject>();

    // --- CÁC BIẾN CÀI ĐẶT ---
    [Tooltip("Prefab của tia sét")]
    [SerializeField] private GameObject projectilePrefab;

    void Awake()
    {
        // Lấy các component cần thiết trên cùng một trụ
        towerController = GetComponent<TowerController>();
        stats = GetComponent<Stats>();

        // Kiểm tra lỗi để đảm bảo thiết lập đúng
        if (towerController == null)
        {
            Debug.LogError("ContinuousBeamManager yêu cầu phải có TowerController trên cùng GameObject!", this);
            this.enabled = false; // Vô hiệu hóa script nếu không có TowerController
            return;
        }

        // Lấy vị trí bắn từ TowerController (bạn cần đảm bảo biến 'shooter' trong TowerController không phải private)
        // Nếu nó là private, bạn cần tạo một getter cho nó giống như GetCurrentTargets()
        // Hoặc bạn có thể gán trực tiếp vào đây qua Inspector
        // Giả sử bạn có một getter tên là GetShooterTransform() trong TowerController
        // shooterTransform = towerController.GetShooterTransform();
    }

    // Update chạy mỗi frame, không còn phụ thuộc vào DoAttack
    void Update()
    {
        // Lấy danh sách mục tiêu mới nhất trực tiếp từ TowerController
        // Chuyển từ HashSet sang List để dễ làm việc
        List<GameObject> currentTargets = towerController.GetCurrentTargets().ToList();

        // --- 1. DỌN DẸP TIA SÉT KHÔNG HỢP LỆ ---
        // Sử dụng một List riêng để tránh lỗi khi sửa Dictionary trong lúc duyệt
        List<GameObject> targetsToRemove = new List<GameObject>();
        foreach (var pair in activeBeams)
        {
            GameObject target = pair.Key;

            // Một tia sét bị coi là không hợp lệ nếu:
            // - Mục tiêu đã bị hủy (null)
            // - Mục tiêu không còn active
            // - Mục tiêu không còn nằm trong danh sách mục tiêu chính thức của TowerController
            if (target == null || !target.activeInHierarchy || !currentTargets.Contains(target))
            {
                targetsToRemove.Add(target);
                ObjectPoolManager.ReturnObject(pair.Value); // Dùng hàm trả pool của bạn
            }
        }

        // Thực hiện xóa khỏi dictionary
        foreach (var target in targetsToRemove)
        {
            activeBeams.Remove(target);
        }

        // --- 2. TẠO TIA SÉT MỚI CHO CÁC MỤC TIÊU HỢP LỆ ---
        // Lấy vị trí bắn từ shooter của TowerController
       

        foreach (GameObject target in currentTargets)
        {
            // Nếu mục tiêu này hợp lệ và CHƯA có tia sét nào bắn vào nó
            if (target != null && !activeBeams.ContainsKey(target))
            {
                // Lấy prefab từ TowerController hoặc gán trực tiếp vào đây
                GameObject beamGO = ObjectPoolManager.SpawnObject(projectilePrefab, shooterTransform.position, Quaternion.identity, ObjectPoolManager.PoolType.TowerProjectile);
                LightningBeam beamScript = beamGO.GetComponent<LightningBeam>();

                if (beamScript != null)
                {
                    // Ra lệnh cho tia sét tấn công
                    beamScript.Launch(shooterTransform, target, stats.AttackDamage);
                    // Lưu lại để quản lý
                    activeBeams.Add(target, beamGO);
                }
                else
                {
                    Debug.LogError("Prefab tia điện thiếu script LightningBeam!");
                    ObjectPoolManager.ReturnObject(beamGO);
                }
            }
        }
    }

    // Quan trọng: Dọn dẹp tất cả khi trụ bị vô hiệu hóa hoặc phá hủy
    private void OnDisable()
    {
        foreach (var pair in activeBeams)
        {
            if (pair.Value != null)
            {
                ObjectPoolManager.ReturnObject(pair.Value);
            }
        }
        activeBeams.Clear();
    }
}