using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	private Transform Player;
	private Transform PlayerAvatar;

	// Use this for initialization
	void Start()
	{
		Player = transform.parent;
		PlayerAvatar = Player.GetComponentInChildren<PlayerController>().transform;
	}

	// Update is called once per frame
	void Update()
	{
		transform.position = PlayerAvatar.transform.position + Vector3.back;
	}
}
