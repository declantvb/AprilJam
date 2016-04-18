using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;

public class GameController : MonoBehaviour
{
	public int StartLives;
	public int Lives;
	public GameState CurrentState;

    public List<PlayerController> ActivePlayers = new List<PlayerController>();
    private float checkTime = 0;

    public int AliensAlive { get; private set; }
    public int AliensKilled { get; private set; }
    public int EggsKilled { get; private set; }
    public int EggsAlive { get; private set; }

    // Use this for initialization
    void Start()
	{
        CheckAliens();
	}

    public void CheckAliens()
    {
        AliensAlive = FindObjectsOfType<Enemy>().Length;
        EggsAlive = FindObjectsOfType<EnemySpawner>().Length;
    }

    public void AlienKilled()
    {
        AliensKilled++;
    }

    public void EggKilled()
    {
        EggsKilled++;
    }

	// Update is called once per frame
	void Update()
	{


        if (checkTime <= 0)
        {
            ActivePlayers = FindObjectsOfType<PlayerController>().ToList();
            checkTime = 2;
        }
        checkTime -= Time.deltaTime;

        if (ActivePlayers.Count == 0 && Lives <= 0) // no players and no lives
		{
			CurrentState = GameState.Menu;
		}
		else
		{
			CurrentState = GameState.Playing;
		}

		switch (CurrentState)
		{
			case GameState.Menu:
				//TODO menu music and options
				if (Input.GetKey(KeyCode.R))
				{
					Lives = StartLives;
				}
				break;
			case GameState.Playing:
				//TODO ?
				break;
			default:
				break;
		}
	}

	void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 200, 50), 
            "Lives: " + Lives + Environment.NewLine +
            "Aliens Alive / Killed: " + AliensAlive + " / " + AliensKilled + Environment.NewLine +
            "Eggs Alive / Killed: " + EggsAlive + " / " + EggsKilled + Environment.NewLine);
    }

	public enum GameState
	{
		Menu,
		Playing
	}
}
