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
    public string ShowMapAxis;
    public Rect ViewportRect;

	public bool bound = false;
	public InputController inputMaster;
	public GameController gameMaster;

    public Rect[] ViewportRects { get; internal set; }


    // Use this for initialization
    void Start()
	{
		inputMaster = GetComponentInParent<InputController>();
		gameMaster = FindObjectOfType<GameController>();
	}

	// Update is called once per frame
	void Update()
	{
		if (!bound && Input.GetAxis(FireAxis) > 0)
        {
            Spawn();
            bound = true;
        }
    }

    public void Spawn()
    {
        var player = inputMaster.NextAvailablePlayer();
        var avatar = player.GetComponentInChildren<PlayerController>();
        avatar.Controller = this;

        inputMaster.UpdateAllCameras();

        gameMaster.Lives--;        
    }
}
