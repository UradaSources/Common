using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Gravity : MonoBehaviour
{
	[SerializeField]
	private CircleCollider2D m_trigger;

	[SerializeField]
	private float m_gravityForce;

	[SerializeField]
	private AnimationCurve m_forceCurve;

	[SerializeField]
	private LayerMask m_mask;

	public Vector2 Center
	{
		get => (Vector2)this.transform.position + m_trigger.offset;
	}

	public LayerMask Mask 
	{
		get => m_mask; 
		set => m_mask = value; 
	}

	public AnimationCurve ForceCurve 
	{
		get => m_forceCurve; 
	}

	public float GravityForce 
	{
		get => m_gravityForce; 
		set => m_gravityForce = Mathf.Max(value, 0); 
	}

	public void TryApplyGravity(Rigidbody2D rb)
	{
		if (rb.bodyType == RigidbodyType2D.Dynamic)
		{
			var deltaPos = this.Center - rb.position;

			float dist = deltaPos.magnitude;
			var dir = deltaPos.normalized;

			float rate = 1 - Mathf.Clamp01(dist / m_trigger.radius);

			var force = m_gravityForce * m_forceCurve.Evaluate(rate);
			rb.AddForce(force * dir, ForceMode2D.Force);

#if UNITY_EDITOR
			// 显示范围内所有受影响的对象
			if (showAllAffectedObject)
			{
				DebugUtility.DrawMark(rb.position, new DrawParam(color: Color.blue));
				DebugUtility.DrawArrow(rb.position, force * dir * Time.fixedDeltaTime, new DrawParam(color: Color.gray));
				DebugUtility.DrawArrow(rb.position, rb.velocity * Time.fixedDeltaTime, new DrawParam(color: Color.red));
			}
#endif
		}
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		var rb = c.attachedRigidbody;
		if (rb.gameObject.InLayer(m_mask))
			this.TryApplyGravity(rb);
	}

#if UNITY_EDITOR
	[Header("Editor")]

	[SerializeField]
	private bool showAllAffectedObject = false;

	private void Reset()
	{
		this.gameObject.TryGetComponent(out m_trigger);
		if (m_trigger) m_trigger.isTrigger = true;
		
		m_forceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	}

	private void OnDrawGizmosSelected()
	{
		// 获取触发器右侧的边缘点
		// 在边缘点与中心点之间绘制重力系数曲线
		var sidePoint = this.Center + Vector2.one * m_trigger.radius;

		DebugUtility.DrawRange(this.Center, sidePoint);
		DebugUtility.DrawNormalizedCurveTo((float t) => m_forceCurve.Evaluate(1-t), 24, sidePoint, this.Center);
	}
#endif
}
