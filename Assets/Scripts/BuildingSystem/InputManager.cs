using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    private Vector3 lastPosition;

    [SerializeField]
    private LayerMask layerMask;

    public Vector3 selectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
    
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, layerMask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;

    }
    // Update is called once per frame
    void Update()
    {

    }
}
