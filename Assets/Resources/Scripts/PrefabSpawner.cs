using UnityEngine;
using System.Collections;

public class PrefabSpawner : MonoBehaviour
{
	[SerializeField] GameObject Prefab;

	// Use this for initialization
	void Start()
	{
		Instantiate(Prefab, transform.position, transform.rotation);
	}
}
