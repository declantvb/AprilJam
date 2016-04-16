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
	public Rect ViewportRect;

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
			var player = inputMaster.NextAvailablePlayer();
			var avatar = player.GetComponentInChildren<PlayerController>();
			avatar.horizontalAxis = HorizontalAxis;
			avatar.verticalAxis = VerticalAxis;
			avatar.horizontal2Axis = HorizontalAimAxis;
			avatar.vertical2Axis = VerticalAimAxis;
			avatar.shootAxis = FireAxis;
			avatar.WeaponSwitchAxis = WeaponSwitchAxis;
			var camera = player.GetComponentInChildren<CameraController>();
			camera.updateCameraViewport(ViewportRect);

			bound = true;
		}
	}
}
