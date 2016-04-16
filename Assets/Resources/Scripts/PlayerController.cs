using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Combat")]
    public float health = 100f;
	
	public GameObject bulletStart;
	public bool SwitchingWeapon;
	private float shootCooldown = 0f;

	[Header("Axes")]
	public string horizontalAxis = "";
	public string verticalAxis = "";
	public string horizontal2Axis = "";
	public string vertical2Axis = "";
	public string shootAxis = "";
	public string WeaponSwitchAxis = "";
	
	public float speed = 1.0f;
	private Rigidbody2D rigidbodys;

    [Header("Graphics")]
    [SerializeField] SpriteRenderer PlayerSprite;
    [SerializeField] float DamageRedFlashDuration = 0.2f;

	public WeaponDescription[] Weapons = new WeaponDescription[0];
	public int currentWeaponIndex = 0;
	[SerializeField]
	public WeaponDescription currentWeapon { get { return currentWeaponIndex < Weapons.Length ? Weapons[currentWeaponIndex] : null; } }
	Quaternion rotation;


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

		if (horizontalAxis != "")
		{
			HandleMovement();

			HandleShooting(); 
		}
	}

	private void HandleMovement()
	{
		var targetVelocity = (Vector2.right * Input.GetAxis(horizontalAxis) + Vector2.up * -Input.GetAxis(verticalAxis)).normalized * speed;
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

		Vector2 shootDirection = Vector2.right * -Input.GetAxis(horizontal2Axis) + Vector2.up * Input.GetAxis(vertical2Axis);

		if (shootDirection.sqrMagnitude > 0.0f)
		{
			rotation = Quaternion.LookRotation(shootDirection, Vector3.forward);
			rotation.x = 0;
			rotation.y = 0;
			//transform.rotation = rot;

			if (shootCooldown < 0f)
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
				MakeBullets(currentWeapon, bulletStart.transform.position, rotation);
				break;
			case WeaponType.Shotgun:
				MakeBullets(currentWeapon, bulletStart.transform.position, rotation);
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
        StartCoroutine(FlashRed(DamageRedFlashDuration));
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
                PlayerSprite.color = Color.Lerp(startColor, endColor, t * 2f);
            }
            else
            {
                PlayerSprite.color = Color.Lerp(endColor, startColor, t * 2f);
            }

            yield return null;
        }
        while (t < 1f);

        PlayerSprite.color = startColor;
    }
}
