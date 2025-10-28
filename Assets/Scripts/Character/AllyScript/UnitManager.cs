using UnityEngine;
using UnityEngine.UI; // Cần cho UI
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    public List<Unit> allUnits = new List<Unit>();
    public HashSet<Unit> selectedUnits = new HashSet<Unit>();

    [Header("Input Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask unitLayer;

    [Header("Selection Box")]
    [SerializeField] private Image selectionBoxImage; // UI Image cho hộp chọn
    private Vector2 startDragPosition;

    private Camera mainCamera;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        mainCamera = Camera.main;
        selectionBoxImage.gameObject.SetActive(false);
    }

    // Các hàm này được gọi bởi Unit.cs
    public void RegisterUnit(Unit unit)
    {
        if (!allUnits.Contains(unit)) allUnits.Add(unit);
    }
    public void UnregisterUnit(Unit unit)
    {
        if (allUnits.Contains(unit)) allUnits.Remove(unit);
        if (selectedUnits.Contains(unit)) selectedUnits.Remove(unit);
    }

    void Update()
    {
        HandleSelectionInput();
        HandleCommandInput();
    }

    void HandleSelectionInput()
    {
        // 1. Bắt đầu Click/Kéo (Chuột Trái)
        if (Input.GetMouseButtonDown(0))
        {
            startDragPosition = Input.mousePosition;
            selectionBoxImage.gameObject.SetActive(true);
            UpdateSelectionBox(Input.mousePosition);

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Nếu không giữ Ctrl, bỏ chọn tất cả trước
            if (!Input.GetKey(KeyCode.LeftControl))
            {
                DeselectAllUnits();
            }

            // Bắn Raycast để chọn 1 đơn vị
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, unitLayer))
            {
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null)
                {
                    ToggleSelectUnit(unit);
                }
            }
        }

        // 2. Đang Kéo chuột
        if (Input.GetMouseButton(0))
        {
            UpdateSelectionBox(Input.mousePosition);
        }

        // 3. Thả Chuột
        if (Input.GetMouseButtonUp(0))
        {
            selectionBoxImage.gameObject.SetActive(false);

            Rect selectionRect = GetSelectionRect(startDragPosition, Input.mousePosition);

            // Nếu không kéo (chỉ click) thì không cần chọn trong hộp
            if (Vector2.Distance(startDragPosition, Input.mousePosition) < 10f) return;

            // Chọn tất cả lính trong hộp
            foreach (Unit unit in allUnits)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(unit.transform.position);
                if (selectionRect.Contains(screenPos))
                {
                    ToggleSelectUnit(unit);
                }
            }
        }
    }

    void HandleCommandInput()
    {
        // 1. Ra lệnh (Chuột Phải)
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Ưu tiên 1: Tấn công địch
            if (Physics.Raycast(ray, out RaycastHit hitEnemy, 1000f, enemyLayer))
            {
                GameObject enemyTarget = hitEnemy.collider.gameObject;
                CommandAttack(enemyTarget);
            }
            // Ưu tiên 2: Di chuyển đến mặt đất
            else if (Physics.Raycast(ray, out RaycastHit hitGround, 1000f, groundLayer))
            {
                CommandMove(hitGround.point);
            }
        }
    }

    // --- Các hàm hỗ trợ ---

    private void DeselectAllUnits()
    {
        foreach (Unit unit in selectedUnits)
        {
            unit.Deselect();
        }
        selectedUnits.Clear();
    }

    private void ToggleSelectUnit(Unit unit)
    {
        if (selectedUnits.Contains(unit))
        {
            unit.Deselect();
            selectedUnits.Remove(unit);
        }
        else
        {
            unit.Select();
            selectedUnits.Add(unit);
        }
    }

    private void CommandMove(Vector3 destination)
    {
        foreach (Unit unit in selectedUnits)
        {
            unit.ReceiveMoveCommand(destination);
        }
    }

    private void CommandAttack(GameObject target)
    {
        foreach (Unit unit in selectedUnits)
        {
            unit.ReceiveAttackCommand(target);
        }
    }

    // --- Logic vẽ Hộp chọn ---

    private void UpdateSelectionBox(Vector2 currentMousePos)
    {
        RectTransform rect = selectionBoxImage.rectTransform;
        float width = currentMousePos.x - startDragPosition.x;
        float height = currentMousePos.y - startDragPosition.y;

        rect.anchoredPosition = startDragPosition + new Vector2(width / 2, height / 2);
        rect.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
    }

    private Rect GetSelectionRect(Vector2 start, Vector2 end)
    {
        float xMin = Mathf.Min(start.x, end.x);
        float yMin = Mathf.Min(start.y, end.y);
        float xMax = Mathf.Max(start.x, end.x);
        float yMax = Mathf.Max(start.y, end.y);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}