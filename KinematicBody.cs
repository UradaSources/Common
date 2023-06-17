using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionFliter
{
	public void OnCollisionBegin(Vector2 pos);
	public void OnCollisionEnd();

	public bool Fliter(Vector2Int side, RaycastHit2D hit);
}

[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D)), DefaultExecutionOrder(1)]
public class KinematicBody : MonoBehaviour
{
	public enum Edge
	{ 
		Top		= 0b000,
		Bottom	= 0b001,

		Left	= 0b110,
		Right	= 0b100
	}

	public struct CollidedInfo
	{
		public enum CollisionDirect
		{
			None = 0,
			Positive = 1,
			Negative = -1
		}

		public static implicit operator bool(CollidedInfo info)
			=> info.xDirect != CollisionDirect.None || info.yDirect != CollisionDirect.None;

		public CollisionDirect xDirect;
		public CollisionDirect yDirect;

		public Collider2D xContact;
		public Collider2D yContact;

		public Vector2 velocity;
		public Vector2 position;

		public Collider2D RightContact
		{
			get => xDirect == CollisionDirect.Positive ? 
				xContact : null;
		}
		public Collider2D LeftContact
		{ 
			get => xDirect == CollisionDirect.Negative ? 
				xContact : null; 
		}

		public Collider2D TopContact
		{
			get => yDirect == CollisionDirect.Positive ?
				yContact : null;
		}
		public Collider2D BottomContact
		{
			get => yDirect == CollisionDirect.Negative ? 
				yContact : null;
		}
	}

	public delegate bool CollisionFliter(Vector2Int side, RaycastHit2D hit);

	// 最小间隙
	public const float MinSpace = 0.01f;

	// 最小爬坡角度
	public const float MaxClimbAngle = 10.0f;

	[SerializeField]
	private Rigidbody2D m_rb;

	[SerializeField]
	private BoxCollider2D m_box;

	[SerializeField]
	private float m_gravityScale = 1.0f;

	[SerializeField]
	private LayerMask m_targetMask;

	public Rigidbody2D Rb { get => m_rb; }
	public BoxCollider2D Box { get => m_box; }

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
		set => m_box.size = value / this.transform.localScale;
		get => m_box.size * this.transform.localScale;
	}
	public Vector2 Offset
	{
		set => m_box.offset = value / this.transform.localScale;
		get => m_box.offset * this.transform.localScale;
	}

	public float GravityScale
	{
		set => m_gravityScale = value;
		get => m_gravityScale;
	}

	public LayerMask TargetMask
	{
		set => m_targetMask = value;
		get => m_targetMask;
	}

	// 储存命中信息的缓冲区
	private List<RaycastHit2D> m_hitButter = new List<RaycastHit2D>();

	public void SnapEdge(Vector2 side, Vector2 pos, float space = MinSpace)
	{
		if (side.x != 0)
		{
			// 使用碰撞盒中心计算对齐后的坐标并应用
			var half = this.Size.x * 0.5f + space;
			this.Centre = new Vector2(pos.x - side.x * half, this.Centre.y);
		}
		if (side.y != 0)
		{
			// 使用碰撞盒中心计算对齐后的坐标并应用
			var half = this.Size.y * 0.5f + space;
			this.Centre = new Vector2(this.Centre.x, pos.y - side.y * half);
		}
	}

	private RaycastHit2D CollisionTest(Vector2 side, float delta = MinSpace)
	{
		delta = Mathf.Abs(delta);

		var angle = Vector2.SignedAngle(side * new Vector2(1, -1), Vector2.left);

		var filter = new ContactFilter2D()
		{
			useLayerMask = true,
			layerMask = m_targetMask,

			useNormalAngle = true,
			minNormalAngle = angle - MaxClimbAngle,
			maxNormalAngle = angle + MaxClimbAngle
		};

		// 清空缓冲区并进行碰撞检测
		m_hitButter.Clear();
		m_box.Cast(side, filter, m_hitButter, delta);

		// 进行可能的碰撞过滤
		foreach (var candidateHit in m_hitButter)
		{
			//if (this.Fliter == null || this.Fliter(side, candidateHit))
			return candidateHit;
		}
		return default;
	}

	private void CollisionDetectionAndHandle(Vector2 delta)
	{
		// var info = new CollidedInfo();
		if (!Mathf.Approximately(delta.x, 0))
		{
			var dir = MathUtils.SignInt(delta.x);
			var side = new Vector2(dir, 0);

			var hit = this.CollisionTest(side, Mathf.Abs(delta.x));
			if (hit.collider)
			{
				// 将边贴合到撞击点
				this.SnapEdge(side, hit.point);

				// 钳制速度
				if (MathUtils.SignInt(this.Velocity.x) == side.x)
					this.Velocity = new Vector2(0, this.Velocity.y);

				DebugUtils.DrawArrowBetween(this.Position, hit.point);
			}

			// 更新碰撞信息
		}
		if (!Mathf.Approximately(delta.y, 0))
		{
			var dir = MathUtils.SignInt(delta.y);
			var side = new Vector2(0, dir);

			var hit = this.CollisionTest(side, Mathf.Abs(delta.y));
			if (hit.collider)
			{
				// 将边贴合到撞击点
				this.SnapEdge(side, hit.point);

				// 钳制速度
				if (MathUtils.SignInt(this.Velocity.y) == side.y)
					this.Velocity = new Vector2(this.Velocity.x, 0);

				DebugUtils.DrawArrowBetween(this.Position, hit.point);
			}

			// 更新碰撞信息
		}
	}

	private void Awake()
	{
		m_rb = GetComponent<Rigidbody2D>();
		m_box = GetComponent<BoxCollider2D>();
	}

	private void FixedUpdate()
	{
		if (!this.m_rb.simulated) return;

		// 施加重力
		var gravity = Physics2D.gravity * this.GravityScale;
		this.Velocity += gravity * Time.fixedDeltaTime;

		// 计算位置增量并测试碰撞
		var delta = this.Velocity * Time.fixedDeltaTime;
		this.CollisionDetectionAndHandle(delta);
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_rb = this.GetComponent<Rigidbody2D>();
		m_box = this.GetComponent<BoxCollider2D>();

		m_rb.bodyType = RigidbodyType2D.Kinematic;
		m_rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
		m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
	}

	protected void OnDrawGizmosSelected()
	{
		if (Application.isPlaying)
		{
			string mesg =
				$"velocity : {this.Velocity}\n" +
				$"position : {this.Position}\n";
				//$"top : {this.topContact}\n" +
				//$"bottom : {this.bottomContact}\n" +
				//$"left : {this.leftContact}\n" +
				//$"right : {this.rightContact}\n";
			UnityEditor.Handles.Label(this.Centre, mesg, UnityEditor.EditorStyles.boldLabel);
		}
	}
#endif
}
