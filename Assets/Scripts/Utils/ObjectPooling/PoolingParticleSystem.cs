using UnityEngine;

public class PoolingParticleSystem : MonoBehaviour
{
    private void OnParticleSystemStopped()
    {
        ObjectPoolManager.ReturnObject(gameObject);
    }
}
