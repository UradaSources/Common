using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Placeable : MonoBehaviour
{
	private Vector2Int m_coord;
	private Vector2Int m_size;

	public Vector2Int Coord
	{
		get => m_coord;  
		protected set => m_coord = value;
	}
	public Vector2Int Size
	{
		get => m_size;
		protected set => m_size = value;
	}
}
