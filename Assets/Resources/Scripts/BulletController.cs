﻿using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
	public float TimeToLive = 5f;
	private float ttl;
	public float damage;
    public float knockbackForce = 20f;
	public GameObject Owner;

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

		if (Owner.GetComponent<PlayerController>() == null)
		{
			var player = other.GetComponent<PlayerController>();
			if (player != null)
			{
				player.Hit(damage, transform.up, knockbackForce);
			} 
		}
		
		var enemy = other.GetComponent<Enemy>();
		if (enemy != null)
		{
			enemy.Hit(damage, transform.up, knockbackForce);
		}

		var enemySpawner = other.GetComponent<EnemySpawner>();
		if (enemySpawner != null)
		{
			enemySpawner.Hit(damage, transform.up, knockbackForce);
		}
		Destroy(gameObject);
	}
}
