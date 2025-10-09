using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameSetting gameSetting;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        cam.orthographicSize = gameSetting.CameraFov; // set default FOV
    }

    private void Update()
    {
        HandleZoom();
        HandleMovement();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            
            cam.orthographicSize -= scroll * gameSetting.ZoomSensitivity;


            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize,
                                          gameSetting.MinCameraFov,
                                          gameSetting.MaxCameraFov);
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDir = Vector3.zero;

        Vector3 mousePos = Input.mousePosition;
        float edgeSize = gameSetting.ScreenEdgeSize;

        // check mouse pos close to screen edge
        if (mousePos.x <= edgeSize) moveDir.x = -1;             // left
        else if (mousePos.x >= Screen.width - edgeSize) moveDir.x = 1; // right

        if (mousePos.y <= edgeSize) moveDir.z = -1;             // down
        else if (mousePos.y >= Screen.height - edgeSize) moveDir.z = 1; // up

        transform.position += moveDir.normalized * gameSetting.CameraMoveSpeed * Time.deltaTime;

        // move within limit area
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, gameSetting.XMin, gameSetting.XMax);
        pos.z = Mathf.Clamp(pos.z, gameSetting.ZMin, gameSetting.ZMax);
       
        transform.position = pos;
    }
}
