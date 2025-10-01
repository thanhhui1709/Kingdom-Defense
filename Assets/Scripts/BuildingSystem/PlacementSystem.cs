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
    private GameObject indicator;
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private ObjectDataBaseSO database;

    [SerializeField]
    private Tile tile;

    [SerializeField]
    //panel show towers
    private GameObject UI;

    //[SerializeField] private Button deleteButton;

    //private GridData gridData;

    private PlacementPersistence persistence = new PlacementPersistence();

    private GameObject parentObject;

    private GameObject selectedGameObject;



    private async void Awake()
    {
        //persistence.DeleteAllData();

        inputManager.OnObjectSelected += HandleObjectSelection;
        selectedGameObject = null;
        parentObject = new() { name = "Decoration" };
        //gridData = new GridData();

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
    private void HandleObjectSelection(GameObject obj)
    {
        selectedGameObject = obj;

        if (selectedGameObject.CompareTag("Buildable"))
        {
            indicator.gameObject.SetActive(true);
            indicator.transform.position = selectedGameObject.transform.position + offsetPos;

            Debug.Log("Now PlacementSystem knows: " + selectedGameObject.name);


            UI.gameObject.SetActive(true);

      
            inputManager.OnCancel += StopPlacement;
            StartPlacement();
        }
    }
    #region PlaceObject
    private void StopPlacement()
    {
        UI.gameObject.SetActive(false);
        indicator.gameObject.SetActive(false);

        inputManager.OnCancel -= StopPlacement;
    }

    public void StartPlacement()
    {
        if(inputManager.IsPointerOverUI()) { return; }
        if (selectedGameObject!=null&&!selectedGameObject.tag.Equals("Buildable")) return;

        UI.gameObject.SetActive(true);
        inputManager.OnCancel += StopPlacement;
        inputManager.OnClicked -= StartPlacement;




    }
    public async Task DeleteObject()
    {
        if (selectedGameObject == null) { return; }
        Vector3 mousePos = inputManager.SelectedMapPosition();
        Vector3Int gridPos = grid.WorldToCell(mousePos);

        string cleanName = selectedGameObject.name.Substring(0, selectedGameObject.name.Length - 7);
        ObjectData objectData = database.objectData.Find(x => x.prefab.name.Equals(cleanName));

        //List<Vector3Int> occupiedCells = gridData.CalculateOccupyCells(gridPos, objectData.size);
        //await persistence.DeletePlacedObject(objectData.id, occupiedCells);
        Destroy(selectedGameObject);
        selectedGameObject = null;
        //deleteButton.gameObject.SetActive(false);

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
        return tile.IsCellFree(gridPos);
            //&& gridData.CanPlacePosition(gridPos, database.objectData[selectedObjectIndex].size, database.objectData[selectedObjectIndex].prefab);
    }

    public void PlaceObject(int ID)
    {
        Vector3Int pos = grid.WorldToCell(selectedGameObject.transform.position);
        pos.y = 0;
        bool isValidPos = CheckValidPosition(pos, ID);
        if (!isValidPos) return;

        // Place the object and add to grid data
        GameObject newObject = Instantiate(database.objectData[ID].prefab);
        Destroy(selectedGameObject);
        selectedGameObject = null;
        newObject.transform.position = grid.GetCellCenterWorld(pos) + offsetPos;


        //gridData.AddObject(pos, database.objectData[ID].size, newObject);
        StopPlacement();
        //List<Vector3Int> occupiedCells = gridData.CalculateOccupyCells(gridPos, database.objectData[selectedIndex].size);
        //await persistence.SavePlacedObject(database.objectData[selectedIndex].id, occupiedCells, newObject.transform.position);
    }
    #endregion





}
