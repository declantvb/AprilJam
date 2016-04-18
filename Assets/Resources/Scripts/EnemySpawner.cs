using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
	[Header("Combat")]
	public float health = 100f;
	[SerializeField] float DamageRedFlashDuration = 0.2f;
    [SerializeField] SpriteRenderer Sprite;
	[SerializeField] ParticleSystem BloodParticles;
	[SerializeField] bool dead;
	[SerializeField] GameObject DeadPrefab;

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
	[SerializeField] SpriteAnimation Anim_Die;
    [SerializeField] int state;

    private GameController gameController;

    void Start()
    {
        nextSpawnDelay = Random.Range(MinSpawnDelay, MaxSpawnDelay);
		Sprite = GetComponent<SpriteRenderer>();

        gameController = FindObjectOfType<GameController>();
	}
    
    void Update()
    {
		if (!dead)
		{
			if (health <= 0)
            {
                StopAllCoroutines();

                Anim_Die.PlayOneShot(); 
				dead = true;

                SoundEffects.Singleton.Play("Egg Burst");

                gameController.EggKilled();
            }
			else
			{
				spawnElapsed += Time.deltaTime;
				if (spawnElapsed >= nextSpawnDelay)
				{
					spawnElapsed = 0;
					nextSpawnDelay = Random.Range(MinSpawnDelay, MaxSpawnDelay);
					StartCoroutine(HatchNewEnemy());
				}
			}
		}
		else if (!Anim_Die.IsPlaying)
		{
			Instantiate(DeadPrefab, transform.position, transform.rotation);
			Destroy(gameObject);
		}
    }
	
	internal void Hit(float damage, Vector3 hitDirection, float hitForce)
	{
		health -= damage;
		StartCoroutine(FlashRed(DamageRedFlashDuration));

		//Play blood particles
		BloodParticles.transform.forward = hitDirection;
		BloodParticles.Play();
	}

	IEnumerator FlashRed(float duration)
	{
		float elapsed = 0;
		Color startColor = new Color(1, 1, 1);
		Color endColor = new Color(1, 0, 0);
		float t = 0;

		do
		{
			elapsed += Time.deltaTime;
			t = elapsed / duration;

			if (t < 0.5f)
			{
				Sprite.color = Color.Lerp(startColor, endColor, t * 2f);
			}
			else
			{
				Sprite.color = Color.Lerp(endColor, startColor, t * 2f);
			}

			yield return null;
		}
		while (t < 1f);

		Sprite.color = startColor;
	}

	IEnumerator HatchNewEnemy()
    {
        state = 0;

        //Halt idle animation
        Anim_Idle.Stop();

        do
        {
			if (health <= 0)
			{
				break;
			}
			else if (state == 0)
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

                gameController.CheckAliens();
            }
            else if (state == 3 && !Anim_Uncrack.IsPlaying)
            {
                //Return to idle animation
                Anim_Idle.Play();
                state++;
            }

            yield return null;
        }
        while (state != 4);     //Loop until animation sequence is complete    

    }
}
