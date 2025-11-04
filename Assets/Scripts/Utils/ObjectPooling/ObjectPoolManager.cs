using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static List<PoolObjectInfo> objectPools = new List<PoolObjectInfo>();

    public static PoolType poolType;
    private GameObject _objectPoolEmptyHolder;
    private static GameObject towerProjectilePool;
    private static GameObject enemyProjectilePool;
    private static GameObject unitProjectilePool;
    private static GameObject particle;
    private static GameObject audioPool;
    private static GameObject enemyPool;
    private static GameObject unitPool;
    private static GameObject towerPool;
    public enum PoolType
    {
        Enemy,
        Unit,
        Tower,
        TowerProjectile,
        EnemyProjectile,
        UnitProjectile,
        Particle,
        Audio,
        None,
    }
    private void Awake()
    {
        SetUpEmpty();
        CreateDefaultAudioObject();
    }

    private void SetUpEmpty()
    {
        _objectPoolEmptyHolder = new GameObject("PoolObjects");

        enemyPool = new GameObject("EnemyPool");
        enemyPool.transform.SetParent(_objectPoolEmptyHolder.transform);

        unitPool = new GameObject("UnitPool");
        unitPool.transform.SetParent(_objectPoolEmptyHolder.transform);

        towerPool = new GameObject("TowerPool");
        towerPool.transform.SetParent(_objectPoolEmptyHolder.transform);

        unitProjectilePool = new GameObject("UnitProjectilePool");
        unitProjectilePool.transform.SetParent(_objectPoolEmptyHolder.transform);

        towerProjectilePool = new GameObject("TowerProjectilePool");
        towerProjectilePool.transform.SetParent(_objectPoolEmptyHolder.transform);

        enemyProjectilePool = new GameObject("EnemyProjectilePool");
        enemyProjectilePool.transform.SetParent(_objectPoolEmptyHolder.transform);

        particle = new GameObject("ParticlePool");
        particle.transform.SetParent(_objectPoolEmptyHolder.transform);

        audioPool = new GameObject("AudioPool");
        audioPool.transform.SetParent(_objectPoolEmptyHolder.transform);
    }
    private void CreateDefaultAudioObject()
    {

        PoolObjectInfo pool = objectPools.Find(x => x.poolName == "AudioClip");
        if (pool == null)
        {
            pool = new PoolObjectInfo { poolName = "AudioClip" };
            objectPools.Add(pool);
        }

        // Tạo 1 object mới
        GameObject audioClipGO = new GameObject("AudioClip");
        audioClipGO.AddComponent<PoolAudio>();
        audioClipGO.transform.SetParent(audioPool.transform);

        // Tắt nó đi và thêm vào pool
        audioClipGO.SetActive(false);
        pool.poolObjects.Add(audioClipGO);
    }

    // (Hàm này được gọi khi pool hết object)
    private static GameObject CreateAudioObjectStatic()
    {
        GameObject audioClipGO = new GameObject("AudioClip");
        audioClipGO.AddComponent<PoolAudio>();
        audioClipGO.transform.SetParent(audioPool.transform);
  
        return audioClipGO;
    }

    public static GameObject SpawnObject(GameObject gameObject, Vector3 spawnPos, Quaternion rotation, PoolType poolType = PoolType.None)
    {
        PoolObjectInfo pool = objectPools.Find(x => x.poolName == gameObject.name);
        if (pool == null)
        {
            pool = new PoolObjectInfo();
            pool.poolName = gameObject.name;
            objectPools.Add(pool);

        }
        GameObject obj = pool.poolObjects.FirstOrDefault();
        if (obj == null)
        {
            GameObject parent = SetParentGameObject(poolType);
            obj = Instantiate(gameObject, spawnPos, rotation);
            if (parent != null)
            {
                obj.transform.SetParent(parent.transform);
            }
        }
        else
        {
            obj.transform.position = spawnPos;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            pool.poolObjects.Remove(obj);
        }
        return obj;
    }
    public static GameObject SpawnObject(GameObject gameObject, PoolType poolType = PoolType.None)
    {
        PoolObjectInfo pool = objectPools.Find(x => x.poolName == gameObject.name);
        if (pool == null)
        {
            pool = new PoolObjectInfo();
            pool.poolName = gameObject.name;
            objectPools.Add(pool);

        }
        GameObject obj = pool.poolObjects.FirstOrDefault();
        if (obj == null)
        {
            GameObject parent = SetParentGameObject(poolType);
            obj = Instantiate(gameObject);
            if (parent != null)
            {
                obj.transform.SetParent(parent.transform);
            }
        }
        else
        {
            obj.SetActive(true);
            pool.poolObjects.Remove(obj);
        }
        return obj;
    }
    private static PoolAudio GetAudioPlayerFromPool()
    {
        PoolObjectInfo pool = objectPools.Find(x => x.poolName.Equals("AudioClip"));
        if (pool == null)
        {
          
            pool = new PoolObjectInfo { poolName = "AudioClip" };
            objectPools.Add(pool);
        }

        GameObject obj = pool.poolObjects.FirstOrDefault();
        if (obj != null) 
        {
            pool.poolObjects.Remove(obj);
            obj.SetActive(true);
        }
        else 
        {
            obj = CreateAudioObjectStatic();
          
        }
        return obj.GetComponent<PoolAudio>();
    }
    public static void PlayAudio(AudioClip audioClip, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        PoolAudio player = GetAudioPlayerFromPool();
        player.Play(audioClip, position, volume, minDistance, maxDistance);
    }

    /// <summary>
    /// Chơi âm thanh 3D tại 1 vị trí (dùng min/max mặc định)
    /// </summary>
    public static void PlayAudio(AudioClip audioClip, Vector3 position, float volume)
    {
        PoolAudio player = GetAudioPlayerFromPool();
        // Gọi hàm Play của PoolAudio với min/max mặc định
        player.Play(audioClip, position, volume);
    }

    /// <summary>
    /// Chơi âm thanh 2D (cho UI, nhạc...)
    /// </summary>
    public static void PlayAudio2D(AudioClip audioClip, float volume)
    {
        PoolAudio player = GetAudioPlayerFromPool();
        player.Play2D(audioClip, volume);
    }
    public static void ReturnObject(GameObject gameObject)
    {
        string poolName;


        if (gameObject.name == "AudioClip")
        {
            poolName = "AudioClip";
        }
        else if (gameObject.name.EndsWith("(Clone)"))
        {
      
            poolName = gameObject.name.Substring(0, gameObject.name.Length - 7);
        }
        else
        {
     
            poolName = gameObject.name;
        }

        PoolObjectInfo pool = objectPools.Find(x => x.poolName.Equals(poolName));

        if (pool != null)
        {
            // Reset lại Audio Object
            if (poolName == "AudioClip")
            {
                gameObject.transform.SetParent(audioPool.transform);
                gameObject.transform.position = audioPool.transform.position;
            }

            gameObject.SetActive(false);
            pool.poolObjects.Add(gameObject);
        }
        else
        {
            Debug.LogWarning("Pool not found for " + gameObject.name + " (Pool name was '" + poolName + "')");
            Destroy(gameObject);
        }
    }
    public static GameObject SetParentGameObject(PoolType type)
    {
        switch (type)
        {
            //projectile
            case PoolType.TowerProjectile:
                return towerProjectilePool;

            case PoolType.UnitProjectile:
                return unitProjectilePool;

            case PoolType.EnemyProjectile:
                return enemyProjectilePool;

            //character
            case PoolType.Tower:
                return towerPool;

            case PoolType.Unit:
                return unitPool;
            case PoolType.Enemy:
                return enemyPool;


            //other
            case PoolType.Particle:
                return particle;

            case PoolType.Audio:
                return audioPool;

            case PoolType.None:
                return null;

            default:
                return null;

        }
    }
}
public class PoolObjectInfo
{
    public string poolName;
    public List<GameObject> poolObjects = new List<GameObject>();
}

