using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	private Transform Player;
	private Transform PlayerAvatar;
	private Controller PlayerController;
	private bool zoomingOut;
	private float zoomOut;
	private float defaultSize;

	private Camera localCamera;

	public float MaxZoomout;
	public float ZoomSpeed;

	JankMode pixelMode;

	// Use this for initialization
	void Start()
	{
		localCamera = GetComponent<Camera>();
		Player = transform.parent;
		PlayerAvatar = Player.GetComponentInChildren<PlayerController>().transform;
		PlayerController = Player.GetComponentInChildren<PlayerController>().Controller;
		defaultSize = localCamera.orthographicSize;
	}

	public void updateCameraViewport(Rect ViewportRect)
	{
		localCamera = GetComponent<Camera>();
		localCamera.rect = ViewportRect;
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.J))
		{
			if (pixelMode == JankMode.Dejank)
				pixelMode = JankMode.FullJank;
			else if (pixelMode == JankMode.FullJank)
				pixelMode = JankMode.Dejank;
		}

		transform.position = PlayerAvatar.transform.position + Vector3.back;

		// Show map by zooming out
		zoomingOut = Input.GetAxis(PlayerController.ShowMapAxis) > 0;

		localCamera.orthographicSize = 5;

		var size = localCamera.ViewportToScreenPoint(Vector3.up + Vector3.right) - localCamera.ViewportToScreenPoint(Vector3.zero);

		var zoomFactor = zoomingOut ? 0.5f : 2;

		var pixelXOffset = size.x % 2 == 0 ? 0 : 0.5f;
		var pixelYOffset = size.y % 2 == 0 ? 0 : 0.5f;
		var snapSize = ((int)size.y / (32f * zoomFactor)) / 2f;
		var snapPosition = new Vector3((int)(transform.position.x * 32) + pixelXOffset, (int)(transform.position.y * 32) + pixelYOffset, transform.position.z * 32) / 32f;

		if (pixelMode == JankMode.Dejank)
		{
			localCamera.orthographicSize = snapSize;
			transform.position = snapPosition;
		}

		Debug.Log(pixelMode + " - " + size + "   - " + localCamera.orthographicSize + " - " + snapSize + " - " + transform.position + " - " + snapPosition);
	}

	enum JankMode
	{
		FullJank,
		Dejank
	}
}
