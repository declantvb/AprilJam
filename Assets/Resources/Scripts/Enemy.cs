using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;

public class Enemy : MonoBehaviour
{
	[Header("Movement")]
    [SerializeField] float MaxSpeed = 10f;
    [SerializeField] Vector3 PathFindTargetPos;

    [Header("Combat")]
    public float health = 100f;
	
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
    }

	void Update()
    {
        timeSinceRepath += Time.deltaTime;

		if (health <= 0)
		{
			Destroy(gameObject);
		}

        //Find target  
        FindNewTarget();

        //Move along path towards target
        UpdateMovement();

        lastState = State;
    }
       
    void FindNewTarget()
    {
        //Find closest player within attack range and target them
        List<PlayerController> possibleTargetsWithinRange = FindObjectsOfType<PlayerController>().Where(p => Vector3.Distance(p.transform.position, transform.position) < TargetDetectionRange).ToList();

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
        if (currentPath == null || PathFindTargetPos == null)
        {
            return;
        }

        //Check if enemy has reached end of it's path
        if (currentWaypointIndex >= currentPath.vectorPath.Count)
        {
            return;
        }

        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        while (Vector3.Distance(transform.position, currentPath.vectorPath[currentWaypointIndex]) < nextWaypointDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= currentPath.vectorPath.Count)
            {
                currentPath = null;
                return;
            }
        }

        //Get direction to the next waypoint and move towards it
        moveDir = (currentPath.vectorPath[currentWaypointIndex] - transform.position).normalized;
        rb.MovePosition(transform.position + moveDir * MaxSpeed * Time.deltaTime);        
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
            Debug.Log("Enemy had path error: " + p.error);
        }

        calculatingPath = false;
        timeSinceRepath = 0;
    }
   
	internal void Hit(float damage)
	{
		health -= damage;
	}

    public enum EnemyState
    {
        PatrolWait,
        PatrolMove,
        ChasingTarget,
        Attacking
    }
}