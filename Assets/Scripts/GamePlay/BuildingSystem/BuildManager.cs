using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
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

        // (Logic kiểm tra tiền)
        // if (GameManager.Instance.Money < towerToBuild.buildCost) return;

        GameObject newTower = ObjectPoolManager.SpawnObject(towerToBuild.towerPrefab, selectedBuildableTile.position, Quaternion.identity, ObjectPoolManager.PoolType.Tower);

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

        // (Logic hoàn tiền)
        // Stats towerStats = towerToSell.GetComponent<Stats>();
        // GameManager.Instance.AddMoney(towerStats.GetSellValue());

        ObjectPoolManager.ReturnObject(towerToSell);
        selectedTileForDemolish.towerOnTile = null;

        DeselectAll();
    }
}