using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class PlacementSystem : MonoBehaviour
{

    [SerializeField]
    private Vector3 offsetPos;
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private ObjectDataBaseSO database;

    [SerializeField]
    private Tile tile;

    [SerializeField]
    private GameObject gridVisualizer;
    private int selectedIndex = -1;

    [SerializeField] private Button deleteButton;

    private GridData gridData;


    private PlacementPersistence persistence = new PlacementPersistence();

    private GameObject parentObject;

    private GameObject selectedGameObject;

    [SerializeField]
    private PreviewSystem previewSystem;
    private Vector3Int lastPreviewPos;
    private async void Awake()
    {
        //persistence.DeleteAllData();
        lastPreviewPos = Vector3Int.zero;
        inputManager.OnObjectSelected += HandleObjectSelection;
        selectedGameObject = null;
        parentObject = new() { name = "Decoration" };
        gridData = new GridData();

        StopPlacement();
        await LoadMapAsync();

    }
    private async Task LoadMapAsync()
    {
        await persistence.LoadPlacedObjects(database, (idx, occupiedPos, pos) =>
        {
            GameObject go = Instantiate(database.objectData[idx].prefab);
            go.transform.SetParent(parentObject.transform);
            go.transform.position = pos;
        });
        tile.LoadTile();
    }

    private void Update()
    {
        if (selectedIndex < 0) return;
        Vector3 mousePos = inputManager.SelectedMapPosition();
        Vector3Int gridPos = grid.WorldToCell(mousePos);
        

        if (lastPreviewPos != gridPos)
        {
            gridPos.y = 0;
            bool isValidPos = CheckValidPosition((Vector3Int)gridPos, selectedIndex);
            Debug.Log($"Preview at {gridPos} is valid: {isValidPos}");
            previewSystem.UpdatePosition(grid.GetCellCenterWorld(gridPos), isValidPos);
        }
    }

    private void HandleObjectSelection(GameObject obj)
    {
        selectedGameObject = obj;
        Debug.Log("Now PlacementSystem knows: " + selectedGameObject.name);
        deleteButton.gameObject.SetActive(true);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(obj.transform.position);
        deleteButton.transform.position = screenPos + new Vector3(50, -50, 0);
    }

    private void StopPlacement()
    {
        selectedIndex = -1;
        gridVisualizer.SetActive(false);
        previewSystem.DestroyPreview();
        inputManager.OnClicked -=  PlaceObject;
        inputManager.OnCancel -= StopPlacement;

        lastPreviewPos = Vector3Int.zero;
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        selectedIndex = database.objectData.FindIndex(x => x.id == ID);
        if (selectedIndex < 0) { Debug.LogError("ID not found in database"); return; }
        gridVisualizer.SetActive(true);
        previewSystem.ShowingPreview(database.objectData[selectedIndex].prefab, database.objectData[selectedIndex].size);
        inputManager.OnClicked +=  PlaceObject;
        inputManager.OnCancel += StopPlacement;
    }
    public async Task DeleteObject()
    {
        if (selectedGameObject == null) { return; }
        Vector3 mousePos = inputManager.SelectedMapPosition();
        Vector3Int gridPos = grid.WorldToCell(mousePos);

        string cleanName = selectedGameObject.name.Substring(0, selectedGameObject.name.Length - 7);
        ObjectData objectData = database.objectData.Find(x => x.prefab.name.Equals(cleanName));

        List<Vector3Int> occupiedCells = gridData.CalculateOccupyCells(gridPos, objectData.size);
        await persistence.DeletePlacedObject(objectData.id, occupiedCells);
        Destroy(selectedGameObject);
        selectedGameObject = null;
        deleteButton.gameObject.SetActive(false);

        //GameObject objToDelete = gridData.GetObjectAtPosition((Vector3Int)gridPos);
        //if (objToDelete != null)
        //{
        //    Destroy(objToDelete);
        //    gridData.RemoveObject((Vector3Int)gridPos, database.objectData[selectedIndex].size, objToDelete);
        //    deleteButton.gameObject.SetActive(false);
        //}
    }

    public bool CheckValidPosition(Vector3Int gridPos, int selectedObjectIndex)
    {
        return tile.IsCellFree(gridPos)&& gridData.CanPlacePosition(gridPos, database.objectData[selectedObjectIndex].size, database.objectData[selectedObjectIndex].prefab);
    }

    private void PlaceObject()
    {
        if (inputManager.IsPointerOverUI()) { return; }
        Vector3 mousePos = inputManager.SelectedMapPosition();
        Vector3Int gridPos = grid.WorldToCell(mousePos);
        gridPos.y = 0;

        bool isValidPos = CheckValidPosition((Vector3Int)gridPos, selectedIndex);
        if (!isValidPos) return;

        // Place the object and add to grid data
        GameObject newObject = Instantiate(database.objectData[selectedIndex].prefab);
        newObject.transform.position = grid.GetCellCenterWorld(gridPos) + offsetPos;
        
        lastPreviewPos= gridPos;
        gridData.AddObject(gridPos, database.objectData[selectedIndex].size, newObject);
        //List<Vector3Int> occupiedCells = gridData.CalculateOccupyCells(gridPos, database.objectData[selectedIndex].size);
        //await persistence.SavePlacedObject(database.objectData[selectedIndex].id, occupiedCells, newObject.transform.position);
    }
}
