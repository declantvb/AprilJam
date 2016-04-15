using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public string horizontalAxis = "Horizontal";
	public string verticalAxis = "Vertical";
	public float speed = 1.0f;

	public GameObject bullet;
	public float shootDelay = 0.1f;
	private bool canShoot = true;
	private Rigidbody2D rigidbodys;

	// Use this for initialization
	void Start()
	{
		canShoot = true;
		rigidbodys = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update()
	{
		//move
		var targetVelocity = (Vector2.right * Input.GetAxis(horizontalAxis) + Vector2.up * Input.GetAxis(verticalAxis)).normalized * speed;
		var delta = targetVelocity - rigidbodys.velocity;
		rigidbodys.AddForce(delta, ForceMode2D.Impulse);

		//shoot
		//Vector3 shootDirection = Vector3.right * Input.GetAxis(horizontalAxis) + Vector3.up * Input.GetAxis(verticalAxis);
		//if (shootDirection.sqrMagnitude > 0.0f)
		//{
		//	transform.rotation = Quaternion.LookRotation(shootDirection, Vector3.back);

		//	if (canShoot)
		//	{
		//		Instantiate(bullet, transform.position, transform.rotation);

		//		canShoot = false;
		//		Invoke("ResetShot", shootDelay);
		//	}
		//}
	}
}
