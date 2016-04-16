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

    [Header("Animations")]
    [SerializeField] SpriteAnimation Anim_Idle;
    [SerializeField] SpriteAnimation Anim_Puru;
    [SerializeField] SpriteAnimation Anim_CrackPuru;
    [SerializeField] SpriteAnimation Anim_Uncrack;
    [SerializeField] int state;

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
            StartCoroutine(HatchNewEnemy());            
        }      
    }

    IEnumerator HatchNewEnemy()
    {
        state = 0;

        //Halt idle animation
        Anim_Idle.Play(false);

        do
        {
            if (state == 0)
            {
                //Play puru animation
                Anim_Puru.PlayOneShot();
                state++;
            }
            else if (state == 1 && !Anim_Puru.IsPlaying)
            {
                //Play crackpuru animation
                Anim_CrackPuru.PlayOneShot();
                state++;
            }
            else if (state == 2 && !Anim_CrackPuru.IsPlaying)
            {
                //Play uncrack animation
                Anim_Uncrack.PlayOneShot();
                state++;

                //Spawn new enemy
                GameObject newEnemy = (GameObject)Instantiate(EnemyPrefab);
                newEnemy.transform.position = transform.position;
                SpawnedEnemies.Add(newEnemy.GetComponent<Enemy>());
            }
            else if (state == 3 && !Anim_Uncrack.IsPlaying)
            {
                //Return to idle animation
                Anim_Idle.Play(true);
                state++;
            }

            yield return null;
        }
        while (state != 4);     //Loop until animation sequence is complete    

    }
}
