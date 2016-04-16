using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
	public float TimeToLive = 5f;
	private float ttl;
	public float damage;
    public float knockbackForce = 20f;

    // Use this for initialization
    void Start()
	{
		ttl = TimeToLive;
	}

	// Update is called once per frame
	void Update()
	{
		ttl -= Time.deltaTime;
		if (ttl < 0)
		{
			Destroy(gameObject);
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		print("hit");

		var player = other.GetComponent<PlayerController>();
		if (player != null)
		{
			player.Hit(damage);
		}
		
		var enemy = other.GetComponent<Enemy>();
		if (enemy != null)
		{
			enemy.Hit(damage, transform.up, knockbackForce);            
		}
        Destroy(gameObject);
	}
}
