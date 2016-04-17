using UnityEngine;
using System.Collections;

public class RocketController : BulletController
{
	public GameObject ExplosionPrefab;
	public float Acceleration; 

	public new void Update()
	{
		GetComponent<Rigidbody2D>().AddForce(transform.up * Acceleration, ForceMode2D.Force);
		base.Update();
	}

	public new void OnTriggerEnter2D(Collider2D other)
	{
		Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);

		var hits = Physics2D.OverlapCircleAll(transform.position, 2f);
		foreach (var col in hits)
		{
			DoDamageTo(col, damage, transform.up);
		}

		Destroy(gameObject);
	}
}