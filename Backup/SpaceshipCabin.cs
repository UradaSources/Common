using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class SpaceshipCabin : MonoBehaviour
{
	[SerializeField]
	private BoxCollider2D m_trigger;

	[SerializeField]
	private SpaceshipStructure m_structure;

	[SerializeField]
	private StructureFloor m_floor;

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
			m_trigger.size = value;
			m_floor.UpdateSize(m_trigger.size);
		}
		get => m_trigger.size;
	}

#if UNITY_EDITOR
	[Header("Editor only")]

	[SerializeField]
	private Vector2 __size = Vector2.one;

	private void OnValidate()
	{
		if (m_trigger && m_floor)
			this.Size = __size;
	}

	private void Reset()
	{
		m_structure = this.GetComponentInParent<SpaceshipStructure>();
		m_floor = this.GetComponentInChildren<StructureFloor>();
	}

	private void OnDrawGizmosSelected()
	{
		if (m_trigger && m_floor)
			DebugUtils.DrawBox(this.Position, this.Size);
	}
#endif
}
