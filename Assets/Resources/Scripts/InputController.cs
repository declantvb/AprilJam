using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class InputController : MonoBehaviour
{
	private int nextPlayer = 0;
	public GameObject ControllerPrefab;
	public Transform[] players;
	public Controller[] controllers;

	public string L_XAxis = "L_XAxis_";
	public string L_YAxis = "L_YAxis_";
	public string R_XAxis = "R_XAxis_";
	public string R_YAxis = "R_YAxis_";
	public string AButton = "A_";
	public string BButton = "B_";
	public string XButton = "X_";
	public string YButton = "Y_";
	public string LBButton = "LB_";
	public string RBButton = "RB_";

	// Use this for initialization
	void Start()
	{
		players = FindObjectsOfType<PlayerController>().Select(p=>p.transform.parent).OrderBy(x=>x.name).ToArray();
		controllers = new Controller[4];
		for (int i = 0; i < 4; i++)
		{
			var inputIndex = i + 1;
			var newController = Instantiate(ControllerPrefab);
			newController.transform.parent = transform;
			var controller = newController.GetComponent<Controller>();
			controller.HorizontalAxis = L_XAxis + inputIndex;
			controller.VerticalAxis = L_YAxis + inputIndex;
			controller.HorizontalAimAxis = R_XAxis + inputIndex;
			controller.VerticalAimAxis = R_YAxis + inputIndex;
			controller.FireAxis = RBButton + inputIndex;
			controller.WeaponSwitchAxis = AButton + inputIndex;
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

	internal Transform NextAvailablePlayer()
	{
		return players[nextPlayer++];
	}
}
