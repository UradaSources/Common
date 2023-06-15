using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PawnPhysicsBody))]
[DisallowMultipleComponent, DefaultExecutionOrder(1)]
public class PawnMotion : MonoBehaviour
{
	[SerializeField]
	private PawnPhysicsBody m_body;

	[SerializeField]
	private float m_acc;

	[SerializeField]
	private float m_dec;

	[SerializeField]
	private float m_midairDragFactor = 1.0f;

	[SerializeField]
	private float m_midairSpeedFactor = 1.0f;

	// 缓存的运动参数
	// 将在稍后的fxiedUpdate里应用
	private float m_targetSpeed;

	// 当前的运动方向
	public float Toward
	{
		get => Mathf.Approximately(m_targetSpeed, 0) ?
			0 : Mathf.Sign(m_targetSpeed);
	}

	public void MoveTowards(float speed)
	{
		m_targetSpeed = speed;
	}

	public void Jump(float initSpeed)
	{
		m_body.Velocity = new Vector2(m_body.Velocity.x, initSpeed);
	}
	public void JumpToHeight(float height)
	{
		var speed = Mathf.Sqrt(-2.0f * m_body.Gravity.y * height);
		this.Jump(speed);
	}

	private void FixedUpdate()
	{
		var vel = m_body.Velocity;

		// 方向一致时, 目标速度大于当前速度时为加速
		// 方向不一致时为减速
		var speedDir = Mathf.Approximately(vel.x, 0) ? 0 : Mathf.Sign(vel.x);
		var acc = speedDir == this.Toward && Mathf.Abs(m_targetSpeed - vel.x) > 0 ?
			m_acc : m_dec;

		var targetSpeed = m_targetSpeed;

		if (m_body.CollidedInfo.BottomContact == null)
		{
			targetSpeed *= m_midairSpeedFactor;
			acc *= m_midairDragFactor;
		}

		// 更新速度
		vel.x = Mathf.MoveTowards(vel.x, targetSpeed, acc * Time.fixedDeltaTime);
		m_body.Velocity = vel;
	}

	public void RequireCommponent()
	{
		m_body = this.GetComponent<PawnPhysicsBody>();
	}

#if UNITY_EDITOR
	private void Reset()
	{	
		m_midairDragFactor = 1.0f;
		m_midairSpeedFactor = 1.0f;

		this.RequireCommponent();
	}
#endif
}
