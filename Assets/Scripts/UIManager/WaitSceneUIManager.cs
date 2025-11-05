using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static SpawnEnemyWave;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;

public class WaitSceneUIManager : MonoBehaviour
{
    [Header("UI References")]
    public List<Sprite> costSprites = new List<Sprite>();
    public GameObject blackSmith;
    public GameObject alchemist;
    public GameObject shopTowerPanel;
    public GameObject shopUnitPanel;

    [Header("Money")]
    public TextMeshProUGUI moneyText;

    [Header("Upgrade Panels")]
    public GameObject buyTowerContainer;
    public GameObject buyUnitContainer;

    [Header("Prefabs")]
    public GameObject upgradeItemPrefab;

    [Header("UI Sound")]
    public AudioClip clickSound;
    public AudioClip buySound;
    public AudioClip notEnoughMoneySound;

    [Header("Stage Select")]
    public Sprite circleMaskSprite;
    public List<StageData> stages;
    public Transform stageContainer;
    public Button nextBtn;
    public Button prevBtn;
    public Button playBtn;

    private List<List<LevelUpData>> datas;
    private int currentStageIndex = 0;
    private GameObject currentStageObj;

    private void Awake()
    {
        datas = LevelUpManager.Instance.GetLevelUpDatas();
        RefreshPanel(LevelUpType.Tower, buyTowerContainer);
        RefreshPanel(LevelUpType.Unit, buyUnitContainer);
        moneyText.text = StartMoney.Instance.GetBalance().ToString();
        CloseAllPanel();
    }

    private void Start()
    {
        LoadStage(currentStageIndex);
        UpdateSlideButtons();
    }

    private void Update()
    {
        // Nếu click lên UI → bỏ qua
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // Nếu panel đang bật → không cho tương tác công trình
        if (shopTowerPanel.activeSelf || shopUnitPanel.activeSelf)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (hitInfo.collider.gameObject == alchemist)
                {
                    TogglePanel(shopTowerPanel);
                }
                else if (hitInfo.collider.gameObject == blackSmith)
                {
                    TogglePanel(shopUnitPanel);
                }
            }
        }
    }

    // ---------------- UPGRADE PANEL ---------------- //

    private void RefreshPanel(LevelUpType type, GameObject container)
    {
        foreach (Transform child in container.transform)
            Destroy(child.gameObject);

        var listGroups = datas.Where(g => g.Any(x => x.type == type)).ToList();

        foreach (var group in listGroups)
        {
            LevelUpData unlockedLevel = group.Where(x => x.isUnlocked)
                                             .OrderByDescending(x => x.level)
                                             .FirstOrDefault();

            LevelUpData shownLevel;
            Sprite lvSprite;

            if (unlockedLevel != null)
            {
                shownLevel = unlockedLevel;
                lvSprite = costSprites[Mathf.Clamp(shownLevel.level, 0, costSprites.Count - 1)];
            }
            else
            {
                shownLevel = group.First();
                lvSprite = costSprites[0];
            }

            SpawnItemUI(shownLevel, lvSprite, container);
        }
    }

    private void SpawnItemUI(LevelUpData data, Sprite levelBarSprite, GameObject container)
    {
        GameObject item = Instantiate(upgradeItemPrefab, container.transform);
        item.transform.Find("Avarta").GetComponent<Image>().sprite = data.avartar;
        item.transform.Find("LevelImg").GetComponent<Image>().sprite = levelBarSprite;

        Transform costPanel = item.transform.Find("CostPanel");
        Image costIcon = costPanel.Find("CostImg").GetComponent<Image>();

        for (int i = 1; i < data.cost; i++)
            Instantiate(costIcon, costPanel);

        Button buyBtn = item.transform.Find("BuyBtn").GetComponent<Button>();
        buyBtn.onClick.AddListener(() => OnBuyClicked(data, container));

        AddHoverEffect(buyBtn.transform);

        StartCoroutine(AutoCheckButtonState(data, buyBtn));
    }

    private void OnBuyClicked(LevelUpData data, GameObject container)
    {
        if (!TrySpendMoney(data.cost))
        {
            ObjectPoolManager.PlayAudio2D(notEnoughMoneySound, 1f);
            return;
        }

        ObjectPoolManager.PlayAudio2D(buySound, 1f);
        UnlockNextLevel(data);
        RefreshPanel(data.type, container);
    }

    private void UnlockNextLevel(LevelUpData data)
    {
        var group = datas.First(g => g.Contains(data));
        var next = group.FirstOrDefault(x => x.level == data.level + 1);
        if (next != null)
            next.isUnlocked = true;
    }

    private bool TrySpendMoney(int cost)
    {
        if (!StartMoney.Instance.CheckBalance(cost)) return false;

        int oldMoney = StartMoney.Instance.GetBalance();
        StartMoney.Instance.RemoveMoney(cost);
        int newMoney = StartMoney.Instance.GetBalance();

        DOTween.To(() => oldMoney, x => moneyText.text = x.ToString(), newMoney, 0.4f)
               .SetEase(Ease.OutQuad);

        return true;
    }

    private IEnumerator AutoCheckButtonState(LevelUpData data, Button buyBtn)
    {
        while (buyBtn != null)
        {
            var group = datas.First(g => g.Contains(data));
            bool hasNext = group.Any(x => x.level == data.level + 1);
            bool enoughMoney = StartMoney.Instance.CheckBalance(data.cost);

            buyBtn.interactable = hasNext && enoughMoney;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void AddHoverEffect(Transform btn)
    {
        EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();
        AddTrigger(trigger, EventTriggerType.PointerEnter, () => btn.DOScale(1.1f, 0.15f));
        AddTrigger(trigger, EventTriggerType.PointerExit, () => btn.DOScale(1f, 0.15f));
    }

    private void AddTrigger(EventTrigger trigger, EventTriggerType type, TweenCallback action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    public void TogglePanel(GameObject panel)
    {
        ObjectPoolManager.PlayAudio2D(clickSound, 1f);
        panel.SetActive(!panel.activeSelf);
    }

    public void CloseAllPanel()
    {
        shopUnitPanel.SetActive(false);
        shopTowerPanel.SetActive(false);
    }
    #region Stage Logic

    private void LoadStage(int index)
    {
        if (currentStageObj != null)
            Destroy(currentStageObj);

        currentStageObj = Instantiate(stages[index].stagePrefab, stageContainer);

        Transform towerPanel = currentStageObj.transform.Find("MyTower");
        Transform unitPanel = currentStageObj.transform.Find("MyUnit");

        RefreshArmyPanels(towerPanel, unitPanel);
    }

    public void OnNextStage()
    {
        if (currentStageIndex < stages.Count - 1)
        {
            currentStageIndex++;
            LoadStage(currentStageIndex);
            ObjectPoolManager.PlayAudio2D(clickSound, 1f);
          
            UpdateSlideButtons();
        }
    }

    public void OnPrevStage()
    {
        if (currentStageIndex > 0)
        {
            currentStageIndex--;
            LoadStage(currentStageIndex);
            ObjectPoolManager.PlayAudio2D(clickSound, 1f);
       
            UpdateSlideButtons();
        }
    }

    private void UpdateSlideButtons()
    {
        prevBtn.interactable = currentStageIndex > 0;
        nextBtn.interactable = currentStageIndex < stages.Count - 1;
        playBtn.onClick.RemoveAllListeners();
        playBtn.onClick.AddListener(OnPlayStage);
        IsAbleToPlayStage();
    }
    private void IsAbleToPlayStage()
    {
        if (GameManager.Instance.CheckValidScene(stages[currentStageIndex].sceneName))
        {
            playBtn.interactable = true;
        }
        else
        {
            playBtn.interactable = false;
        }
    }
    public void OnPlayStage()
    {
        ObjectPoolManager.PlayAudio2D(clickSound, 1f);
        SceneManager.LoadScene(stages[currentStageIndex].sceneName);
    }

    #endregion

    #region Army UI Refresh

    private void RefreshArmyPanels(Transform towerPanel, Transform unitPanel)
    {
        foreach (Transform t in towerPanel) Destroy(t.gameObject);
        foreach (Transform u in unitPanel) Destroy(u.gameObject);

        foreach (var group in datas)
        {
            LevelUpData best = group
                .Where(x => x.isUnlocked)
                .OrderByDescending(x => x.level)
                .FirstOrDefault();

            if (best == null) continue;

            Transform parent = best.type == LevelUpType.Tower ? towerPanel : unitPanel;

            // --- Tạo object Icon cha ---
            GameObject iconGO = new GameObject("Icon", typeof(RectTransform));
            iconGO.transform.SetParent(parent, false);

            RectTransform rt = iconGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(95, 95);

            // --- Tạo Shape mask hình tròn ---
            GameObject maskGO = new GameObject("Mask", typeof(Image), typeof(Mask));
            maskGO.transform.SetParent(iconGO.transform, false);

            Image maskImg = maskGO.GetComponent<Image>();
            maskImg.sprite = circleMaskSprite;
            maskImg.preserveAspect = true;

            RectTransform mrt = maskGO.GetComponent<RectTransform>();
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.offsetMin = Vector2.zero;
            mrt.offsetMax = Vector2.zero;

            // --- Tạo Avatar con và gán ảnh lính / tower ---
            GameObject avatarGO = new GameObject("Avatar", typeof(Image));
            avatarGO.transform.SetParent(maskGO.transform, false);

            Image avatarImg = avatarGO.GetComponent<Image>();
            avatarImg.sprite = best.avartar;
            avatarImg.preserveAspect = true;

            RectTransform art = avatarGO.GetComponent<RectTransform>();
            art.anchorMin = Vector2.zero;
            art.anchorMax = Vector2.one;
            art.offsetMin = Vector2.zero;
            art.offsetMax = Vector2.zero;
        }
    }

    #endregion
}
[System.Serializable]
public class StageData
{
    public string sceneName;       // Tên scene sẽ load khi Play
    public GameObject stagePrefab; // Prefab UI màn chơi trong select panel
}
