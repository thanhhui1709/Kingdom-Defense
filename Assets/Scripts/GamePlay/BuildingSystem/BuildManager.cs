using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    [Header("Tower Data")]
    // Danh sách chứa tất cả các loại trụ có thể xây.
    public List<TowerData> availableTowers = new List<TowerData>();

    [Header("UI References")]
    // Panel chính để chứa các nút.
    public GameObject towerSelectionPanel;
    // Prefab của nút bấm 
    public GameObject towerButtonPrefab;
    // Biến tạm để lưu ô đất đã chọn.
    private Transform selectedBuildableTile;

    [Header("Demolish UI References")]
    public GameObject demolishPanel;
    public Button sellButton;
    private BuildableTile selectedTileForDemolish; // Lưu lại ô đất được chọn để phá

    [Header("Upgrade")]
    public Button upgradeButton;

    public LayerMask targetLayer;

    void Start()
    {
        towerSelectionPanel.SetActive(false);
        demolishPanel.SetActive(false); // Ẩn panel bán trụ
        GenerateTowerButtons();
        GameEvent.Instance.SubscribeTowerLevelUp(OnBuildableTileChanged);
        // Gán sự kiện cho nút bán trụ
        sellButton.onClick.AddListener(SellTower);
    }

    // Hàm này sẽ tự động tạo các nút chọn trụ.
    void GenerateTowerButtons()
    {
        // Xóa các nút cũ nếu có
        foreach (Transform child in towerSelectionPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Tạo nút mới cho mỗi loại trụ trong danh sách
        foreach (TowerData towerData in availableTowers)
        {
            // Tạo một bản sao của prefab nút
            GameObject buttonGO = Instantiate(towerButtonPrefab, towerSelectionPanel.transform);

            // Tìm component TextMeshPro bên trong nút con
            TMPro.TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                // Gán tên của trụ từ TowerData vào chữ của nút
                buttonText.text = towerData.towerName;
            }

            // Gán sự kiện OnClick cho nút
            Button newButton = buttonGO.GetComponent<Button>();
            TowerData currentTowerData = towerData;
            newButton.onClick.AddListener(() => SelectAndPlaceTower(currentTowerData));
        }
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit,100,targetLayer))
            {
               

                // Lấy component BuildableTile trên vật thể bị click
                BuildableTile tile = hit.collider.GetComponent<BuildableTile>();

                if (tile != null)
                {
                    // Nếu ô đất ĐÃ CÓ trụ, hiện UI phá hủy
                    if (tile.towerOnTile != null)
                    {
                        selectedTileForDemolish = tile; // Lưu tham chiếu ô đất
                        ShowDemolishPanel(true);
                        ShowTowerSelectionPanel(false); // Đảm bảo panel xây bị tắt
                    }
                    // Nếu ô đất CHƯA CÓ trụ (có thể xây), hiện UI xây dựng
                    else
                    {
                        selectedBuildableTile = hit.transform; // Lưu transform của ô đất
                        ShowTowerSelectionPanel(true);
                        ShowDemolishPanel(false); // Đảm bảo panel bán bị tắt
                    }
                }
                else
                {
                    // Click vào vật thể khác không phải ô đất có thể xây
                    ShowTowerSelectionPanel(false);
                    ShowDemolishPanel(false);
                }
            }
            else
            {
                // Nếu click ra ngoài, tắt cả 2 panel
                ShowTowerSelectionPanel(false);
                ShowDemolishPanel(false);
            }

        }
    }

    // Hàm hiển thị/ẩn bảng chọn.
    public void ShowTowerSelectionPanel(bool show, Vector3 worldPosition = default)
    {
        towerSelectionPanel.SetActive(show);
    }

    // Hàm này được gọi khi một nút chọn trụ được nhấn.
    void SelectAndPlaceTower(TowerData towerToBuild)
    {
        if (selectedBuildableTile != null)
        {
            // Tạo trụ
            GameObject newTower = ObjectPoolManager.SpawnObject(towerToBuild.towerPrefab, selectedBuildableTile.position, Quaternion.identity,ObjectPoolManager.PoolType.Tower);

            // Đánh dấu ô đất là đã bị chiếm
            selectedBuildableTile.tag = "Tower";
            // Lấy script của ô đất và lưu tham chiếu đến trụ vừa xây
            selectedBuildableTile.GetComponent<BuildableTile>().towerOnTile = newTower;


            // Tắt collider để không hiện menu xây nữa
            // selectedBuildableTile.GetComponent<Collider>().enabled = false; // Dòng này không cần nữa, vì ta dùng tag "Occupied"

            ShowTowerSelectionPanel(false);
        }
    }

    // Hàm để hiện/ẩn panel bán trụ
    void ShowDemolishPanel(bool show)
    {
        if (show)
        {
            LevelController levelController = selectedTileForDemolish.towerOnTile.GetComponent<LevelController>();
            if (levelController != null)
            {
                upgradeButton.onClick.AddListener(levelController.LevelUp);
            }
        }
        else
        {
            upgradeButton.onClick.RemoveAllListeners();
        }
        demolishPanel.SetActive(show);
        upgradeButton.gameObject.SetActive(show);
    }
    

    // Hàm được gọi khi nút "SellButton" được nhấn
    void SellTower()
    {
        // Lấy ra trụ đang nằm trên ô đất đã chọn
        GameObject towerToSell = selectedTileForDemolish.towerOnTile;

        // Phá hủy GameObject của trụ
        ObjectPoolManager.ReturnObject(towerToSell);

        // Reset lại ô đất
        selectedTileForDemolish.tag = "Buildable"; // Đổi tag lại như cũ
        selectedTileForDemolish.towerOnTile = null; // Xóa tham chiếu

        // (Tùy chọn) Hoàn tiền cho người chơi
        // PlayerStats.Money += towerData.getSellValue();

        // Ẩn panel đi
        ShowDemolishPanel(false);
    }
    public void OnBuildableTileChanged(GameObject gameObject)
    {
        selectedTileForDemolish.towerOnTile = gameObject;
        upgradeButton.onClick.RemoveAllListeners();
        LevelController levelController = gameObject.GetComponent<LevelController>();
        if (levelController != null)
        {
            upgradeButton.onClick.AddListener(levelController.LevelUp);
        }
    }
}