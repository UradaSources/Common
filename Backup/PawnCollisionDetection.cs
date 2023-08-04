using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1), DisallowMultipleComponent]
public class PawnCollisionDetection : MonoBehaviour
{
	public const float MinSpace = 0.001f;

	public const float MaxClimbAngle = 30.0f;

	[SerializeField]
	private Rigidbody2D m_rb;

	[SerializeField]
	private BoxCollider2D m_collider;

	[SerializeField]
	private LayerMask m_layerMask;

	// 储存命中信息的缓冲区
	private List<RaycastHit2D> m_hitButter = new List<RaycastHit2D>();

	public Vector2 Centre
	{
		set => m_rb.position = value - m_collider.offset;
		get => m_rb.position + m_collider.offset;
	}

	public void SnapEdge(Vector2 side, Vector2 pos, float space = MinSpace, bool clampSpeed = true)
	{
		if (side.x != 0)
		{
			var dir = Mathf.Sign(side.x);

			// 使用碰撞盒中心计算对齐后的坐标并应用
			var half = m_collider.size.x * 0.5f + space;
			this.Centre = new Vector2(pos.x - dir * half, this.Centre.y);

			if (clampSpeed && Mathf.Sign(m_rb.velocity.x) == dir)
				m_rb.velocity = new Vector2(0, m_rb.velocity.y);
		}
		if (side.y != 0)
		{
			var dir = Mathf.Sign(side.y);

			// 使用碰撞盒中心计算对齐后的坐标并应用
			var half = m_collider.size.y * 0.5f + space;
			this.Centre = new Vector2(this.Centre.x, pos.y - dir * half);

			if (clampSpeed && Mathf.Sign(m_rb.velocity.y) == dir)
				m_rb.velocity = new Vector2(m_rb.velocity.x, 0);
		}
	}

	private bool CollisionTest(out RaycastHit2D hit, Vector2 side, float delta = MinSpace)
	{
		var angle = Vector2.SignedAngle(side * new Vector2(1, -1), Vector2.left);

		var filter = new ContactFilter2D()
		{
			useLayerMask = true,
			layerMask = m_layerMask,

			useNormalAngle = true,
			minNormalAngle = angle - MaxClimbAngle,
			maxNormalAngle = angle + MaxClimbAngle
		};

		// 清空缓冲区并进行碰撞检测
		m_hitButter.Clear();
		m_collider.Cast(side.normalized, filter, m_hitButter, delta);

		// 进行可能的碰撞过滤
		foreach (var candidateHit in m_hitButter)
		{
			//if (this.Fliter == null || this.Fliter(side, candidateHit))
			hit = candidateHit;
			return true;
		}

		hit = default;
		return false;
	}

	private void FixedUpdate()
	{
		var vel = m_rb.velocity;
		vel += Physics2D.gravity * Time.fixedDeltaTime;
		m_rb.velocity = vel;

		// 计算位置增量并测试碰撞
		var delta = m_rb.velocity * Time.fixedDeltaTime;
		if (!Mathf.Approximately(delta.x, 0))
		{
			var side = new Vector2(Mathf.Sign(delta.x), 0);
			if (this.CollisionTest(out var hit, side, Mathf.Abs(delta.x)))
			{ 
				this.SnapEdge(side, hit.point);

				DebugUtils.DrawMark(hit.point, new DrawParam(color: Color.yellow, duration: 10));

				// do something
			}
		}
		if (!Mathf.Approximately(delta.y, 0))
		{
			var side = new Vector2(0, Mathf.Sign(delta.y));
			if (this.CollisionTest(out var hit, side, Mathf.Abs(delta.y)))
			{
				this.SnapEdge(side, hit.point);

				DebugUtils.DrawMark(hit.point, new DrawParam(color: Color.red, duration: 10));

				// do something
			}
		}
	}

	public void RequireCommponent()
	{
		m_rb = this.GetComponent<Rigidbody2D>();
		m_rb.bodyType = RigidbodyType2D.Kinematic;
		m_rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
		m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;

		m_collider = this.GetComponent<BoxCollider2D>();
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_layerMask = ~0;

		this.RequireCommponent();
	}
#endif
}
