using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Combat")]
    public float health = 100f;

	public string horizontalAxis = "Horizontal";
	public string verticalAxis = "Vertical";
	public float speed = 1.0f;

	public GameObject bulletStart;
	public string horizontal2Axis = "Horizontal2";
	public string vertical2Axis = "Vertical2";
	private const string shootAxis = "Fire1";
	private const string WeaponSwitchAxis = "WeaponSwitch";
	public bool SwitchingWeapon;
	public bool joystickShooting = true;
	private float shootCooldown = 0f;

	private Rigidbody2D rigidbodys;
	public float bulletSpeed;

	public WeaponDescription[] Weapons = new WeaponDescription[0];
	public int currentWeaponIndex = 0;
	[SerializeField]
	public WeaponDescription currentWeapon { get { return currentWeaponIndex < Weapons.Length ? Weapons[currentWeaponIndex] : null; } }


	// Use this for initialization
	void Start()
	{
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

		HandleMovement();

		HandleShooting();
	}

	private void HandleMovement()
	{
		var targetVelocity = (Vector2.right * Input.GetAxis(horizontalAxis) + Vector2.up * Input.GetAxis(verticalAxis)).normalized * speed;
		var delta = targetVelocity - rigidbodys.velocity;
		rigidbodys.MovePosition(rigidbodys.position + delta * Time.deltaTime);
	}

	private void HandleShooting()
	{
		if (Input.GetAxis(WeaponSwitchAxis) > 0)
		{
			if (!SwitchingWeapon)
			{
				SwitchingWeapon = true;
				currentWeaponIndex = (currentWeaponIndex + 1) % Weapons.Length;
			}
		}
		else
		{
			SwitchingWeapon = false;
		}

		Vector2 shootDirection = Vector2.right * Input.GetAxis(horizontal2Axis) + Vector2.up * Input.GetAxis(vertical2Axis);

		if (shootDirection.sqrMagnitude == 0.0f)
		{
			joystickShooting = false;
			var mousePos = Input.mousePosition;
			shootDirection = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
		}
		else
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
				FireWeapon();
			}
		}
		shootCooldown -= Time.deltaTime;
	}

	private void FireWeapon()
	{
		if (currentWeapon == null)
		{
			Debug.LogError("no weapon");
			return;
		}
		switch (currentWeapon.Type)
		{
			case WeaponType.Auto:
				MakeBullets(currentWeapon, bulletStart.transform.position, transform.rotation);
				break;
			case WeaponType.Shotgun:
				MakeBullets(currentWeapon, bulletStart.transform.position, transform.rotation);
				break;
			default:
				Debug.LogError("bad weapon type");
				break;
		}

		shootCooldown = currentWeapon.Cooldown;
	}

	private void MakeBullets(WeaponDescription weapon, Vector3 position, Quaternion rotation)
	{
		for (int i = 0; i < weapon.ShellCount; i++)
		{
			var randAngle = Quaternion.AngleAxis((UnityEngine.Random.value - 0.5f) / weapon.Accuracy, Vector3.forward);
			var newBullet = (GameObject)Instantiate(weapon.BulletPrefab, position, rotation * randAngle);
			var randomSpeedFactor = 1;// + (UnityEngine.Random.value - 0.5f) / 10;
			newBullet.GetComponent<Rigidbody2D>().AddForce(newBullet.transform.up * weapon.ShellSpeed * randomSpeedFactor, ForceMode2D.Impulse);
		}
	}

	internal void Hit(float damage)
	{
		health -= damage;
	}
}
