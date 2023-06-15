using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class Cabin : MonoBehaviour
{
	[SerializeField]
	private BoxCollider2D m_floor;

	[SerializeField]
	private Vector2Int m_size;

	public Vector2Int CellSize { get => m_size; }

	public void SetSize(Vector2Int size)
	{
		m_size = size;

		m_floor.size = new Vector2(size.x, 0.5f);
		m_floor.offset = new Vector2(m_size.x * 0.5f, -m_floor.size.y * 0.5f);
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		var pos = this.transform.position;
		DebugUtils.DrawGridLines(pos, Vector2Int.one, this.CellSize, new DrawParam(color: Color.gray));
	}

	private void OnValidate()
	{
		this.SetSize(m_size);
	}
#endif
}
