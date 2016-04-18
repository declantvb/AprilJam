using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
	[Header("Combat")]
	public float health = 100f;

    public bool IsAlive { get; private set; }

	public GameObject bulletStart;
	public bool SwitchingWeapon;
	private float shootCooldown = 0f;
    
	public float speed = 1.0f;
	private Rigidbody2D rb2d;

	[Header("Graphics")]
	[SerializeField]
	SpriteRenderer PlayerSprite;
	[SerializeField]
	float DamageRedFlashDuration = 0.2f;
	[SerializeField]
	ParticleSystem BloodParticles;
    [SerializeField] GameObject GunHolder;
    [SerializeField] SpriteAnimation Anim_Death;

    public WeaponDescription[] Weapons = new WeaponDescription[0];
	public int currentWeaponIndex = 0;
	[SerializeField]
	public WeaponDescription currentWeapon { get { return currentWeaponIndex < Weapons.Length ? Weapons[currentWeaponIndex] : null; } }
	public Quaternion rotation;
    public Controller Controller;

    bool deathAnimFinished = false;
    private bool shootNextFrame;

    public PlayerController()
    {
        IsAlive = true;
    }


    // Use this for initialization
    void Start()
	{
		shootCooldown = 0f;
		rb2d = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update()
	{
        if (health <= 0)
        {
            IsAlive = false;
            DestroyImmediate(GetComponentInChildren<WalkAnimator>());
            DestroyImmediate(GetComponentInChildren<GunMover>());
            DestroyImmediate(GunHolder);
            DestroyImmediate(rb2d);
            
            foreach(Transform t in GetComponentsInChildren<Transform>())
            {
                if (t.GetComponentInChildren<SpriteAnimation>() == null && t.GetComponentInChildren<ParticleSystem>() == null)
                {
                    DestroyImmediate(t.gameObject);
                }
            }

            foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            {
                DestroyImmediate(col);
            }
                       

            //Do death animation until complete
            if (!Anim_Death.IsPlaying && !deathAnimFinished)
            {
                Anim_Death.PlayOneShot();

                SoundEffects.Singleton.Play("Player Death");
            }
            else
            {
                deathAnimFinished = true;             
            }
                                   
            if (Input.GetAxis(Controller.FireAxis) > 0 && deathAnimFinished)
            {
                Controller.Spawn();
                SoundEffects.Singleton.Play("Respawn");
                Destroy(transform.parent.GetComponentInChildren<Camera>().gameObject);
                Destroy(this);
            }            
        }
        else
        {
            if (Controller != null)
            {
                HandleMovement();

                HandleShooting();
            }
        }
	}

	private void HandleMovement()
	{
		var targetVelocity = (Vector2.right * Input.GetAxis(Controller.HorizontalAxis) + Vector2.up * -Input.GetAxis(Controller.VerticalAxis)).normalized * speed;
		var delta = targetVelocity - rb2d.velocity;
		rb2d.MovePosition(rb2d.position + delta * Time.deltaTime);
	}

	private void HandleShooting()
	{
        if (shootNextFrame && shootCooldown < 0f)
        {
            FireWeapon();
        }
        shootNextFrame = false;

        if (Input.GetAxis(Controller.WeaponSwitchAxis) > 0)
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

		Vector2 shootDirection = Vector2.right * -Input.GetAxis(Controller.HorizontalAimAxis) + Vector2.up * Input.GetAxis(Controller.VerticalAimAxis);

		if (shootDirection.sqrMagnitude > 0.3f)
		{
			var angle = Vector3.Angle(Vector3.up, -shootDirection) * (Vector3.Cross(Vector3.up, -shootDirection).z < 0 ? -1 : 1);
			rotation = Quaternion.Euler(0, 0, angle);
            shootNextFrame = true;

			//rotation = Quaternion.LookRotation(shootDirection, Vector3.forward);
			//rotation.x = 0;
			//rotation.y = 0;
			//transform.rotation = rot;
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
                MakeBullets(currentWeapon, bulletStart.transform.position, bulletStart.transform.rotation);
				break;
			case WeaponType.Shotgun:
				MakeBullets(currentWeapon, bulletStart.transform.position, bulletStart.transform.rotation);
				break;
			default:
				Debug.LogError("bad weapon type");
				break;
		}

        SoundEffects.Singleton.Play("Gun Shot");

        shootCooldown = currentWeapon.Cooldown;
	}

	private void MakeBullets(WeaponDescription weapon, Vector3 position, Quaternion rotation)
	{
		for (int i = 0; i < weapon.ShellCount; i++)
		{
			var randAngle = Quaternion.AngleAxis((UnityEngine.Random.value - 0.5f) / weapon.Accuracy, Vector3.forward);
			var newBullet = (GameObject)Instantiate(weapon.BulletPrefab, position, rotation * randAngle);
			var bullet = newBullet.GetComponent<BulletController>();
			if (bullet != null)
			{
				bullet.Owner = gameObject;
				bullet.damage = weapon.Damage;
			}
			var rocket = newBullet.GetComponent<RocketController>();
			if (rocket != null)
			{
				rocket.Owner = gameObject;
				rocket.damage = weapon.Damage;
			}
			foreach (var col in gameObject.GetComponentsInChildren<Collider2D>())
			{
				Physics2D.IgnoreCollision(newBullet.GetComponent<Collider2D>(), col);
			}
			var randomSpeedFactor = 1 + ((UnityEngine.Random.value - 0.5f) / 0.5f) * weapon.SpeedJitterPercent;
			newBullet.GetComponent<Rigidbody2D>().AddForce(newBullet.transform.up * weapon.ShellSpeed * randomSpeedFactor, ForceMode2D.Impulse);
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
