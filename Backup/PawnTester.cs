using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnTester : MonoBehaviour
{
	[SerializeField]
	private Rigidbody2D m_rb;

	[SerializeField]
	private float m_speed;

	[SerializeField]
	private float m_jumpSpeed;

	private bool m_jumpKeyDown;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			m_jumpKeyDown = true;
	}

	private void FixedUpdate()
	{
		var dir = Input.GetAxisRaw("Horizontal");
		var target = dir * m_speed;

		var vel = m_rb.velocity;
		vel.x = target;

		if (m_jumpKeyDown)
			vel.y = m_jumpSpeed;

		m_jumpKeyDown = false;

		vel += Physics2D.gravity * Time.fixedDeltaTime;
		m_rb.velocity = vel;
	}
}
