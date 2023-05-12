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

	public bool ApplyGravity(Rigidbody2D rb, float factor = 1.0f)
	{
		if (rb.bodyType != RigidbodyType2D.Dynamic) return false;

		var deltaPos = (Vector2)this.transform.position - rb.position;

		float dist = deltaPos.magnitude;
		var dir = deltaPos.normalized;

		float rate = 1 - Mathf.Clamp01(dist / m_trigger.radius);

		var force = m_gravityForce * m_forceCurve.Evaluate(rate) * factor;
		rb.AddForce(force * dir, ForceMode2D.Force);

#if UNITY_EDITOR
		if (editor_drawGizmos)
		{
			DebugUtility.DrawMark(rb.position, 0.1f, Color.green);
			DebugUtility.DrawArrow(rb.position, force * dir, 0.1f, color: Color.grey);
			DebugUtility.DrawArrow(rb.position, rb.velocity, 0.1f, color: Color.white);
		}
#endif

		return true;
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		var rb = c.attachedRigidbody;
		if (rb.bodyType == RigidbodyType2D.Dynamic && MiscUtils.InLayer(rb.gameObject, m_mask))
		{
			this.ApplyGravity(rb);
		}
	}

#if UNITY_EDITOR

	public bool editor_drawGizmos = false;

	private void Reset()
	{
		this.gameObject.TryGetComponent(out m_trigger);
		if (m_trigger) m_trigger.isTrigger = true;	

		m_forceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	}
#endif
}
