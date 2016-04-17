using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
	public int StartLives;
	public int Lives;
	public GameState CurrentState;

    public List<PlayerController> ActivePlayers = new List<PlayerController>();
    private float checkTime = 0;

	// Use this for initialization
	void Start()
	{

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
		GUI.Box(new Rect(10, 10, 100, 50), "Lives: " + Lives);
	}

	public enum GameState
	{
		Menu,
		Playing
	}
}
