using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSpaceshipData : Singleton<GlobalSpaceshipData>
{
	public static GlobalSpaceshipData Main => Instance;

	[SerializeField]
	private float m_floorThickness;

	public float FloorThickness { get => m_floorThickness; }
}
