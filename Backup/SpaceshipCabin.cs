using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpaceshipCabin : MonoBehaviour
{
	[SerializeField]
	private SpaceshipStructure m_structure;

	[SerializeField]
	private BoxCollider2D m_floor;

	[SerializeField]
	private Vector2 _size;

	public Vector2 Position
	{
		set
		{
			var z = this.transform.position.z;
			this.transform.position = new Vector3(value.x, value.y, z);
		}
		get => this.transform.position;
	}

	public Vector2 Size
	{
		set
		{
			_size = value;

			var thickness = GlobalSpaceshipData.Main.FloorThickness;
			m_floor.size = new Vector2(_size.x, thickness);

			var offset = -_size.y * 0.5f + m_floor.size.y * 0.5f;
			m_floor.offset = new Vector2(0, offset);
		}
		get => _size;
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_structure = this.GetComponentInParent<SpaceshipStructure>();
		m_floor = this.GetComponent<BoxCollider2D>();
	}

	private void OnValidate()
	{
		this.Size = _size;
	}

	private void OnDrawGizmosSelected()
	{
		DebugUtils.DrawBox(this.Position, this.Size);
	}
#endif
}
