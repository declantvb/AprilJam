using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class WeaponDescription
{
	public GameObject BulletPrefab;
	public WeaponType Type;
	public float Damage;
	public int ShellCount;
	public float ShellSpeed;
	public float Cooldown;
	public float Accuracy;
}

public enum WeaponType
{
	Auto,
	Shotgun
}
