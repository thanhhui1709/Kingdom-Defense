using UnityEngine;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField]
    private float previewOffset = 0.6f;

  
    private GameObject previewObject;
    [SerializeField]
    private GameObject cellIndicator;

    [SerializeField]
    private Material previewMaterial;
    private Material[] originalMaterials;

    private void Start()
    {
        if (cellIndicator != null)
            cellIndicator.gameObject.SetActive(false);
    }

    public void ShowingPreview(GameObject prefab, Vector2Int size)
    {
        // Xóa preview cũ nếu còn
        if (previewObject != null)
            Destroy(previewObject);

        previewObject = Instantiate(prefab);
        previewObject.name = prefab.name + "_Preview";
        previewObject.layer=2;

        // gán transparent material
        AdjustPreviewMaterial(previewObject);

        AdjustCursor(size);
    }

    private void AdjustCursor(Vector2Int size)
    {
        if (cellIndicator != null && size.x > 0 && size.y > 0)
        {
            cellIndicator.transform.localScale = new Vector3(size.x + previewOffset, 0.1f, size.y);
            cellIndicator.gameObject.SetActive(true);
        }
    }

    private void AdjustPreviewMaterial(GameObject previewGO)
    {
        MeshRenderer[] renderers = previewGO.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in renderers)
        {
            originalMaterials = renderer.materials;

            Material[] mats = new Material[renderer.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(previewMaterial); // tạo instance riêng
            }

            renderer.materials = mats;
        }
    }

    public void HidePreview()
    {
        if (previewObject != null)
            previewObject.SetActive(false);

        if (cellIndicator != null)
            cellIndicator.SetActive(false);
    }

    public void DestroyPreview()
    {
        if (previewObject != null)
            Destroy(previewObject);

        if (cellIndicator != null)
            cellIndicator.SetActive(false);
    }

    /// <summary>
    /// Update vị trí và màu của preview object + cursor
    /// </summary>
    public void UpdatePosition(Vector3 position, bool validity)
    {
        MovePreview(position);
        MoveCursor(position);
        DisplayValidColor(validity);
    }

    private void DisplayValidColor(bool validity)
    {
        Color c = validity ? Color.green : Color.red;
        c.a = 0.5f; 

        if (previewObject != null)
        {
            MeshRenderer[] renderers = previewObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    mat.color = c;
                }
            }
        }

        if (cellIndicator != null)
        {
            SpriteRenderer r = cellIndicator.GetComponent<SpriteRenderer>();
            if (r != null)
                r.color = c;
        }
    }

    private void MoveCursor(Vector3 position)
    {
        if (cellIndicator != null)
            cellIndicator.transform.position = position;
    }

    private void MovePreview(Vector3 position)
    {
        if (previewObject != null)
            previewObject.transform.position =new Vector3(position.x,position.y+previewOffset,position.z);
    }
}
