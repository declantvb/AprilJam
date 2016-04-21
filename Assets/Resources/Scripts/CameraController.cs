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
		transform.position = PlayerAvatar.transform.position + Vector3.back;

		// Show map by zooming out
		zoomingOut = Input.GetAxis(PlayerController.ShowMapAxis) > 0;

		if (zoomingOut && zoomOut <= MaxZoomout) zoomOut += Time.deltaTime * ZoomSpeed;
		if (!zoomingOut && zoomOut >= 0) zoomOut -= Time.deltaTime * ZoomSpeed;
		if (zoomOut > MaxZoomout) zoomOut = MaxZoomout;
		if (zoomOut < 0) zoomOut = 0;

		localCamera.orthographicSize = defaultSize * (1 + zoomOut);

		var size = localCamera.ViewportToScreenPoint(Vector3.up) - localCamera.ViewportToScreenPoint(Vector3.zero);
		

		var snapSize = size.y / ((int)(size.y / 16) * 16) * localCamera.orthographicSize;
		var snapPosition = new Vector3((int)(transform.position.x * 32), (int)(transform.position.y * 32), transform.position.z * 32) / 32f;

		Debug.Log(size + "   - " + localCamera.orthographicSize + " - " + snapSize + " - " + transform.position + " - " + snapPosition);
		localCamera.orthographicSize = snapSize;
		transform.position = snapPosition;
	}
}
