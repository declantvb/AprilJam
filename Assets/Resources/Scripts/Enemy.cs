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

	Rigidbody2D rb;
    Seeker seeker;
    float nextWaypointDistance = 3;     //The max distance from the AI to a waypoint for it to continue to the next waypoint
    int currentWaypoint = 0;            //The waypoint we are currently moving towards
    Path currentPath;

	void Start()
    {
		health = 100f;
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
    }

	void Update()
    {
		if (health <= 0)
		{
			Destroy(gameObject);
		}

        //Find target  
        if (Target == null)
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

            //Begin pathfinding to new target
            seeker.StartPath(transform.position, Target.transform.position, OnPathComplete);
        }

        //Move along path towards target
        else
        {
            if (currentPath == null)
            {
                return;
            }

            //Get direction to target
            //Vector3 targetDir = (Target.position - transform.position).normalized;
            // rb.MovePosition(transform.position + targetDir * MaxSpeed * Time.deltaTime);
        }
    }

    //This sets the path of the Seeker object when seeker.StartPath is called       (assuming a path is possible between target and seeker)
    void OnPathComplete(Path p)
    {        
        if (!p.error)
        {
            currentPath = p;           
            currentWaypoint = 0;        //Reset the waypoint counter
        }
        else
        {
            Debug.Log("Enemy had path error: " + p.error);
        }
	}

	internal void Hit(float damage)
	{
		health -= damage;
	}
}