using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // Cần cho .ToList()

// 1. Định nghĩa class UI Nâng cấp
[System.Serializable]
public class UpgradeTowerUI
{
    public GameObject UpgradePanel; // Panel cha
    public TMP_Text towerName;
    public TMP_Text upgradeCostText;
    public TMP_Text sellCostText;
    public Button upgradeButton;
    public Button sellButton;
    public Image avatar;

    // Thêm các tham chiếu thanh máu cho panel
    public Slider healthBar;
    public Image healthFill;
}


public class InGameUIManager : MonoBehaviour
{
    // Lớp nội bộ để lưu trữ nút và giá tiền của nó
    private class UnitButtonInfo
    {
        public Button button;
        public int cost;
    }
    [Header("Buy Unit Settings")]
    [Tooltip("Tham chiếu đến script SpawnUnit trong Scene")]
    [SerializeField] private SpawnUnit spawnUnit;
    [SerializeField] private GameObject unitButtonPrefab; // Prefab của nút mua lính
    [SerializeField] private Transform buyUnitButtonContainer; // Layout Group chứa các nút

    [Header("Panel References")]
    [SerializeField] private UpgradeTowerUI upgradeTowerUI;

    [Header("Buy Tower Settings")]
    [SerializeField] private GameObject towerButtonPrefab; // Prefab của nút mua trụ
    [SerializeField] private Transform buyTowerButtonContainer; // Layout Group chứa các nút

    [Header("Health Bar Settings")]
    [SerializeField] private Gradient panelHealthGradient; // Gradient cho thanh máu trên panel

    // Tham chiếu đến các Manager
    private BuildManager buildManager;

    // Biến lưu trụ đang được chọn để lắng nghe sự kiện
    private TowerHealth currentSelectedTowerHealth;
    private List<UnitButtonInfo> unitButtons = new List<UnitButtonInfo>();

    void Start()
    {
        // Ẩn tất cả các panel khi bắt đầu
        ToggleBuyTowerPanel(false);
        ToggleUpgradeTowerPanel(false);
        PopulateUnitSpawnMenu();

        // Lưu ý: BuildManager sẽ tự gán nó khi gọi PopulateBuyTowerMenu
    }

    /// <summary>
    /// Lưu tham chiếu BuildManager khi nó tự giới thiệu
    /// </summary>
    private void SetBuildManager(BuildManager builder)
    {
        if (buildManager == null)
            buildManager = builder;
    }

    // --- Các hàm Bật/Tắt Panel ---

    public void ToggleBuyTowerPanel(bool isActive)
    {
        buyTowerButtonContainer.gameObject.SetActive(isActive);
    }

    public void ToggleBuyUnitPanel(bool isActive)
    {
        buyUnitButtonContainer.gameObject.SetActive(isActive);
    }

    public void ToggleUpgradeTowerPanel(bool isActive)
    {
        if (upgradeTowerUI.UpgradePanel == null) return;

        if (!isActive)
        {
            // Nếu tắt panel, HỦY ĐĂNG KÝ sự kiện
            UnsubscribeFromTowerHealth();
        }
        upgradeTowerUI.UpgradePanel.SetActive(isActive);
    }

    // --- Logic UI Mua Trụ ---

    /// <summary>
    /// Tạo các nút mua trụ dựa trên danh sách trụ có sẵn.
    /// </summary>
    public void PopulateBuyTowerMenu(List<TowerData> availableTowers, BuildManager builder)
    {
        SetBuildManager(builder); // Lưu tham chiếu BuildManager

        // 1. Xóa các nút cũ
        foreach (Transform child in buyTowerButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Tạo các nút mới
        foreach (TowerData towerData in availableTowers)
        {
            if (!towerData.isUnlocked) continue; // Bỏ comment nếu bạn có logic này

            GameObject buttonGO = Instantiate(towerButtonPrefab, buyTowerButtonContainer);
            Button newButton = buttonGO.GetComponentInChildren<Button>(); // Giả sử nút nằm ở gốc

            //3.Điền dữ liệu cho nút(Giả sử prefab có các component này)
            Image icon = buttonGO.transform.GetComponentInChildren<Image>();
            TMP_Text costText = buttonGO.transform.Find("CostText").GetComponent<TMP_Text>();

            //(Giả sử TowerData có các biến này)
            icon.sprite = towerData.towerIcon;
          
            costText.text = towerData.buildCost.ToString();

            // 4. Gán sự kiện OnClick
            newButton.onClick.AddListener(() =>
            {
                builder.SelectAndPlaceTower(towerData);
            });
        }
    }

    // --- Logic UI Nâng Cấp/Bán ---

    /// <summary>
    /// Hàm hủy đăng ký an toàn khỏi sự kiện máu của trụ
    /// </summary>
    private void UnsubscribeFromTowerHealth()
    {
        if (currentSelectedTowerHealth != null)
        {
            currentSelectedTowerHealth.OnHealthChanged -= UpdatePanelHealthBar;
            currentSelectedTowerHealth = null;
        }
    }

    /// <summary>
    /// Hàm này được gọi bởi Event từ TowerHealth để cập nhật UI
    /// </summary>
    private void UpdatePanelHealthBar(float current, float max)
    {
        if (upgradeTowerUI.healthBar == null) return;

        float fillAmount = 0f;
        if (max > 0) // Tránh lỗi chia cho 0
        {
            fillAmount = current / max;
        }

        upgradeTowerUI.healthBar.value = fillAmount;

        if (upgradeTowerUI.healthFill != null && panelHealthGradient != null)
        {
            upgradeTowerUI.healthFill.color = panelHealthGradient.Evaluate(fillAmount);
        }
    }

    /// <summary>
    /// Hiển thị và điền dữ liệu cho panel Nâng Cấp/Bán.
    /// </summary>
    // Trong class InGameUIManager.cs

    public void ShowUpgradePanel(BuildableTile tile)
    {
        GameObject towerOnTile = tile.towerOnTile;
        if (towerOnTile == null) return;

        LevelController levelController = towerOnTile.GetComponent<LevelController>();
        Stats towerStats = towerOnTile.GetComponent<Stats>();
        TowerHealth towerHealth = towerOnTile.GetComponent<TowerHealth>();

        if (levelController == null || towerStats == null || towerHealth == null)
        {
            Debug.LogError("Trụ thiếu LevelController, Stats, hoặc TowerHealth!");
            return;
        }

        // (Logic đăng ký sự kiện thanh máu giữ nguyên...)
        UnsubscribeFromTowerHealth();
        currentSelectedTowerHealth = towerHealth;
        currentSelectedTowerHealth.OnHealthChanged += UpdatePanelHealthBar;
        UpdatePanelHealthBar(towerHealth.CurrentHealth, towerHealth.MaxHealth);

        // --- 1. ĐIỀN DỮ LIỆU (ĐÃ CẬP NHẬT) ---
        upgradeTowerUI.towerName.text = towerOnTile.name;
        upgradeTowerUI.avatar.sprite = towerStats.sprite;

        // Hiển thị 70% giá trị bán
        int sellCost = (int)(towerStats.TotalInvestedMoney * 0.7f);
        upgradeTowerUI.sellCostText.text = sellCost.ToString();

        // --- 2. KIỂM TRA TRẠNG THÁI NÂNG CẤP (ĐÃ CẬP NHẬT) ---
        int playerMoney = Currency.Instance.GetBalance();
        int nextLevelCost = levelController.GetNextLevelCost();

        if (levelController.IsReadyToEvolve() && levelController.IsAtMaxEvolution())
        {
            upgradeTowerUI.upgradeCostText.text = "MAX";
            upgradeTowerUI.upgradeButton.interactable = false;
        }
        else
        {
            upgradeTowerUI.upgradeCostText.text = nextLevelCost.ToString();

            // Vô hiệu hóa nút nếu không đủ tiền
            upgradeTowerUI.upgradeButton.interactable = (playerMoney >= nextLevelCost);
        }

        // --- 3. THIẾT LẬP SỰ KIỆN (Giữ nguyên) ---
        if (buildManager == null)
        {
            Debug.LogError("BuildManager chưa được gán cho UIManager!");
            return;
        }
        upgradeTowerUI.upgradeButton.onClick.RemoveAllListeners();
        upgradeTowerUI.upgradeButton.onClick.AddListener(buildManager.UpgradeSelectedTower);

        upgradeTowerUI.sellButton.onClick.RemoveAllListeners();
        upgradeTowerUI.sellButton.onClick.AddListener(buildManager.SellSelectedTower);

        // --- 4. HIỂN THỊ PANEL (Giữ nguyên) ---
        ToggleUpgradeTowerPanel(true);
    }
    public void PopulateUnitSpawnMenu()
    {
        if (spawnUnit == null)
        {
            Debug.LogError("Chưa gán SpawnUnit cho InGameUIManager!");
            return;
        }

        // 1. Lấy dữ liệu từ "Database"
        List<UnitData> units = UnitManager.Instance.GetAvailableUnits();

        // 2. Xóa các nút cũ
        foreach (Transform child in buyUnitButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 3. Tạo các nút mới
        foreach (UnitData unit in units)
        {
            // Chỉ tạo nút cho lính đã được mở khóa
            if (!unit.isUnlocked)
            {
                continue;
            }

            GameObject buttonGO = Instantiate(unitButtonPrefab, buyUnitButtonContainer);
            Button newButton = buttonGO.GetComponentInChildren<Button>();

          

            // 4. Điền dữ liệu vào nút
            // (Giả sử prefab của bạn có các component này)

            Image iconImg = buttonGO.transform.Find("KnightBtn/Icon").GetComponentInChildren<Image>();
            TMP_Text costText = buttonGO.transform.Find("Price/CostText").GetComponentInChildren<TMP_Text>();

            iconImg.sprite = unit.icon;
            costText.text = unit.cost.ToString();

            // tao unit button infor
            UnitButtonInfo unitButtonInfo = new()
            {
                button = newButton,
                cost =unit.cost


            };
            unitButtons.Add(unitButtonInfo);


            // 5. Gán sự kiện OnClick
            newButton.onClick.AddListener(() =>
            {
                // Khi nhấn nút, gọi hàm logic trong SpawnUnit
                spawnUnit.AttemptToSpawnUnit(unit);
            });
            // --- THÊM MỚI ---
            // Chạy kiểm tra 1 lần ngay lập tức
            UpdateAllButtonStates();

            // Bắt đầu 1 bộ đếm lặp, gọi hàm "UpdateAllButtonStates"
            // lặp lại mỗi 0.25 giây.
            InvokeRepeating(nameof(UpdateAllButtonStates), 0.25f, 0.25f);
            // --- KẾT THÚC THÊM MỚI ---
        }
    }
    private void UpdateAllButtonStates()
    {
        // 1. Lấy số tiền hiện tại
        int currentMoney = Currency.Instance.GetBalance();

        // 2. Cập nhật các nút mua lính
        foreach (UnitButtonInfo info in unitButtons)
        {
            if (info.button != null) // Kiểm tra an toàn
            {
                // Nút chỉ có thể nhấn nếu tiền >= giá
                info.button.interactable = (currentMoney >= info.cost);
            }
        }

        // 3. (Tương lai) Cập nhật các nút mua trụ
        // foreach (UnitButtonInfo info in towerButtons) { ... }
    }
}