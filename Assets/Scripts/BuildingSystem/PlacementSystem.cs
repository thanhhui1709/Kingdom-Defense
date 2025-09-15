using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject indicator;
    [SerializeField]
    private Vector3 offsetPos;
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private Grid grid;
    private void Update()
    {
        Vector3 mousePos = inputManager.selectedMapPosition();
       
        Vector3Int gridPos=grid.WorldToCell(mousePos);
       

        indicator.transform.position=grid.GetCellCenterWorld(gridPos)+offsetPos;
       
    }

}
