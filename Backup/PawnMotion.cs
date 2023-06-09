using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnMotion : MonoBehaviour
{
	[SerializeField]
	private Rigidbody2D m_rb;

	// 缓存的运动参数
	// 将在稍后的fxiedUpdate里应用
	private float m_speed;

	private float m_acc;
	private float m_dec;

	// 当前的运动方向
	public float Toward
	{
		get => Mathf.Approximately(m_speed, 0) ? 0 : Mathf.Sign(m_speed);
	}

	public void MoveTowards(float speed, float acc, float dec)
	{
		m_speed = speed;

		m_acc = acc;
		m_dec = dec;
	}

	public void Jump(float initSpeed)
	{
		m_rb.velocity = new Vector2(m_rb.velocity.x, initSpeed);
	}
	public void JumpToHeight(float height)
	{
		var speed = Mathf.Sqrt(-2.0f * Physics2D.gravity.y * height);
		this.Jump(speed);
	}

	private void FixedUpdate()
	{
		var vel = m_rb.velocity;

		// 方向一致时, 目标速度大于当前速度时为加速
		// 方向不一致时为减速
		var speedDir = Mathf.Approximately(vel.x, 0) ? 0 : Mathf.Sign(vel.x);
		var acc = speedDir == this.Toward && Mathf.Abs(m_speed - vel.x) > 0 ?
			m_acc : m_dec;

		// 更新速度
		vel.x = Mathf.MoveTowards(vel.x, m_speed, acc * Time.fixedDeltaTime);
		m_rb.velocity = vel;
	}

	public void RequireCommponent()
	{
		m_rb = this.GetComponent<Rigidbody2D>();
	}

#if UNITY_EDITOR
	private void Reset()
	{
		this.RequireCommponent();
	}
#endif
}
