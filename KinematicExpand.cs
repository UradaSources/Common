using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D)), DefaultExecutionOrder(1), DisallowMultipleComponent]
public class KinematicExpand : MonoBehaviour
{
	public struct CollisionInfo
	{
		// 发生碰撞的边
		// 由x代表水平轴, y代表垂直轴
		// 1为正方向, -1为反方向, 0为该轴未发生碰撞
		public float side;

		// 碰撞点
		// 使用之前需要先检查side确认分量是否有效
		public float point;

		// 冲击速度
		// 使用之前需要先检查side确认分量是否有效
		public float velocity;
	}

	public class CollisionEvent : UnityEvent<KinematicExpand, CollisionInfo> { };

	public const float MIN_SPACING = 0.005f;
	public const float MAX_CLIMB_ANGLE = 10.0f;

	public const float MAX_FALL_SPEED = -10.0F;

	// 碰撞事件, 在colliison enter时触发
	public CollisionEvent OnHorizontalCollision = new CollisionEvent();
	public CollisionEvent OnVerticalCollision = new CollisionEvent();

	// 私有字段 ==============================================================

	// 所使用的的碰撞盒与运动学刚体
	[SerializeField]
	private Rigidbody2D m_rb;

	[SerializeField]
	private BoxCollider2D m_box;

	// 当前接触到的碰撞体
	private Collider2D m_left;
	private Collider2D m_right;

	private Collider2D m_top;
	private Collider2D m_ground;

	// 命中信息缓冲区
	private List<RaycastHit2D> m_hitButter = new List<RaycastHit2D>(5);

	// 属性 ==============================================================

	public LayerMask targetMask { set; get; }

	public System.Func<Vector2, RaycastHit2D, bool> collisionFilter { set; get; }

	public Rigidbody2D Rb { get => m_rb; }
	public BoxCollider2D Box { get => m_box; }

	private float m_xContactDirect;
	private float m_yContactDirect;

	private Collider2D m_xContact;
	private Collider2D m_yContact;

	public bool simulated
	{
		set => m_rb.simulated = value;
		get => m_rb.simulated;
	}

	public Vector2 centre
	{
		set => m_rb.position = value - this.offset;
		get => m_rb.position + this.offset;
	}

	public Vector2 velocity
	{
		set => m_rb.velocity = value;
		get => m_rb.velocity;
	}
	public Vector2 position
	{
		set => m_rb.position = value;
		get => m_rb.position;
	}

	public Vector2 size => m_box.size * this.transform.localScale;
	public Vector2 offset => m_box.offset * this.transform.localScale;

	public Collider2D left => m_left;
	public Collider2D right => m_right;

	public Collider2D top => m_top;
	public Collider2D ground => m_ground;

	// 功能函数 ==========================================================

	// 捕获边到特定位置
	public void snapTo(Vector2Int side, Vector2 point)
	{
		if (side.x != 0)
		{
			// 使用碰撞盒中心计算对齐后的坐标并应用
			var half = this.size.x * 0.5f;
			this.centre = new Vector2(point.x - side.x * half, this.centre.y);
		}
		if (side.y != 0)
		{
			// 使用碰撞盒中心计算对齐后的坐标并应用
			var half = this.size.y * 0.5f;
			this.centre = new Vector2(this.centre.x, point.y - side.y * half);
		}
	}

	// 测试在特定位置时边的碰撞情况
	public bool testCollision(Vector2Int side, Vector2 pos, float delta, out RaycastHit2D hit)
	{
		delta = Mathf.Abs(delta);

		// 利用法线角度过滤意外的碰撞(例如水平检查时盒射线触碰到地面)
		// 过滤器使用的法线角度是以Vector2.SignAngle(normal, Vector2.right) 计算出的
		// 由于此处使用side检测方向相反的边, 所以对应side的法线角度由原来的坐标系沿(1, -1)对角线翻转而成
		// up: -90, down: 90, left: 180, right: 0
		var angle = Vector2.SignedAngle(side * new Vector2(1, -1), Vector2.left);

		var filter = new ContactFilter2D()
		{
			layerMask = this.targetMask,
			useLayerMask = true,

			minNormalAngle = angle - MAX_CLIMB_ANGLE,
			maxNormalAngle = angle + MAX_CLIMB_ANGLE,
			useNormalAngle = true
		};

		// 计算投射距离
		// 在delat为0时, 投射从0-min_space向法线方向投射min_space距离, 最终停在-min_space + min_space = 0
		// 在delta非0时, 投射从0-delta向法线方向投射delta*2距离, 最终停在-delta + 2*delta = delta

		// 边上的点
		var castPos = pos + side * (size * 0.5f - Vector2.one * (delta + 0.5f * MIN_SPACING));

		// 计算投射点时考虑投射盒本身的大小

		// 计算投射盒的投射点
		// 为了在不平整的地面上平滑的移动, 将投射点从边上向中心内收MIN_SPACING
		// var castPos = pos + side * (size * 0.5f - Vector2.one * (delta + MIN_SPACING * 0.5f));

		// 计算投射盒的尺寸
		var castSize = this.size;

		if (!Mathf.Approximately(side.x, 0)) castSize.x = MIN_SPACING;
		else castSize.y = MIN_SPACING;

		// 在投射检测盒前先关闭自己的碰撞体避免被检测到
		m_box.enabled = false;

		// 可视化调试
		DebugUtility.DrawMark(castPos, new DrawParam(color: Color.yellow)); // 黄色投射点
		DebugUtility.DrawMark(pos, new DrawParam(size: 1.0f, color: Color.red)); // 红色碰撞体大小

		DebugUtility.DrawBoxCast(castPos, castSize, 0, side, delta * 2);

		// 检测碰撞
		m_hitButter.Clear();
		Physics2D.BoxCast(castPos, castSize, 0, side, filter, m_hitButter, delta * 2);

		m_box.enabled = true; // 在检测完毕后重新打开本体的碰撞器

		// 进行可能的碰撞过滤
		foreach (var candidateHit in m_hitButter)
		{
			// 若过滤器存在且返回false, 则该hit是无效的, 简单的跳过
			if (this.collisionFilter?.Invoke(side, candidateHit) == false) continue;

			hit = candidateHit;
			return true;
		}

		hit = default;
		return false;
	}

	// 预测并处理碰撞
	private void anticipateAndHandleCollisions()
	{
		// 计算位置
		Vector2 delta = this.velocity * Time.fixedDeltaTime;

		Vector2 expectedPos = this.position + delta;

		// 水平轴运动方向
		var xDir = new Vector2Int(MathUtility.SignInt(delta.x), 0);
		float xDelta = Mathf.Abs(delta.x);

		if (xDir.x != 0 && this.testCollision(xDir, expectedPos, xDelta, out var xHit))
		{
			if (xHit.collider)
			{
				m_yContact = xHit.collider;
				m_yContactDirect = xDir.x;

				this.snapTo(xDir, xHit.point);

				// 钳制速度
				if (Mathf.Sign(this.velocity.x) == Mathf.Sign(xDir.x))
					this.velocity = new Vector2(0, this.velocity.y);
			}
			else
			{
				m_yContact = xHit.collider;
				m_yContactDirect = xDir.x;
			}
		}

		// 垂直轴运动方向
		var yDir = new Vector2Int(MathUtility.SignInt(delta.x), 0);
		float yDelta = Mathf.Abs(delta.x);

		if (yDir.y != 0 && this.testCollision(xDir, expectedPos, yDelta, out var yHit))
		{
			if (yHit.collider)
			{
				m_yContact = yHit.collider;
				m_yContactDirect = yDir.y;

				this.snapTo(yDir, yHit.point);

				// 钳制速度
				if (Mathf.Sign(this.velocity.y) == Mathf.Sign(yDir.y))
					this.velocity = new Vector2(this.velocity.x, 0);
			}
			else
			{
				m_yContact = yHit.collider;
				m_yContactDirect = yDir.y;
			}
		}
	}

	private void applyVeclocity()
	{
		// 计算在当前速度下的位置增量
		Vector2 delta = this.velocity * Time.fixedDeltaTime;

		// 预期的位置
		Vector2 expectedPos = this.position + delta;

		float dirDelta;
		RaycastHit2D hit;

		// 顶部碰撞
		dirDelta = MathUtility.Sign(delta.y) > 0 ? delta.y : MIN_SPACING;
		if (this.testCollision(Vector2Int.up, expectedPos, dirDelta, out hit))
		{
			m_top = hit.collider;
			this.snapTo(Vector2Int.up, hit.point);
		}
		else
		{
			m_top = null;
		}

		// 底部碰撞
		dirDelta = MathUtility.Sign(delta.y) < 0 ? delta.y : MIN_SPACING;
		if (this.testCollision(Vector2Int.down, expectedPos, dirDelta, out hit))
		{ 
			m_ground = hit.collider;
			this.snapTo(Vector2Int.down, hit.point);
		}
		else
		{
			m_ground = null;
		}

		// 右侧碰撞
		dirDelta = MathUtility.Sign(delta.x) > 0 ? delta.x : MIN_SPACING;
		if (this.testCollision(Vector2Int.right, expectedPos, dirDelta, out hit))
		{
			m_right = hit.collider;
			this.snapTo(Vector2Int.right, hit.point);
		}
		else
		{
			m_right = null;
		}

		// 左侧碰撞
		dirDelta = MathUtility.Sign(delta.x) < 0 ? delta.x : MIN_SPACING;
		if (this.testCollision(Vector2Int.left, expectedPos, dirDelta, out hit))
		{
			m_left = hit.collider;
			this.snapTo(Vector2Int.left, hit.point);
		}
		else
		{
			m_left = null;
		}
	}

	private void Reset()
	{
		m_rb = this.GetComponent<Rigidbody2D>();
		m_rb.bodyType = RigidbodyType2D.Kinematic;
		m_rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
		m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;

		m_box = this.GetComponent<BoxCollider2D>();
	}

	private void Awake()
	{
		m_rb = GetComponent<Rigidbody2D>();
		m_box = GetComponent<BoxCollider2D>();

		this.targetMask = LayerMask.GetMask("Default");
	}

	private void FixedUpdate()
	{
		if (!this.simulated) return;

		// 预测碰撞并进行处理
		this.applyVeclocity();
	}

	private void OnDrawGizmosSelected()
	{
		if (Application.isPlaying)
		{
			string mesg =
				$"velocity : {this.velocity}\n" +
				$"position : {this.position}\n" +
				$"t : {this.m_top}\n" +
				$"g : {this.m_ground}\n" +
				$"l : {this.m_left}\n"+
				$"r : {this.m_right}\n";
			UnityEditor.Handles.Label(Vector2.up, mesg, UnityEditor.EditorStyles.boldLabel);
		}
	}
}
