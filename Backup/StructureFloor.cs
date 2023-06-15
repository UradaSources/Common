using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class StructureFloor : MonoBehaviour
{
	[SerializeField]
	private BoxCollider2D m_collider;

	[SerializeField]
	private float m_thickness;

	public void UpdateSize(Vector2 cabinSize)
	{
		m_collider.size = new Vector2(cabinSize.x, m_thickness);

		var offset = -cabinSize.y * 0.5f + m_collider.size.y * 0.5f;
		m_collider.offset = new Vector2(0, offset);
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_collider = this.GetComponent<BoxCollider2D>();
	}
#endif
}
