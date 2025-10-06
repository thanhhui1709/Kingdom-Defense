using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; 
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    [Header("Tower Data")]
    // Danh sách chứa tất cả các loại trụ có thể xây.
    // Chúng ta sẽ kéo các file TowerData vào đây trong Inspector.
    public List<TowerData> availableTowers = new List<TowerData>();

    [Header("UI References")]
    // Panel chính để chứa các nút.
    public GameObject towerSelectionPanel;
    // Prefab của nút bấm 
    public GameObject towerButtonPrefab;
    // Đối tượng cha để chứa các nút (chính là Panel có Layout Group).
    public Transform buttonContainer;

    // Biến tạm để lưu ô đất đã chọn.
    private Transform selectedBuildableTile;

    [Header("Demolish UI References")]
    public GameObject demolishPanel;
    public Button sellButton;
    private BuildableTile selectedTileForDemolish; // Lưu lại ô đất được chọn để phá

    void Start()
    {
        towerSelectionPanel.SetActive(false);
        demolishPanel.SetActive(false); // Ẩn panel bán trụ

        GenerateTowerButtons();

        // Gán sự kiện cho nút bán trụ
        sellButton.onClick.AddListener(SellTower);
    }

    // Hàm này sẽ tự động tạo các nút chọn trụ.
    void GenerateTowerButtons()
    {
        // Xóa các nút cũ nếu có
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Tạo nút mới cho mỗi loại trụ trong danh sách
        foreach (TowerData towerData in availableTowers)
        {
            // Tạo một bản sao của prefab nút
            GameObject buttonGO = Instantiate(towerButtonPrefab, buttonContainer);

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

            if (Physics.Raycast(ray, out hit))
            {
                // Click vào ô có thể xây
                if (hit.collider.CompareTag("Buildable"))
                {
                    selectedBuildableTile = hit.transform;
                    ShowTowerSelectionPanel(true);
                    ShowDemolishPanel(false); // Đảm bảo panel bán bị tắt
                }
                // Click vào ô đã có trụ 
                else if (hit.collider.CompareTag("Occupied"))
                {
                    // Lấy script BuildableTile từ ô đất được click
                    selectedTileForDemolish = hit.collider.GetComponent<BuildableTile>();
                    ShowDemolishPanel(true);
                    ShowTowerSelectionPanel(false); // Đảm bảo panel xây bị tắt
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
            GameObject newTower = Instantiate(towerToBuild.towerPrefab, selectedBuildableTile.position, Quaternion.identity);
          
            // Đánh dấu ô đất là đã bị chiếm
            selectedBuildableTile.tag = "Occupied";
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
        demolishPanel.SetActive(show);
    }

    // Hàm được gọi khi nút "SellButton" được nhấn
    void SellTower()
    {
        // Lấy ra trụ đang nằm trên ô đất đã chọn
        GameObject towerToSell = selectedTileForDemolish.towerOnTile;

        // Phá hủy GameObject của trụ
        Destroy(towerToSell);

        // Reset lại ô đất
        selectedTileForDemolish.tag = "Buildable"; // Đổi tag lại như cũ
        selectedTileForDemolish.towerOnTile = null; // Xóa tham chiếu

        // (Tùy chọn) Hoàn tiền cho người chơi
        // PlayerStats.Money += towerData.getSellValue();

        // Ẩn panel đi
        ShowDemolishPanel(false);
    }
}