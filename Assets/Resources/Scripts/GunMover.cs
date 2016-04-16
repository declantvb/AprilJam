using UnityEngine;
using System.Collections;

public class GunMover : MonoBehaviour 
{
	[Header("Sprite offsets")]
	[SerializeField] Vector2 offsetForGunForDownSprite;
	[SerializeField] Vector2 offsetForGunForUpSprite;
	[SerializeField] Vector2 offsetForGunForLeftSprite;
	[SerializeField] Vector2 offsetForGunForRightSprite;

	[Header("Sprites")]
	public Sprite UpGun;
	public Sprite RightGun;
	public Sprite DownGun;
	public Sprite LeftGun;

	[SerializeField] Transform gunTransform;
	[SerializeField] SpriteRenderer gunSpriterenderer;

	public Sprite CurrentGunSprite;
	Vector2 mousePosition;

	void Start()
	{
		CurrentGunSprite = RightGun;
	}

	void Update()
	{
		mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 direction = mousePosition - (Vector2)transform.position;
		direction = direction.normalized;

		UpdateGunRotation(direction);
		UpdateSprite(direction);
	}

	void UpdateGunRotation(Vector2 direction)
	{
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		gunTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	void UpdateSprite(Vector2 direction)
	{
		if (direction == Vector2.zero)
		{
			//Special case
		}
		else//We are moving
		{
			if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.x))//We are moving up or down
			{
				if (direction.y >= 0)//We are moving up
				{
					CurrentGunSprite = UpGun;
				}
				else//We are moving down
				{
					CurrentGunSprite = DownGun;
				}
			}
			else //We are moving left or right
			{
				if (direction.x >= 0)//We are moving to the right
				{
					CurrentGunSprite = RightGun;
				}
				else//We are moving to the left
				{
					CurrentGunSprite = LeftGun;
				}
			}

			gunSpriterenderer.sprite = CurrentGunSprite;
		}
	}
}
