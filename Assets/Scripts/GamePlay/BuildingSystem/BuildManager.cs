using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    [Header("Sound Effect")]
    public AudioClip buildSound;
  
    public AudioClip sellSound;
    [Header("Tower Data")]
    public List<TowerData> availableTowers = new List<TowerData>();

    [Header("Core References")]
    [SerializeField] private InGameUIManager uiManager;
    [SerializeField] private LayerMask targetLayer;

    // Lưu lại các ô đất được chọn cho logic
    private Transform selectedBuildableTile; // Ô đất trống để xây
    private BuildableTile selectedTileForDemolish; // Ô đất có trụ để nâng cấp/bán

    void Start()
    {
        // Yêu cầu UIManager tạo các nút, truyền vào danh sách trụ và CHÍNH NÓ
        uiManager.PopulateBuyTowerMenu(availableTowers, this);

        // Lắng nghe sự kiện trụ được nâng cấp/tiến hóa
        GameEvent.Instance.SubscribeTowerLevelUp(OnTowerUpgradedOrEvolved);
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, targetLayer))
            {
                BuildableTile tile = hit.collider.GetComponent<BuildableTile>();

                if (tile != null)
                {
                    if (tile.towerOnTile != null)
                    {
                        // Click vào trụ đã xây:
                        selectedTileForDemolish = tile; // Lưu lại
                        selectedBuildableTile = null;   // Xóa chọn ô trống
                        uiManager.ShowUpgradePanel(tile); // Yêu cầu UI hiện panel nâng cấp
                        uiManager.ToggleBuyTowerPanel(false); // Ẩn panel mua
                    }
                    else
                    {
                        // Click vào ô trống:
                        selectedBuildableTile = hit.transform; // Lưu lại
                        selectedTileForDemolish = null;       // Xóa chọn trụ
                        uiManager.ToggleBuyTowerPanel(true);  // Yêu cầu UI hiện panel mua
                        uiManager.ToggleUpgradeTowerPanel(false); // Ẩn panel nâng cấp
                    }
                }
                else
                {
                    // Click ra ngoài
                    DeselectAll();
                }
            }
            else
            {
                // Click ra ngoài
                DeselectAll();
            }
        }
    }

    // Hàm public để UIManager có thể lấy ô đất đang chọn
    public BuildableTile GetSelectedTileForDemolish()
    {
        return selectedTileForDemolish;
    }

    /// <summary>
    /// Bỏ chọn tất cả và ẩn mọi UI
    /// </summary>
    private void DeselectAll()
    {
        selectedBuildableTile = null;
        selectedTileForDemolish = null;
        uiManager.ToggleBuyTowerPanel(false);
        uiManager.ToggleUpgradeTowerPanel(false);
    }

    /// <summary>
    /// Hàm xử lý sự kiện khi trụ thay đổi (do tiến hóa hoặc lên cấp)
    /// </summary>
    private void OnTowerUpgradedOrEvolved(GameObject newOrUpdatedTower)
    {
        // 1. Kiểm tra xem có đang chọn ô nào không
        if (selectedTileForDemolish == null) return;

        // 2. Kiểm tra xem trụ mới/cập nhật có phải là trụ đang chọn không
        // (So sánh vị trí là cách an toàn nhất vì trụ cũ có thể đã bị destroy)
        if (Vector3.Distance(newOrUpdatedTower.transform.position, selectedTileForDemolish.transform.position) < 0.1f)
        {
            // 3. CẬP NHẬT THAM CHIẾU
            selectedTileForDemolish.towerOnTile = newOrUpdatedTower;

            // 4. Yêu cầu UI cập nhật lại thông tin
            uiManager.ShowUpgradePanel(selectedTileForDemolish);
        }
    }

    // --- Logic Xây ---
    public void SelectAndPlaceTower(TowerData towerToBuild)
    {
        if (selectedBuildableTile == null) return;

        // Lấy chi phí xây dựng từ prefab
        int buildCost = towerToBuild.buildCost;

        // --- KIỂM TRA TIỀN (XÂY MỚI) ---
        if (!InGameMoney.Instance.CheckBalance(buildCost))
        {
            Debug.Log("Không đủ tiền xây trụ!");
            return; // Dừng
        }

        // Đủ tiền, trừ tiền
        InGameMoney.Instance.SubMoney(buildCost);
        // --- KẾT THÚC LOGIC TIỀN ---

        GameObject newTower = ObjectPoolManager.SpawnObject(towerToBuild.towerPrefab, selectedBuildableTile.position, Quaternion.identity, ObjectPoolManager.PoolType.Tower);
        ObjectPoolManager.PlayAudio(buildSound, selectedBuildableTile.position, 1.0f);

        // --- GÁN TIỀN ĐẦU TƯ BAN ĐẦU ---
        Stats newTowerStats = newTower.GetComponent<Stats>();
        if (newTowerStats != null)
        {
            newTowerStats.TotalInvestedMoney = buildCost;
        }
        // --- KẾT THÚC ---

        BuildableTile tileScript = selectedBuildableTile.GetComponent<BuildableTile>();
        tileScript.towerOnTile = newTower;

        DeselectAll();
    }

    // --- Logic Nâng Cấp & Bán ---
    public void UpgradeSelectedTower()
    {
        if (selectedTileForDemolish == null || selectedTileForDemolish.towerOnTile == null) return;

        LevelController levelController = selectedTileForDemolish.towerOnTile.GetComponent<LevelController>();
        if (levelController != null)
        {
            levelController.LevelUp();
            // LevelUp() sẽ tự bắn sự kiện -> OnTowerUpgradedOrEvolved() sẽ bắt
        }
    }

    public void SellSelectedTower()
    {
        if (selectedTileForDemolish == null) return;

        GameObject towerToSell = selectedTileForDemolish.towerOnTile;
        Stats towerStats = towerToSell.GetComponent<Stats>();

        if (towerStats != null)
        {
            // --- LOGIC BÁN TRỤ (70%) ---
            int sellAmount = (int)(towerStats.TotalInvestedMoney * 0.7f);
            InGameMoney.Instance.AddMoney(sellAmount);
            // --- KẾT THÚC ---
        }
        else
        {
            Debug.LogError("Trụ bị bán không có Stats!");
        }

        ObjectPoolManager.ReturnObject(towerToSell);
        ObjectPoolManager.PlayAudio(sellSound, selectedTileForDemolish.transform.position, 1.0f);
        selectedTileForDemolish.towerOnTile = null;

        DeselectAll();
    }
}