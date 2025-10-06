using UnityEngine;

public class CameraDragController : MonoBehaviour
{
    private Vector3 dragOrigin;

    void Update()
    {
        // Khi nhấn chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        // Khi giữ chuột trái
        if (Input.GetMouseButton(0))
        {
            Vector3 difference = Input.mousePosition - dragOrigin;
            Vector3 move = new Vector3(-difference.x, 0, -difference.y) * 0.1f; // điều chỉnh tốc độ bằng 0.1
            Camera.main.transform.Translate(move, Space.World);
            dragOrigin = Input.mousePosition;
        }
    }
}
