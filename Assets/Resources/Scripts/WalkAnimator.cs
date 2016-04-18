using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class WalkAnimator : MonoBehaviour 
{
	[Header("Sprites")]
	[SerializeField] Sprite[] walkingUpSprites;
	[SerializeField] Sprite[] walkingDownSprites;
	[SerializeField] Sprite[] walkingLeftSprites;
	[SerializeField] Sprite[] walkingRightSprites;

	[SerializeField] Sprite[] idleUpSprites;
	[SerializeField] Sprite[] idleDownSprites;
	[SerializeField] Sprite[] idleLeftSprites;
	[SerializeField] Sprite[] idleRightSprites;

	[Header("Animation Settings")]
	[Range(0.001f, 1f)] [SerializeField] float timePerFrame;
	[SerializeField] bool animationPlayingForward;
	[SerializeField] bool isPlayer;

	[Header("Testing variables")]
	[Range(0f, 4f)] [SerializeField] float speed = 1.5f;

	enum Direction
	{
		None,
		Up,
		Down,
		Left,
		Right
	}

	float currentAnimationSpeed;
	Sprite[] currentSpriteDirection;
	SpriteRenderer spriteRenderer;
	Rigidbody2D rb2d;
	int currentSpriteIndex;
	float timeSinceLastSpriteChange;
    Vector2 previousPositon;
	GunMover gunMover;
	Direction movemmentDirection;
	Direction gunDirection;

	void Start()
	{
        // Make player face right
        previousPositon = transform.position + Vector3.left;

		spriteRenderer = GetComponent<SpriteRenderer>();
		rb2d = GetComponent<Rigidbody2D>();
		currentSpriteDirection = walkingUpSprites;
		currentAnimationSpeed = timePerFrame;
		timeSinceLastSpriteChange = Time.time;

		if (isPlayer)
		{
			gunMover = GetComponent<GunMover>();
		}
	}

	void Update()
	{
		UpdateSpriteIndex();
		DetectWhichDirectionFacing();
        //testMovement();

        previousPositon = transform.position;
	}

	void DetectWhichDirectionFacing()
	{
		if (isPlayer == false)
		{
			Vector2 direction = (Vector2)transform.position - previousPositon;

			if (direction == Vector2.zero)
			{
				if (currentSpriteDirection == walkingUpSprites)
					currentSpriteDirection = idleUpSprites;
				else if (currentSpriteDirection == walkingDownSprites)
					currentSpriteDirection = idleDownSprites;
				else if (currentSpriteDirection == walkingLeftSprites)
					currentSpriteDirection = idleLeftSprites;
				else if (currentSpriteDirection == walkingRightSprites)
					currentSpriteDirection = idleRightSprites;
			}
			else//We are moving
			{
				currentAnimationSpeed = timePerFrame;//Move the animation along if we are currently not moving the animation along

				if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.x))//We are moving up or down
				{
					if (direction.y >= 0)//We are moving up
					{
						currentSpriteDirection = walkingUpSprites;
					}
					else//We are moving down
					{
						currentSpriteDirection = walkingDownSprites;
					}
				}
				else //We are moving left or right
				{
					if (direction.x >= 0)//We are moving to the right
					{
						currentSpriteDirection = walkingRightSprites;
					}
					else//We are moving to the left
					{
						currentSpriteDirection = walkingLeftSprites;
					}
				}
			}
		}
		else
		{
			//We are the player
			if (isPlayer == false)
			{
				Vector2 direction = (Vector2)transform.position - previousPositon;

				//Get the gun direction
				if (gunMover.CurrentGunSprite == gunMover.UpGun)
					gunDirection = Direction.Up;
				else if (gunMover.CurrentGunSprite == gunMover.DownGun)
					gunDirection = Direction.Down;
				else if (gunMover.CurrentGunSprite == gunMover.LeftGun)
					gunDirection = Direction.Left;
				else if (gunMover.CurrentGunSprite == gunMover.RightGun)
					gunDirection = Direction.Right;

				//Get the movement direction
				if (direction == Vector2.zero)
				{
					movemmentDirection = Direction.None;
				}
				else if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.x))//We are moving up or down
				{
					if (direction.y >= 0)//We are moving up
						movemmentDirection = Direction.Up;
					else//We are moving down
						movemmentDirection = Direction.Down;
				}
				else //We are moving left or right
				{
					if (direction.x >= 0)//We are moving to the right
						movemmentDirection = Direction.Right;
					else//We are moving to the left
						movemmentDirection = Direction.Left;
				}

				//if (gunDirection == movemmentDirection)
				//{
				//	if (movemmentDirection == 
				//}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
			}
		}
	}

	void UpdateSpriteIndex()
	{
		if (Time.time >= timeSinceLastSpriteChange + timePerFrame)
		{
			timeSinceLastSpriteChange = Time.time;

			if (animationPlayingForward)
			{
				currentSpriteIndex++;
				currentSpriteIndex %= currentSpriteDirection.Length;//Wrap around the animation
				spriteRenderer.sprite = currentSpriteDirection[currentSpriteIndex];
			}
			else
			{
				currentSpriteIndex--;
				if (currentSpriteIndex < 0)
					currentSpriteIndex = currentSpriteDirection.Length - 1;//Wrap it around
				spriteRenderer.sprite = currentSpriteDirection[currentSpriteIndex];
			}

		}
	}

	void testMovement()
	{
		Vector2 direction = FindObjectOfType<PlayerController>().transform.position - transform.position;
		direction = direction.normalized;

		rb2d.velocity = direction * speed;
	}

}
