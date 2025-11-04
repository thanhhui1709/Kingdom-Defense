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
    [Header("Panel References")]
    [SerializeField] private GameObject buyUnitPanel;
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

    void Start()
    {
        // Ẩn tất cả các panel khi bắt đầu
        ToggleBuyTowerPanel(false);
        ToggleBuyUnitPanel(false); // Bạn có thể đổi thành true nếu muốn
        ToggleUpgradeTowerPanel(false);

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
        buyUnitPanel.SetActive(isActive);
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
    public void ShowUpgradePanel(BuildableTile tile)
    {
        GameObject towerOnTile = tile.towerOnTile;
        if (towerOnTile == null) return;

        // Lấy các component logic từ trụ
        LevelController levelController = towerOnTile.GetComponent<LevelController>();
        Stats towerStats = towerOnTile.GetComponent<Stats>();
        TowerHealth towerHealth = towerOnTile.GetComponent<TowerHealth>(); // LẤY HEALTH

        if (levelController == null || towerStats == null || towerHealth == null)
        {
            Debug.LogError("Trụ thiếu LevelController, Stats, hoặc TowerHealth!");
            return;
        }

        // --- 1. Xử lý ĐĂNG KÝ sự kiện Health ---

        // Hủy đăng ký trụ CŨ (nếu có)
        UnsubscribeFromTowerHealth();

        // Lưu và đăng ký trụ MỚI
        currentSelectedTowerHealth = towerHealth;
        currentSelectedTowerHealth.OnHealthChanged += UpdatePanelHealthBar;

        // Cập nhật thanh máu lần đầu tiên ngay khi mở
        UpdatePanelHealthBar(towerHealth.CurrentHealth, towerHealth.MaxHealth);

        // --- 2. Điền dữ liệu UI ---
        // (Giả sử Stats có biến towerName và sprite)
        upgradeTowerUI.towerName.text = towerOnTile.name.Substring(0,towerOnTile.name.Length-7);
        upgradeTowerUI.avatar.sprite = towerStats.sprite;

        // (Giả sử Stats có hàm GetSellValue())
        int sellCost = (int)(towerStats.Money*0.7);
        upgradeTowerUI.sellCostText.text = sellCost.ToString();

        // --- 3. Kiểm tra trạng thái nâng cấp ---
        if (levelController.IsReadyToEvolve() && levelController.IsAtMaxEvolution())
        {
            // Đã max cả sub-level VÀ max cả tiến hóa
            upgradeTowerUI.upgradeCostText.text = "MAX";
            upgradeTowerUI.upgradeButton.interactable = false;
        }
        else
        {
            // Vẫn còn nâng cấp được (sub-level hoặc tiến hóa)
            upgradeTowerUI.upgradeCostText.text = levelController.GetNextLevelCost().ToString();
            upgradeTowerUI.upgradeButton.interactable = true;
        }

        // --- 4. Thiết lập sự kiện (phải có buildManager) ---
        if (buildManager == null)
        {
            Debug.LogError("BuildManager chưa được gán cho UIManager! Hãy đảm bảo PopulateBuyTowerMenu được gọi trong Start.");
            return;
        }

        upgradeTowerUI.upgradeButton.onClick.RemoveAllListeners();
        upgradeTowerUI.upgradeButton.onClick.AddListener(buildManager.UpgradeSelectedTower);

        upgradeTowerUI.sellButton.onClick.RemoveAllListeners();
        upgradeTowerUI.sellButton.onClick.AddListener(buildManager.SellSelectedTower);

        // --- 5. Hiển thị panel ---
        ToggleUpgradeTowerPanel(true);
    }
}