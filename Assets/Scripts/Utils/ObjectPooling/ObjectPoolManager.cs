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
        //CreateDefaultAudioObject();
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
    //private GameObject CreateDefaultAudioObject()
    //{
    //    GameObject audioClipGO = new GameObject("AudioClip");
    //    audioClipGO.AddComponent<PoolAudioPlayer>();
    //    audioClipGO.transform.SetParent(audioPool.transform);
    //    objectPools.Add(new PoolObjectInfo { poolName = audioClipGO.name, poolObjects = new List<GameObject>() { audioClipGO } });
    //    return audioClipGO;
    //}
    //private static GameObject CreateAudioObjectStatic()
    //{
    //    GameObject audioClipGO = new GameObject("AudioClip");
    //    audioClipGO.AddComponent<PoolAudioPlayer>();
    //    audioClipGO.transform.SetParent(audioPool.transform);
    //    objectPools.Add(new PoolObjectInfo { poolName = audioClipGO.name, poolObjects = new List<GameObject>() { audioClipGO } });
    //    return audioClipGO;
    //}
    //private static GameObject CreateAudioObjectStatic(GameObject parent)
    //{
    //    GameObject audioClipGO = new GameObject("AudioClip");
    //    audioClipGO.AddComponent<PoolAudioPlayer>();
    //    audioClipGO.transform.SetParent(parent.transform);
    //    objectPools.Add(new PoolObjectInfo { poolName = audioClipGO.name, poolObjects = new List<GameObject>() { audioClipGO } });
    //    return audioClipGO;
    //}

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
    //public static GameObject PlayAudio(AudioClip audioClip, float volume, GameObject parent = null)
    //{
    //    PoolObjectInfo pool = objectPools.Find(x => x.poolName.Equals("AudioClip"));
    //    if (pool == null)
    //    {
    //        pool = new PoolObjectInfo();
    //        pool.poolName = "AudioClip";
    //        objectPools.Add(pool);

    //    }
    //    GameObject obj = pool.poolObjects.FirstOrDefault();
    //    if (obj == null)
    //    {
    //        if (parent != null)
    //        {
    //            obj = CreateAudioObjectStatic(parent);
    //        }
    //        else
    //        {

    //            obj = CreateAudioObjectStatic();
    //        }


    //    }
    //    else
    //    {

    //        obj.SetActive(true);



    //    }
    //    PoolAudioPlayer player = obj.GetComponent<PoolAudioPlayer>();
    //    player.PlayAudioClip(audioClip, volume);
    //    pool.poolObjects.Remove(obj);
    //    return obj;
    //}
    public static void ReturnObject(GameObject gameObject)
    {
        string name = gameObject.name.Substring(0, gameObject.name.Length - 7); // Remove "(Clone)" from the name
        PoolObjectInfo pool = objectPools.Find(x => x.poolName.Equals(name) || x.poolName.Equals(gameObject.name));
        if (pool != null)
        {
            if (gameObject.name.Equals("AudioClip"))
            {
                if (gameObject.transform.parent != null)
                {
                    gameObject.transform.parent = null;
                    gameObject.transform.SetParent(SetParentGameObject(PoolType.Audio).transform);
                    gameObject.transform.position = audioPool.transform.position;

                }
            }
            gameObject.SetActive(false);
            pool.poolObjects.Add(gameObject);
        }
        else
        {
            Debug.LogWarning("Pool not found for " + gameObject.name);
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

