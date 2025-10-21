using UnityEngine;
public class SelfDestruct : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.2f; // Tia sét sẽ tồn tại trong 0.2 giây
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}