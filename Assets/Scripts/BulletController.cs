using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
	public float TimeToLive = 5f;
	private float ttl;

	// Use this for initialization
	void Start()
	{
		ttl = TimeToLive;
	}

	// Update is called once per frame
	void Update()
	{
		transform.position = transform.position + transform.up;

		ttl -= Time.deltaTime;
		if (ttl < 0)
		{
			Destroy(gameObject);
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		var player = other.GetComponent<PlayerController>();
		if (player != null)
		{
			//hit
		}
		
		var enemy = other.GetComponent<Enemy>();
		if (enemy != null)
		{
			//hit
		}

		Destroy(gameObject);
	}
}
