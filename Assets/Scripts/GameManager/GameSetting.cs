using UnityEngine;

[CreateAssetMenu(fileName = "GameSetting", menuName = "ScriptableObjects/GameSetting")]
public class GameSetting : ScriptableObject
{
    [Header("Camera FOV")]
    [SerializeField] private float cameraFov = 60f;
    [SerializeField] private float minOrthographicSize = 7;
    [SerializeField] private float maxOrthographicSize = 15;
    [SerializeField] private float zoomSensitivity = 10f;
    [SerializeField] private float screenEdgeSize = 20f; // px


    [Header("Camera Move")]
    [SerializeField] private float cameraMoveSpeed = 10f;
    [SerializeField] private float xMin = -10f;
    [SerializeField] private float xMax = 10f;
    [SerializeField] private float zMin = -10f;
    [SerializeField] private float zMax = 10f;
    // Properties để lấy giá trị từ script khác
    public float CameraFov => cameraFov;
    public float MinCameraFov => minOrthographicSize;
    public float MaxCameraFov => maxOrthographicSize;
    public float ZoomSensitivity => zoomSensitivity;
    public float ScreenEdgeSize => screenEdgeSize;
    public float CameraMoveSpeed => cameraMoveSpeed;
    public float XMin => xMin;
    public float XMax => xMax;
    public float ZMin => zMin;
    public float ZMax => zMax;
}
