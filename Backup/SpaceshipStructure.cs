using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipStructure : MonoBehaviour
{
	[SerializeField]
	private List<SpaceshipCabin> m_cabins;

	public SpaceshipCabin MainCabin
	{
		get => m_cabins.Count > 0 ? m_cabins[0] : null;
	}
}
