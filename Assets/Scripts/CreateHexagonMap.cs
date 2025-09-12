using UnityEngine;

public class CreateHexagonMap : MonoBehaviour
{
    public GameObject hexPrefab; // prefab của 1 tile hex
    public int width = 10;       // số hex theo chiều ngang
    public int height = 10;      // số hex theo chiều dọc
    public float hexRadius = 1f; // bán kính hex (đo từ tâm đến 1 đỉnh)

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        float xOffset = Mathf.Sqrt(3) * hexRadius;
        float zOffset = 1.5f * hexRadius;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float xPos = x * xOffset;

                // dịch hàng lẻ sang phải
                if (z % 2 == 1)
                {
                    xPos += xOffset / 2f;
                }

                Vector3 pos = new Vector3(xPos, 0, z * zOffset);
                Instantiate(hexPrefab, pos, Quaternion.identity, this.transform);
            }
        }
    }
}
