using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class InputController : MonoBehaviour
{
	private int nextPlayer = 0;
	public GameObject ControllerPrefab;
	public GameObject PlayerPrefab;
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
            controller.ShowMapAxis = BButton + inputIndex;
            controller.ViewportRects = new Rect[] {
                new Rect(0, 0, 1f, 1f),
                new Rect(0.5f * (i / 2), 0.5f * (i % 2), 0.5f, 0.5f),
                new Rect(0.5f * (i / 2), 0.5f * (i % 2), 0.5f, 0.5f),
                new Rect(0.5f * (i / 2), 0.5f * (i % 2), 0.5f, 0.5f), // TODO: Move this to the actual update, so we can lay them out goodly.
            };
        }
	}

	// Update is called once per frame
	void Update()
	{

	}

    public void UpdateAllCameras()
    {
        var players = FindObjectsOfType<PlayerController>().Where(p => p.IsAlive).ToArray();
        var viewportIndex = players.Length - 1;
        if (viewportIndex > 3) viewportIndex = 3;

        foreach (var player in players)
        {
            var camera = player.transform.parent.GetComponentInChildren<CameraController>();

            camera.updateCameraViewport(player.Controller.ViewportRects[viewportIndex]);
        }
    }

	internal Transform NextAvailablePlayer()
	{
		return ((GameObject)Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity)).transform;
	}
}
