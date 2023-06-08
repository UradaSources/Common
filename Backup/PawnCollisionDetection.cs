using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100), DisallowMultipleComponent]
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

	public void SnapEdgeX(float dir, float pos, float space = MinSpace)
	{
		if (dir != 0)
		{
			// 使用碰撞盒中心计算对齐后的坐标并应用
			var half = m_collider.size.x * 0.5f + space;
			this.Centre = new Vector2(pos - dir * half, this.Centre.y);
		}
	}
	public void SnapEdgeY(float dir, float pos, float space = MinSpace)
	{
		if (dir != 0)
		{
			// 使用碰撞盒中心计算对齐后的坐标并应用
			var half = m_collider.size.y * 0.5f + space;
			this.Centre = new Vector2(this.Centre.x, pos - dir * half);
		}
	}

	private RaycastHit2D CollisionTestX(float dir, float delta = MinSpace)
	{
		var angle = Vector2.SignedAngle(new Vector2(dir, 0), Vector2.left);

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
		m_collider.Cast(new Vector2(dir, 0), filter, m_hitButter, delta);

		// 进行可能的碰撞过滤
		foreach (var candidateHit in m_hitButter)
		{
			//if (this.Fliter == null || this.Fliter(side, candidateHit))
			return candidateHit;
		}
		return default;
	}
	private RaycastHit2D CollisionTestY(float dir, float delta = MinSpace)
	{
		var angle = Vector2.SignedAngle(new Vector2(0, -dir), Vector2.left);

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
		m_collider.Cast(new Vector2(0, dir), filter, m_hitButter, delta);

		// 进行可能的碰撞过滤
		foreach (var candidateHit in m_hitButter)
		{
			//if (this.Fliter == null || this.Fliter(side, candidateHit))
			return candidateHit;
		}
		return default;
	}

	private void HandleCollisionX(float delta)
	{
		if (Mathf.Approximately(delta, 0)) return;

		var dir = MathUtils.SignInt(delta);
		var hit = this.CollisionTestX(dir, Mathf.Abs(delta));
		if (hit.collider)
		{
			// 将边贴合到撞击点
			this.SnapEdgeX(dir, hit.point.x);

			// 钳制速度
			if (MathUtils.SignInt(m_rb.velocity.x) != dir)
				m_rb.velocity = new Vector2(0, m_rb.velocity.y);
		}
	}
	private void HandleCollisionY(float delta)
	{
		if (Mathf.Approximately(delta, 0)) return;

		var dir = MathUtils.SignInt(delta);
		var hit = this.CollisionTestY(dir, Mathf.Abs(delta));
		if (hit.collider)
		{
			// 将边贴合到撞击点
			this.SnapEdgeY(dir, hit.point.y);

			// 钳制速度
			if (MathUtils.SignInt(m_rb.velocity.y) != dir)
				m_rb.velocity = new Vector2(m_rb.velocity.x, 0);
		}
	}

	private void FixedUpdate()
	{
		// 计算位置增量并测试碰撞
		var delta = m_rb.velocity * Time.fixedDeltaTime;
		this.HandleCollisionX(delta.x);
		this.HandleCollisionY(delta.y);
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
