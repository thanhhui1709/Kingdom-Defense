// File: ContinuousBeamManager.cs (Phiên bản tối ưu)
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq; // Cần thiết cho .ToList()

// Component này sẽ chủ động quản lý các tia sét mỗi frame
public class ContinuousBeamManager : MonoBehaviour
{
    // --- CÁC BIẾN THAM CHIẾU ---
    private TowerController towerController; // Tham chiếu đến script điều khiển chính
    private Stats stats;
    [SerializeField]
    private Transform shooterTransform; // Vị trí bắn, lấy từ TowerController
    public ParticleSystem laserEffect;
    public AudioClip laserAudio;
    public AudioSource laserAudioSource;


    // --- CÁC BIẾN TRẠNG THÁI ---
    private Dictionary<GameObject, GameObject> activeBeams = new Dictionary<GameObject, GameObject>();
   
    // --- CÁC BIẾN CÀI ĐẶT ---
    [Tooltip("Prefab của tia sét")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Vector3 offsetPos=new Vector3(0,2,0);

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
        laserAudioSource.clip = laserAudio;


    }
    private void OnEnable()
    {
        InvokeRepeating(nameof(CheckEnemyExist), 0f, 0.25f);
    }

    // Update chạy mỗi frame, không còn phụ thuộc vào DoAttack
    void Update()
    {
        // Lấy danh sách mục tiêu mới nhất trực tiếp từ TowerController
        // Chuyển từ HashSet sang List để dễ làm việc
        List<GameObject> currentTargets = towerController.GetCurrentTargets().ToList();

    
        List<GameObject> targetsToRemove = new List<GameObject>();
        foreach (var pair in activeBeams)
        {
            GameObject target = pair.Key;

          
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
                    beamScript.Launch(shooterTransform.position+offsetPos, target, stats.AttackDamage);
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
    private void CheckEnemyExist()
    {
     
        if(towerController.GetCurrentTargets().Count == 0)
        {
            laserAudioSource.Pause();
            laserEffect.Stop();
        }
        else
        {
            if (!laserEffect.isPlaying)
            {

               
                laserEffect.Play();
            }
            if (!laserAudioSource.isPlaying)
            {
                laserAudioSource.Play();
            }
        }
    }
}