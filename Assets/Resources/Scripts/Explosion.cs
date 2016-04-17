using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
	[SerializeField]
	public SpriteAnimation Animation;

	// Use this for initialization
	void Start()
	{
		Animation.PlayOneShot();
	}

	// Update is called once per frame
	void Update()
	{
		if (!Animation.IsPlaying)
		{
			Destroy(gameObject);
		}
	}
}
