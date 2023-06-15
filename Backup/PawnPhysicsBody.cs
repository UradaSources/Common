using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CollidedInfo
{
	public static implicit operator bool(CollidedInfo info)
		=> info.xDirect != 0 || info.yDirect != 0;

	public float xDirect;
	public float yDirect;

	public Collider2D xContact;
	public Collider2D yContact;

	public Vector2 velocity;
	public Vector2 position;

	public Collider2D RightContact
	{
		get => xDirect == 1 ?
			xContact : null;
	}
	public Collider2D LeftContact
	{
		get => xDirect == -1 ?
			xContact : null;
	}

	public Collider2D TopContact
	{
		get => yDirect == 1 ?
			yContact : null;
	}
	public Collider2D BottomContact
	{
		get => yDirect == -1 ?
			yContact : null;
	}

	public void RecordContactX(float dir, Collider2D contact , Vector2 pos)
	{
		this.xDirect = dir;
		this.xContact = contact ;
		this.position.x = pos.x;
	}
	public void RecordContactY(float dir, Collider2D contact, Vector2 pos)
	{
		this.yDirect = dir;
		this.yContact = contact;
		this.position.y = pos.y;
	}
}

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
[DefaultExecutionOrder(10), DisallowMultipleComponent]
public class PawnPhysicsBody : MonoBehaviour
{
	public const float MinSpace = 0.001f;

	public const float MaxClimbAngle = 30.0f;

	[SerializeField]
	private Rigidbody2D m_rb;

	[SerializeField]
	private BoxCollider2D m_collider;

	[SerializeField]
	private LayerMask m_layerMask;

	[SerializeField]
	private float m_gravityScale = 1.0f;

	// 储存命中信息的缓冲区
	private List<RaycastHit2D> m_hitButter = new List<RaycastHit2D>();

	// 储存的碰撞信息
	private CollidedInfo m_info = new CollidedInfo();

	public Vector2 Gravity
	{
		get => m_gravityScale * Physics2D.gravity;
	}

	public Vector2 Centre
	{
		set => m_rb.position = value - m_collider.offset;
		get => m_rb.position + m_collider.offset;
	}

	public Vector2 Velocity
	{
		set => m_rb.velocity = value;
		get => m_rb.velocity;
	}

	public CollidedInfo CollidedInfo { get => m_info; }

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
		if (!m_rb.simulated) return;

		var vel = m_rb.velocity;
		vel += this.Gravity * Time.fixedDeltaTime;
		m_rb.velocity = vel;

		// 重置并准备更新碰撞信息
		m_info = default;
		m_info.velocity = m_rb.velocity;

		// 计算位置增量并测试碰撞
		var delta = m_rb.velocity * Time.fixedDeltaTime;
		if (!Mathf.Approximately(delta.x, 0))
		{
			var dir = Mathf.Sign(delta.x);
			var side = new Vector2(dir, 0);
			if (this.CollisionTest(out var hit, side, Mathf.Abs(delta.x)))
			{ 
				this.SnapEdge(side, hit.point);
				m_info.RecordContactX(dir, hit.collider, hit.point);
				// DebugUtils.DrawMark(hit.point, new DrawParam(color: Color.yellow, duration: 10));
			}
		}
		if (!Mathf.Approximately(delta.y, 0))
		{
			var dir = Mathf.Sign(delta.y);
			var side = new Vector2(0, dir);
			if (this.CollisionTest(out var hit, side, Mathf.Abs(delta.y)))
			{
				this.SnapEdge(side, hit.point);
				m_info.RecordContactY(dir, hit.collider, hit.point);
				// DebugUtils.DrawMark(hit.point, new DrawParam(color: Color.red, duration: 10));
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
		m_gravityScale = 1.0f;

		this.RequireCommponent();
	}
#endif
}
