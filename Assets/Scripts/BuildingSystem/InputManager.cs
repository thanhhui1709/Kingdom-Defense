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

    public event Action<GameObject> OnObjectSelected;

    public event Action OnClicked, OnCancel;

    public Vector3 SelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;

        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, buildModeLayer))
        {
            lastPosition = hit.point;
        }
        return lastPosition;

    }
    public void OnSelectedGameObject()
    {
        GameObject selectedObject = null;
        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, normalLayer))
        {
            selectedObject = hit.collider.gameObject;
            Debug.Log($"Selected Object: {selectedObject.name}");
            OnObjectSelected?.Invoke(selectedObject);
        }
       
    }
  
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnClicked?.Invoke();
            OnSelectedGameObject();
            Debug.Log("Clicked");
        }
        if (Input.GetMouseButtonDown(1))
        {
            OnCancel?.Invoke();
            Debug.Log("Canceled");
        }
    }

    public bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
