using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DrawPathSystem : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private GameObject gridVisualization;
    [SerializeField]
    private GameObject indicator;
    [SerializeField]
    private Tile tile;

    [SerializeField]
    private GameObject tileObject;
    [SerializeField]
    private PreviewSystem previewSystem;

    private Dictionary<Vector3Int, GameObject> selectedObjects = new();

    void Start()
    {
        inputManager.OnDrawing += GetDrawnObjects;
        inputManager.OnReleasedMouse += DrawNewPath;
    }

    // Update is called once per frame
    void LateUpdate()
    {

    }
    private void GetDrawnObjects(GameObject go)
    {
        Vector3Int pos = grid.WorldToCell(go.transform.position);
        pos.y = 0;
        if (selectedObjects.ContainsKey(pos)) return;
        selectedObjects.Add(pos, go);
        previewSystem.ShowingPreview(selectedObjects.Values.ToList());
        AdjustColor();
        Debug.Log("Added object at " + pos);
    }
    public void EnterDrawMode()
    {
        inputManager.isOnDrawMode = true;
        indicator.SetActive(true);
        gridVisualization.SetActive(true);
        selectedObjects.Clear();
    }
    public void OnStopDragging()
    {
        inputManager.isOnDrawMode = false;
        indicator.SetActive(false);
        gridVisualization.SetActive(false);
        Debug.Log($"Total drawn objects: {selectedObjects.Count}");
        foreach (var obj in selectedObjects)
        {
            Debug.Log($"Object at {obj.Key}: {obj.Value.name}");
        }
        selectedObjects.Clear();
    }
    private void AdjustColor()
    {
        previewSystem.DisplayValidColorForMultipleObjects(tile.CheckValidDrawRoad(selectedObjects));
    }
    public void DrawNewPath()
    {
     
    
        previewSystem.DestroyAllPreview();

        if (selectedObjects.Count <2) return;
        if (!tile.CheckValidDrawRoad(selectedObjects))
        {
            selectedObjects.Clear();
            return;
        }

        // Tìm giao nhau giữa path cũ và mới
        var overlap = selectedObjects.Keys.Intersect(tile.tiles.Keys).ToList();

        // Xóa path cũ trong vùng vẽ
        foreach (var pos in selectedObjects.Keys)
        {
            if (tile.tiles.ContainsKey(pos))
            {
                Destroy(tile.tiles[pos]);
                tile.tiles.Remove(pos);
            }
        }

        // Tạo path mới
        foreach (var obj in selectedObjects)
        {
            Vector3Int cellPos = obj.Key;
            Vector3 worldPos = grid.GetCellCenterWorld(cellPos);
            worldPos.y = 0;

            GameObject newTile = Instantiate(tileObject, worldPos, Quaternion.identity);
            tile.tiles[cellPos] = newTile;

            obj.Value.SetActive(false); // ẩn terrain
        }

        // Merge và reload
        tile.MergePath(selectedObjects);

        // Kiểm tra lại có duy nhất 1 đường đi từ start đến end
        if (!tile.HasUniquePath())
        {
            Debug.LogError("Đường vẽ không hợp lệ! Không tồn tại duy nhất 1 đường đi.");
            // TODO: rollback hoặc hiển thị cảnh báo
        }

        selectedObjects.Clear();
    }

}

