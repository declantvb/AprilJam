using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;

public class Enemy : MonoBehaviour
{
	[Header("Movement")]
    [SerializeField] float MoveSpeed_Chasing = 15f;   
    [SerializeField] float MoveSpeed_Patrol = 10f;
    [SerializeField] Vector3 PathFindTargetPos;

    [Header("Combat")]
    [SerializeField] float health = 100f;
    [SerializeField] float AttackRange = 1f;                        //If a player is within this range, they will be attacked
    [SerializeField] float AttackCooldown = 1f;
    [SerializeField] float AttackDamage = 10f;
    float attackCooldownElapsed;
	[SerializeField] GameObject DeadPrefab;
	
    [Header("Pathfinding")]
    [SerializeField] float nextWaypointDistance = 1;                //The max distance from the AI to a waypoint for it to continue to the next waypoint
    [SerializeField] float targetRepathDistanceTolerance = 1f;      //Recalculate path if target moves more than this from the end of the current path
    [SerializeField] float MinTimeBetweenRepath = 0.2f;  
    float timeSinceRepath;
    [SerializeField] float TargetDetectionRange = 5f;               //If a player enters this radius, enemy will hunt them forever

    [Header("Patrolling")]
    [SerializeField] float PatrolWaitMin = 1f;                      //How long to wait when reached patrol target before heading for new random position
    [SerializeField] float PatrolWaitMax = 4f;
    [SerializeField] float NextPatrolTargetMaxDistance = 4f;        //Max distance allowed when randomly choosing a new patrol target
    [SerializeField] float MaxTimePatrollingOnePath = 5f;           //If enemy spends longer than this patrolling current path without reaching end, generate a new patrol target.  

    [SerializeField] float patrolWaitElapsed;
    [SerializeField] float patrolTravelElapsed;
    [SerializeField] float nextPatrolWaitTime;
    [SerializeField] float distanceToPatrolTarget;

    [Header("Misc")]
    public EnemyState State = EnemyState.PatrolWait;
    EnemyState lastState = EnemyState.PatrolWait;
    [SerializeField] SpriteRenderer Sprite;
    [SerializeField] float DamageRedFlashDuration = 0.2f;
    [SerializeField] float DamageStunTime = 0.5f;
    float stuntimeElapsed;
    [SerializeField] ParticleSystem BloodParticles;
    [SerializeField] SpriteAnimation Anim_Death;

    [Header("Pounce Attack")]
    [SerializeField] SpriteAnimation[] Anim_Pounce_Up;
    [SerializeField] SpriteAnimation[] Anim_Pounce_Down;
    [SerializeField] SpriteAnimation[] Anim_Pounce_Left;
    [SerializeField] SpriteAnimation[] Anim_Pounce_Right;
    [SerializeField] float PounceForce = 200f;
    [SerializeField] float PounceDelay = 0.5f;
    [SerializeField] float PounceDuration = 0.5f;
    [SerializeField] float MidairDrag = 0.1f;
    [SerializeField] float PounceCooldown = 0.8f;

    int currentWaypointIndex = 0;            //The waypoint we are currently moving towards
    Path currentPath;    
    bool calculatingPath;

    Rigidbody2D rb;
    Seeker seeker;
    Vector3 moveDir;

    bool killer = false;
    
	void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        Sprite = GetComponent<SpriteRenderer>();
    }

	void Update()
    {
        timeSinceRepath += Time.deltaTime;

        if (State != EnemyState.Dead)
        {
            //Check if dead
            if (health <= 0)
            {
                State = EnemyState.Dead;

				//Remove components
				DestroyImmediate(GetComponent<WalkAnimator>());
                Destroy(seeker);
                Destroy(rb);
                Destroy(GetComponent<CircleCollider2D>());

                Anim_Death.PlayOneShot();
            }
            else
            {
                //Find target  
                UpdateState();

                //Move along path towards target
                UpdateMovement();
            }
        }
		else if (!Anim_Death.IsPlaying)
		{
			Instantiate(DeadPrefab, transform.position, transform.rotation);
			Destroy(gameObject);
		}

        lastState = State;        
    }
           
    void UpdateState()
    {      
        //Move enemy unless it is stunned
        if (State == EnemyState.Stunned)
        {
            stuntimeElapsed += Time.deltaTime;
            if (stuntimeElapsed >= DamageStunTime)
            {
                stuntimeElapsed = 0;
                State = EnemyState.PatrolWait;           //Return to patrolling after being stunned
            }
            else
            {
                return;
            }
        }

        //Find closest player within attack range and target them
        List<PlayerController> possibleTargetsWithinRange = FindObjectsOfType<PlayerController>().Where(p => p.health > 0).Where(p => Vector3.Distance(p.transform.position, transform.position) < TargetDetectionRange).ToList();

        if (possibleTargetsWithinRange.Count > 0)
        {
            float closestDistance = float.MaxValue;
            PlayerController closestPlayer = null;

            foreach (PlayerController player in possibleTargetsWithinRange)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distanceToPlayer < closestDistance)
                {
                    closestDistance = distanceToPlayer;
                    closestPlayer = player;
                }
            }
            PathFindTargetPos = closestPlayer.transform.position;

            //If a player is within range of being attacked, attack them
            attackCooldownElapsed += Time.deltaTime;
            if (attackCooldownElapsed >= AttackCooldown && closestDistance < AttackRange)
            {
                attackCooldownElapsed = 0;
                if (State != EnemyState.Pouncing)
                {
                    StartCoroutine(PounceAttack());
                }           
            }

            if (State != EnemyState.Pouncing)
            {
                //Calculate distance from last target position        
                float targetMoveDistance = float.MaxValue;

                if (currentPath != null)
                {
                    targetMoveDistance = Vector3.Distance(PathFindTargetPos, currentPath.vectorPath[currentPath.vectorPath.Count - 1]);
                }

                if (!calculatingPath && targetMoveDistance > targetRepathDistanceTolerance && timeSinceRepath >= MinTimeBetweenRepath)
                {
                    seeker.StartPath(transform.position, PathFindTargetPos, OnPathFound);
                    calculatingPath = true;
                }

                State = EnemyState.ChasingTarget;
            }
        }
        else
        {
            //If enemy was previously chasing a target player, but has lost sight of them, return to patrolling
            if (lastState == EnemyState.ChasingTarget)
            {
                State = EnemyState.PatrolWait;

                //Generate new time to wait
                nextPatrolWaitTime = Random.Range(PatrolWaitMin, PatrolWaitMax);
                patrolWaitElapsed = 0;
            }
        }

        //There were no players found within attack range. Continue patrolling
        if (State == EnemyState.PatrolWait)
        {
            //Randomly generate new position to patrol to when enough waiting time has elapsed
            patrolWaitElapsed += Time.deltaTime;
            if (patrolWaitElapsed >= nextPatrolWaitTime)
            {
                patrolWaitElapsed = 0;

                //Enemy has finished waiting. Begin moving
                State = EnemyState.PatrolMove;

                //Calculate next patrol target
                PathFindTargetPos = transform.position + new Vector3(Random.Range(-NextPatrolTargetMaxDistance, NextPatrolTargetMaxDistance), Random.Range(-NextPatrolTargetMaxDistance, NextPatrolTargetMaxDistance), 0);

                //Set off on new path
                seeker.StartPath(transform.position, PathFindTargetPos, OnPathFound);
                calculatingPath = true;
            }
        }
        if (State == EnemyState.PatrolMove)
        {
            patrolTravelElapsed += Time.deltaTime;

            //If enemy has been patrolling this path for too long, or has reached end of patrol path, enter PatrolWait state
            distanceToPatrolTarget = Vector3.Distance(transform.position, PathFindTargetPos);
            if (patrolTravelElapsed >= MaxTimePatrollingOnePath || distanceToPatrolTarget < 0.3f)           //Hardcoded max distance!!!!!
            {
                patrolTravelElapsed = 0;

                //Generate new time to wait
                nextPatrolWaitTime = Random.Range(PatrolWaitMin, PatrolWaitMax);

                State = EnemyState.PatrolWait;
            }
        }
    }

    void UpdateMovement()
    {
        //Check if enemy has reached end of it's path (or lost a path)
        if (currentPath == null || PathFindTargetPos == null || currentWaypointIndex >= currentPath.vectorPath.Count)
        {
            moveDir = Vector2.zero;
        }
        else
        {
            //Get direction to the next waypoint and move towards it
            moveDir = (currentPath.vectorPath[currentWaypointIndex] - transform.position).normalized;
        }

        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        if (currentPath != null)
        {
            while (Vector3.Distance(transform.position, currentPath.vectorPath[currentWaypointIndex]) < nextWaypointDistance)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= currentPath.vectorPath.Count)
                {
                    currentPath = null;
                    return;
                }
            }
        }

        float moveSpeed = 0;
        if (State == EnemyState.PatrolMove)
        {
            moveSpeed = MoveSpeed_Patrol;
        }
        else if (State == EnemyState.ChasingTarget)
        {
            moveSpeed = MoveSpeed_Chasing;
        }

        if (State != EnemyState.Pouncing)
        {

            //Move enemy unless it is stunned
            Vector3 Vf = moveDir * moveSpeed;
            Vector3 Vi = rb.velocity;

            Vector3 F = (rb.mass * (Vf - Vi)) / 0.05f;               //EUREKA!!!
            rb.AddForce(F, ForceMode2D.Force);
        }

        //rb.MovePosition(transform.position + moveDir * moveSpeed * Time.deltaTime);
    }

    void OnPathFound(Path p)
    {
        //This sets the path of the Seeker object when seeker.StartPath is called 
        //(assuming a path is possible between target and seeker)
        if (!p.error)
        {
            currentPath = p;
            currentWaypointIndex = 0;        //Reset the waypoint counter
        }
        else
        {
            //Debug.Log("Enemy had path error: " + p.error);
        }

        calculatingPath = false;
        timeSinceRepath = 0;
    }
   
	internal void Hit(float damage, Vector3 hitDirection, float hitForce)
	{
		health -= damage;
        StartCoroutine(FlashRed(DamageRedFlashDuration));

        //Stun this enemy temporarily. This will allow knockback
        State = EnemyState.Stunned;

        //Apply knockback
        if (rb != null)
        {
            rb.AddForce(hitForce * hitDirection, ForceMode2D.Force);
        }

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
                Sprite.color = Color.Lerp(startColor, endColor, t * 2f);
            }
            else
            {
                Sprite.color = Color.Lerp(endColor, startColor, t * 2f);
            }

            yield return null;
        }
        while (t < 1f);

        Sprite.color = startColor;
    }    
    
    public enum EnemyState
    {
        PatrolWait,
        PatrolMove,
        ChasingTarget,
        Pouncing,
        Stunned,
        Dead
    }

    IEnumerator PounceAttack()
    {
        State = EnemyState.Pouncing;

        //Stop
        rb.velocity = Vector2.zero;

        //Halt walk animation
        GetComponent<WalkAnimator>().enabled = false;

        Vector2 pounceDir = (PathFindTargetPos - transform.position).normalized;

        SpriteAnimation currentPounceAnim;
        currentPounceAnim = SetPounceAnimation(pounceDir, 0);

        //Start pounce anim
        currentPounceAnim.PlayOneShot();

        float elapsed = 0;

        //Delay before pounce force
        do
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        while (elapsed < PounceDelay);

        // Update in case player has moved
        pounceDir = (PathFindTargetPos - transform.position).normalized;

        // Update animation
        currentPounceAnim.Stop();
        currentPounceAnim = SetPounceAnimation(pounceDir, 1);
        currentPounceAnim.Play();

        // Kill on touch
        killer = true;

        //Add force in direction of pounce
        rb.AddForce(pounceDir * PounceForce, ForceMode2D.Impulse);

        /////////////////////////////////
        //GetComponent<SpriteRenderer>().color = Color.blue;

        //Reduce drag while enemy is in air
        float normalDrag = rb.drag;
        rb.drag = MidairDrag;

        //Delay for pounce length
        yield return new WaitForSeconds(PounceDuration); // TODO: exit prematurely if we hit something

        /////////////////////////////////
       // GetComponent<SpriteRenderer>().color = Color.black;

        //Stop
        rb.velocity = Vector2.zero;

        //Play landing animation
        currentPounceAnim.Stop();
        currentPounceAnim = SetPounceAnimation(pounceDir, 2);
        currentPounceAnim.PlayOneShot();


        // 'es 'armless
        killer = false;

        //Wait until landing is finished
        while (currentPounceAnim.IsPlaying) yield return null;

        /////////////////////////////////
        //GetComponent<SpriteRenderer>().color = Color.white;

        //Return drag to normal when anim is finished
        rb.drag = normalDrag;

        //Re-enable walking animation
        GetComponent<WalkAnimator>().enabled = true;

        //Return to patrol state
        State = EnemyState.PatrolWait;
    }

    private SpriteAnimation SetPounceAnimation(Vector2 pounceDir, int animationPhase)
    {
        SpriteAnimation currentPounceAnim;
        if (Mathf.Abs(pounceDir.y) >= Mathf.Abs(pounceDir.x))//We are moving up or down
        {
            if (rb.velocity.y >= 0)//We are moving up
            {
                currentPounceAnim = Anim_Pounce_Up[animationPhase];
            }
            else//We are moving down
            {
                currentPounceAnim = Anim_Pounce_Down[animationPhase];
            }
        }
        else //We are moving left or right
        {
            if (pounceDir.x >= 0)//We are moving to the right
            {
                currentPounceAnim = Anim_Pounce_Right[animationPhase];
            }
            else//We are moving to the left
            {
                currentPounceAnim = Anim_Pounce_Left[animationPhase];
            }
        }

        return currentPounceAnim;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        PlayerController hitPlayer = col.collider.GetComponentInParent<PlayerController>();
        if (hitPlayer != null && killer)
        {
            //Enemy has hit a player while pouncing. Inflict damage
            hitPlayer.Hit(AttackDamage, (hitPlayer.transform.position - transform.position).normalized, 0f);
        }
    }
}