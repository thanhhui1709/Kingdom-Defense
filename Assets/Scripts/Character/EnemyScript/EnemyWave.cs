using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWave", menuName = "ScriptableObjects/EnemyWave")]
public class EnemyWave : ScriptableObject
{
    public List<WaveData> waveData;
    public float delayForTheNextWave;
    [HideInInspector]
    public bool isDoneSpawned = false;


    [System.Serializable]
    public class WaveData
    {
        //public List<GameObject> dropItems;
        //public float droppedTime;
        public GameObject enemyPrefab;
        public int numberPerWave;
        //public float health;
        //public int reward;
        //public float speed;
        //public float offset;
        public float delaySpawnPrefab;
        public float delayForTheNextWaveData;
    }
}