using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] float MinSpawnDelay = 3f;
    [SerializeField] float MaxSpawnDelay = 5f;
    float spawnElapsed;
    float nextSpawnDelay;

    List<Enemy> SpawnedEnemies = new List<Enemy>();

    void Start()
    {
        nextSpawnDelay = Random.Range(MinSpawnDelay, MaxSpawnDelay);
    }
    
    void Update()
    {
        spawnElapsed += Time.deltaTime;
        if (spawnElapsed >= nextSpawnDelay)
        {
            spawnElapsed = 0;
            nextSpawnDelay = Random.Range(MinSpawnDelay, MaxSpawnDelay);

            GameObject newEnemy = (GameObject)Instantiate(EnemyPrefab);
            newEnemy.transform.position = transform.position;
            SpawnedEnemies.Add(newEnemy.GetComponent<Enemy>());
        }

    }
}
