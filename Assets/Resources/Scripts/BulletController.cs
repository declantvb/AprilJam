using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
	public float TimeToLive = 5f;
	private float ttl;
	public float damage;
    public float knockbackForce = 20f;
	public GameObject Owner;
	[SerializeField] SpriteAnimation Fly;
	[SerializeField] SpriteAnimation Hit;
	private bool destroyed;

	// Use this for initialization
	void Start()
	{
		ttl = TimeToLive;
		Fly.Play();
	}

	// Update is called once per frame
	public void Update()
	{
		ttl -= Time.deltaTime;
		if (ttl < 0 || (destroyed && !Hit.IsPlaying))
		{
			Destroy(gameObject);
		}
	}

	public void OnTriggerEnter2D(Collider2D other)
	{
		DoDamageTo(other, damage, transform.up);
		Hit.PlayOneShot();
		GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		destroyed = true;
	}

	public void DoDamageTo(Collider2D other, float damage, Vector3 direction)
	{
		if (Owner.GetComponent<PlayerController>() == null)
		{
			var player = other.GetComponent<PlayerController>();
			if (player != null)
			{
				player.Hit(damage, direction, knockbackForce);
			}
		}

		var enemy = other.GetComponent<Enemy>();
		if (enemy != null)
		{
			enemy.Hit(damage, direction, knockbackForce);
		}

		var enemySpawner = other.GetComponent<EnemySpawner>();
		if (enemySpawner != null)
		{
			enemySpawner.Hit(damage, direction, knockbackForce);
		}
	}
}
