using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using System;

public class Enemy : MonoBehaviour
{
	[Header("Movement")]
    [SerializeField] float MaxSpeed = 10f;
    [SerializeField] Transform Target;

	[Header("Combat")]
	public float health;
	
    [Header("Pathfinding")]
	Rigidbody2D rb;
    Seeker seeker;
    [SerializeField] float nextWaypointDistance = 1;                //The max distance from the AI to a waypoint for it to continue to the next waypoint
    [SerializeField] float targetRepathDistanceTolerance = 1f;      //Recalculate path if target moves more than this from the end of the current path

    [Header("Swarm Behaviour")]
    [SerializeField] float SwarmNeighbourRadius = 3f;
    [SerializeField] float WeightAlignment = 0.5f;
    [SerializeField] float WeightCohesion = 0.25f;
    [SerializeField] float WeightSeparation = 0.25f;

    int currentWaypoint = 0;            //The waypoint we are currently moving towards
    Path currentPath;

    float repathRate = 3f;
    float repathElapsed;
    bool calculatingPath;

    Vector3 moveDir;

	void Start()
    {
		health = 100f;
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
    }

	void Update()
    {
        repathElapsed += Time.deltaTime;

		if (health <= 0)
		{
			Destroy(gameObject);
		}

        //Find target  
        FindNewTarget();

        //Move along path towards target
        UpdateMovement();
    }
       
    void FindNewTarget()
    {
        //Find closest player and target them
        List<PlayerController> possibleTargets = FindObjectsOfType<PlayerController>().ToList();
        float closestDistance = float.MaxValue;
        PlayerController closestPlayer = null;

        foreach (PlayerController player in possibleTargets)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                closestPlayer = player;
            }
        }
        Target = closestPlayer.transform;

        //Calculate distance from last target position        
        float targetMoveDistance = float.MaxValue;

        if (currentPath != null)
        {
            targetMoveDistance = Vector3.Distance(Target.position, currentPath.vectorPath[currentPath.vectorPath.Count - 1]); 
        }
        
        if (!calculatingPath && targetMoveDistance > targetRepathDistanceTolerance)
        {
            print("Recalculatin' path! " + targetMoveDistance);
            seeker.StartPath(transform.position, Target.transform.position, OnPathFound);
            calculatingPath = true;
        }        
    }

    void UpdateMovement()
    {
        //Recalculate move direction (combination of heading towards attack target, and obeying the swarm motion)     
        //moveDir = CalculateSwarmAgentMovementDirection();


        if (currentPath == null || Target == null)
        {
            return;
        }

        //Check if enemy has reached end of it's path
        if (currentWaypoint >= currentPath.vectorPath.Count)
        {
            return;
        }

        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        while (Vector3.Distance(transform.position, currentPath.vectorPath[currentWaypoint]) < nextWaypointDistance)
        {
            currentWaypoint++;

            if (currentWaypoint >= currentPath.vectorPath.Count)
            {
                currentPath = null;
                return;
            }
        }

        //Get direction to the next waypoint and move towards it
        moveDir = (currentPath.vectorPath[currentWaypoint] - transform.position).normalized;      
        rb.MovePosition(transform.position + moveDir * MaxSpeed * Time.deltaTime);        
    }

    void OnPathFound(Path p)
    {
        //This sets the path of the Seeker object when seeker.StartPath is called 
        //(assuming a path is possible between target and seeker)
        if (!p.error)
        {
            currentPath = p;
            currentWaypoint = 0;        //Reset the waypoint counter
        }
        else
        {
            Debug.Log("Enemy had path error: " + p.error);
        }

        calculatingPath = false;
    }

    Vector3 CalculateSwarmAgentMovementDirection()
    {
        Vector3 Alignment = Vector3.zero;
        Vector3 Cohesion = Vector3.zero;
        Vector3 Separation = Vector3.zero;

        List<Enemy> neighbours = Physics.OverlapSphere(transform.position, SwarmNeighbourRadius).Where(n => n.GetComponent<Enemy>() != null).Select(n => n.GetComponent<Enemy>()).ToList();

        if (neighbours.Count() == 0)
        {
            return Vector3.zero;
        }

        //For each neighbouring enemy, calculate swarm behaviour values
        foreach (Enemy enemy in neighbours)
        {
            Alignment += enemy.GetComponent<Rigidbody>().velocity;
            Cohesion += enemy.transform.position;
            Separation += enemy.transform.position - transform.position;
        }

        //Divide the computation vectors by the neighbor count and normalize them
        Alignment = (Alignment / neighbours.Count).normalized * WeightAlignment;
        Cohesion = (Cohesion / neighbours.Count).normalized * WeightCohesion;
        Separation = (Separation / neighbours.Count).normalized * WeightSeparation;

        return Alignment + Cohesion + Separation;
	}

	internal void Hit(float damage)
	{
		health -= damage;
	}
}