using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    private Vector3 lastPosition;

    [SerializeField]
    private LayerMask buildModeLayer;
    [SerializeField]
    private LayerMask normalLayer;

    public bool isOnDrawMode;
    public event Action<GameObject> OnObjectSelected;
    public event Action<GameObject> OnDrawing;
    public event Action OnReleasedMouse;

    public event Action OnClicked, OnCancel;

    public Vector3 SelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;

        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, normalLayer))
        {
            lastPosition = hit.point;
        }
        return lastPosition;

    }
    public void OnSelectedGameObject()
    {
        if(IsPointerOverUI()) return;
        GameObject selectedObject = null;
        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, normalLayer))
        {
            selectedObject = hit.collider.gameObject;

            if (selectedObject.CompareTag("Buildable"))
            {
                Debug.Log($"Selected Buildable: {selectedObject.name}");
                OnObjectSelected?.Invoke(selectedObject);
            }
            else
            {
                Debug.Log($"Clicked on non-buildable: {selectedObject.name}");
            }
        }
    }
    private void CollectObjectsWhileDragging()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, normalLayer))
        {
            GameObject obj = hit.collider.gameObject;
            OnDrawing?.Invoke(obj);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isOnDrawMode)
        {
            if (Input.GetMouseButton(0))
            {
                CollectObjectsWhileDragging();
            }
            if(Input.GetMouseButtonUp(0))
            {
                OnReleasedMouse?.Invoke();
                Debug.Log("Finished Drawing");
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnSelectedGameObject();
                OnClicked?.Invoke();
                Debug.Log("Clicked");
            }
            if (Input.GetMouseButtonDown(1))
            {
                OnCancel?.Invoke();
                Debug.Log("Canceled");
            }
        }
       

       
      
    }

    public bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
