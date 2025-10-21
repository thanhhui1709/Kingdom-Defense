using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AutoHitbox : MonoBehaviour
{
    void Start()
    {
        var skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedRenderers.Length == 0) return;

        Bounds combinedBounds = skinnedRenderers[0].bounds;
        for (int i = 1; i < skinnedRenderers.Length; i++)
            combinedBounds.Encapsulate(skinnedRenderers[i].bounds);

        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            box.center = transform.InverseTransformPoint(combinedBounds.center);
            box.size = combinedBounds.size;
        }
        else if (col is CapsuleCollider capsule)
        {
            capsule.center = transform.InverseTransformPoint(combinedBounds.center);
            capsule.height = combinedBounds.size.y;
            capsule.radius = Mathf.Max(combinedBounds.size.x, combinedBounds.size.z) / 2f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider box)
                Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
