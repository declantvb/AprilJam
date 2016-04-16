using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
	[Header("Combat")]
	public float health;

	public string horizontalAxis = "Horizontal";
	public string verticalAxis = "Vertical";
	public float speed = 1.0f;

	public GameObject bulletPrefab;
	public GameObject bulletStart;
	public string horizontal2Axis = "Horizontal2";
	public string vertical2Axis = "Vertical2";
	private const string shootAxis = "Fire1";
	public bool joystickShooting = true;
	public float shootDelay = 1f;
	private float shootCooldown = 0f;

	private Rigidbody2D rigidbodys;
	public float bulletSpeed;

	// Use this for initialization
	void Start()
	{
		health = 100f;
		shootCooldown = 0f;
		rigidbodys = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update()
	{
		if (health <= 0)
		{
			Destroy(gameObject);
		}

		//move
		var targetVelocity = (Vector2.right * Input.GetAxis(horizontalAxis) + Vector2.up * Input.GetAxis(verticalAxis)).normalized * speed;
		var delta = targetVelocity - rigidbodys.velocity;
		rigidbodys.MovePosition(rigidbodys.position + delta * Time.deltaTime);

		//shoot
		Vector2 shootDirection = Vector2.right * Input.GetAxis(horizontal2Axis) + Vector2.up * Input.GetAxis(vertical2Axis);


		if (shootDirection.sqrMagnitude == 0.0f)
		{
			joystickShooting = false;
			var mousePos = Input.mousePosition;
			shootDirection = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
		} else
		{
			joystickShooting = true;
		}

		if (shootDirection.sqrMagnitude > 0.0f)
		{
			var rot = Quaternion.LookRotation(shootDirection, Vector3.forward);
			rot.x = 0;
			rot.y = 0;
			transform.rotation = rot;

			if (shootCooldown < 0f && (joystickShooting || Input.GetAxis(shootAxis) > 0))
			{
				var newBullet = (GameObject)Instantiate(bulletPrefab, bulletStart.transform.position, transform.rotation);
				newBullet.GetComponent<Rigidbody2D>().AddForce(newBullet.transform.up * bulletSpeed, ForceMode2D.Impulse);
				
				shootCooldown = shootDelay;
			}
		}
		shootCooldown -= Time.deltaTime;
	}

	internal void Hit(float damage)
	{
		health -= damage;
	}
}
