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

    [Header("Audio Settings")]
    [Range(0.0001f, 1f)][SerializeField] private float musicVolume = 0.7f;
    [Range(0.0001f, 1f)][SerializeField] private float sfxVolume = 1.0f;

  
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

    public float MusicVolume
    {
        get => musicVolume;
        set => musicVolume = Mathf.Clamp(value, 0.0001f, 1f);
    }
    public float SFXVolume
    {
        get => sfxVolume;
        set => sfxVolume = Mathf.Clamp(value, 0.0001f, 1f);
    }
}
