using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour 
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

	[Header("Testing variables")]
	[Range(0f, 4f)] [SerializeField] float speed = 1.5f;

	float currentAnimationSpeed;
	Sprite[] currentSpriteDirection;
	SpriteRenderer spriteRenderer;
	Rigidbody2D rb2d;
	int currentSpriteIndex;
	float timeSinceLastSpriteChange;
    Vector2 previousPositon;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		rb2d = GetComponent<Rigidbody2D>();
		currentSpriteDirection = walkingUpSprites;
		currentAnimationSpeed = timePerFrame;
		timeSinceLastSpriteChange = Time.time;
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
        Vector2 direction = (Vector2)transform.position - previousPositon;
        
		if (direction == Vector2.zero)
		{
			currentAnimationSpeed = 0f;

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

	void UpdateSpriteIndex()
	{
		if (Time.time >= timeSinceLastSpriteChange + timePerFrame)
		{
			timeSinceLastSpriteChange = Time.time;
			currentSpriteIndex++;
			currentSpriteIndex %= currentSpriteDirection.Length;//Wrap around the animation
			spriteRenderer.sprite = currentSpriteDirection[currentSpriteIndex];
			//Debug.Log(currentSpriteIndex);
		}
	}

	void testMovement()
	{
		Vector2 direction = FindObjectOfType<PlayerController>().transform.position - transform.position;
		direction = direction.normalized;

		rb2d.velocity = direction * speed;
	}

}
