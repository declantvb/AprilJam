﻿using UnityEngine;
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
    [SerializeField] int animState = 0;
    [SerializeField] SpriteAnimation Anim_Pounce_Up;
    [SerializeField] SpriteAnimation Anim_Pounce_Down;
    [SerializeField] SpriteAnimation Anim_Pounce_Left;
    [SerializeField] SpriteAnimation Anim_Pounce_Right;

    int currentWaypointIndex = 0;            //The waypoint we are currently moving towards
    Path currentPath;    
    bool calculatingPath;

    Rigidbody2D rb;
    Seeker seeker;
    Vector3 moveDir;
    
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
                DoAttack(closestPlayer);
                attackCooldownElapsed = 0;
            }

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
                
        //Move enemy unless it is stunned
        Vector3 Vf = moveDir * moveSpeed;
        Vector3 Vi = rb.velocity;

        Vector3 F = (rb.mass * (Vf - Vi)) / 0.05f;               //EUREKA!!!
        rb.AddForce(F, ForceMode2D.Force);

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

    void DoAttack(PlayerController playerToAttack)
    {
        playerToAttack.Hit(AttackDamage, (playerToAttack.transform.position - transform.position).normalized, 0f);
    }

    public enum EnemyState
    {
        PatrolWait,
        PatrolMove,
        ChasingTarget,
        Attacking,
        Stunned,
        Dead
    }

    IEnumerator PounceAttack()
    {
        animState = 0;

        //Halt walk animation
        GetComponent<WalkAnimator>().enabled = false;
        
        SpriteAnimation currentPounceAnim;
        if (Mathf.Abs(rb.velocity.y) >= Mathf.Abs(rb.velocity.x))//We are moving up or down
        {
            if (rb.velocity.y >= 0)//We are moving up
            {
                currentPounceAnim = Anim_Pounce_Up;                
            }
            else//We are moving down
            {
                currentPounceAnim = Anim_Pounce_Down;
            }
        }
        else //We are moving left or right
        {
            if (rb.velocity.x >= 0)//We are moving to the right
            {
                currentPounceAnim = Anim_Pounce_Right;
            }
            else//We are moving to the left
            {
                currentPounceAnim = Anim_Pounce_Left;
            }
        }
                
        do
        {/*
            if (animState == 0)
            {
                //Play puru animation
                Anim_Puru.PlayOneShot();
                animState++;
            }
            else if (animState == 1 && !Anim_Puru.IsPlaying)
            {
                //Play crackpuru animation
                Anim_CrackPuru.PlayOneShot();
                animState++;
            }
            else if (animState == 2 && !Anim_CrackPuru.IsPlaying)
            {
                //Play uncrack animation
                Anim_Uncrack.PlayOneShot();
                animState++;

                //Spawn new enemy
                GameObject newEnemy = (GameObject)Instantiate(EnemyPrefab);
                newEnemy.transform.position = transform.position;
                SpawnedEnemies.Add(newEnemy.GetComponent<Enemy>());
            }
            else if (animState == 3 && !Anim_Uncrack.IsPlaying)
            {
                //Return to idle animation
                Anim_Idle.Play(true);
                animState++;
            }*/

            yield return null;
        }
        while (animState != 4);     //Loop until animation sequence is complete    


        //Re-enable walking animation
        GetComponent<WalkAnimator>().enabled = true;
    }    
}