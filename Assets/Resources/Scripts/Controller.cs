using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{
	public string HorizontalAxis;
	public string VerticalAxis;
	public string HorizontalAimAxis;
	public string VerticalAimAxis;
	public string FireAxis;
	public string WeaponSwitchAxis;

	public bool bound = false;
	public InputController inputMaster;

	// Use this for initialization
	void Start()
	{
		inputMaster = GetComponentInParent<InputController>();
	}

	// Update is called once per frame
	void Update()
	{
		if (!bound && Input.GetAxis(FireAxis) > 0)
		{
			var player = inputMaster.NextAvailablePlayer().GetComponentInChildren<PlayerController>();
			player.horizontalAxis = HorizontalAxis;
			player.verticalAxis = VerticalAxis;
			player.horizontal2Axis = HorizontalAimAxis;
			player.vertical2Axis = VerticalAimAxis;
			player.shootAxis = FireAxis;
			player.WeaponSwitchAxis = WeaponSwitchAxis;

			bound = true;
		}
	}
}
