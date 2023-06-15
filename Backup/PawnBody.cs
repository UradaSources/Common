using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class PawnBody : MonoBehaviour
{
	[SerializeField]
	private Rigidbody2D m_rb;

	[SerializeField]
	private BoxCollider2D m_collider;

	[SerializeField]
	private float m_gravityScale = 1.0f;

	public Vector2 Centre
	{
		set => m_rb.position = value - this.Offset;
		get => m_rb.position + this.Offset;
	}

	public Vector2 Velocity
	{
		set => m_rb.velocity = value;
		get => m_rb.velocity;
	}

	public Vector2 Position
	{
		set => m_rb.position = value;
		get => m_rb.position;
	}

	public Vector2 Size
	{
		set => m_collider.size = value / this.transform.localScale;
		get => m_collider.size * this.transform.localScale;
	}

	public Vector2 Offset
	{
		set => m_collider.offset = value / this.transform.localScale;
		get => m_collider.offset * this.transform.localScale;
	}

	public BoxCollider2D Collider
	{
		get => m_collider;
	}

	private void FixedUpdate()
	{
		if (!this.m_rb.simulated) return;

		// 施加重力
		var gravity = Physics2D.gravity * m_gravityScale;
		this.Velocity += gravity * Time.fixedDeltaTime;
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_rb = this.GetComponent<Rigidbody2D>();
		m_collider = this.GetComponent<BoxCollider2D>();

		m_rb.bodyType = RigidbodyType2D.Kinematic;
		m_rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
		m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
	}
#endif
}
