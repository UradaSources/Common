using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnTester : MonoBehaviour
{
	[SerializeField]
	private Rigidbody2D m_rb;

	[SerializeField]
	private PawnMotion m_motion;

	[SerializeField]
	private float m_speed;

	[SerializeField]
	private float m_jumpHeight;

	private void Update()
	{
		var dir = Input.GetAxisRaw("Horizontal");
		var targetSpeed = dir * m_speed;

		m_motion.MoveTowards(targetSpeed);

		if (Input.GetKeyDown(KeyCode.Space))
			m_motion.JumpToHeight(m_jumpHeight);
	}
}
