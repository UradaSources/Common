using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CircularRangeLine : MonoBehaviour
{
	const float MinRadius = 0.05f;

	[SerializeField]
	private LineRenderer m_renderer;

	[Header("Data sources")]
	[Tooltip("use the collider first, use the radius value when the collider is empty")]
	[SerializeField] private CircleCollider2D m_circleCollider;
	[SerializeField] private float m_defaultRadius;

	[Header("Transition speed")]
	[Tooltip("complete immediately when less than or equal to 0")]
	[SerializeField] private float m_transitionSpeed = 1;

	//[Header("Gfx scale factor")]
	//[Tooltip("it is valid when it is greater than 0, linearly scales the thickness of the material and line, and the final value is t=basic*factor*r")]
	//[SerializeField] private Vector2 m_materialTilingBasic = Vector2.zero;
	//[SerializeField] private float m_materialTilingScaleFactor = 1;

	//[SerializeField] private float m_lineWidthScaleFactor = 1;

	[SerializeField, HideInInspector]
	private float m_generatedRadius;

	public float TargetRadius
	{
		get
		{
			float r = m_circleCollider ? m_circleCollider.radius : m_defaultRadius;
			return Mathf.Max(r, MinRadius);
		}
	}

	public CircleCollider2D CircleCollider
	{
		set => m_circleCollider = value;
		get => m_circleCollider;
	}
	public float DefaultRadius
	{
		set => m_defaultRadius = Mathf.Max(value, 0);
		get => m_defaultRadius;
	}

	public float TransitionSpeed { get => m_transitionSpeed; set => m_transitionSpeed = value; }

	//private void UpdateMaterialAndWidth()
	//{
	//	if (m_materialTilingScaleFactor > 0)
	//	{
	//		var r = this.TargetRadius;

	//		var basic = m_materialTilingBasic;
	//		var factor = m_materialTilingScaleFactor;

	//		var m = m_renderer.material;
	//		m.mainTextureScale = factor * r * basic;
	//	}
	//	if (m_lineWidthScaleFactor > 0)
	//	{
	//		var r = this.TargetRadius;
	//		var factor = m_materialTilingScaleFactor;

	//		m_renderer.widthMultiplier = r * factor;
	//	}
	//}

	private void LateUpdate()
	{
		if (!Mathf.Approximately(m_generatedRadius, this.TargetRadius))
		{
			Vector2 pos = Vector2.zero;
			if (m_circleCollider)
				pos = m_circleCollider.offset;

			float radius;
			if (m_transitionSpeed > 0)
				radius = Mathf.MoveTowards(m_generatedRadius, this.TargetRadius, m_transitionSpeed * Time.deltaTime);
			else
				radius = this.TargetRadius;

			if (radius > MinRadius)
			{
				MiscUtils.GenCircleLine(m_renderer, radius, sample: 64, centerOffset: pos);
				m_generatedRadius = radius;
			}
			else
			{
				Debug.Log("clear!");

				m_renderer.positionCount = 0;
				m_renderer.SetPositions(new Vector3[0]);

				m_generatedRadius = MinRadius;
			}

			//this.UpdateMaterialAndWidth();
		}
	}

#if UNITY_EDITOR
	//[ContextMenu("PreGenerate")]
	//private void PreGenerate()
	//{
	//	Vector2 pos = Vector2.zero;
	//	if (m_circleCollider)
	//		pos = m_circleCollider.offset;

	//	float radius = this.TargetRadius;
	//	MiscUtils.GenCircleLine(m_renderer, radius, sample: 64, centerOffset: pos);
	//	m_generatedRadius = radius;
	//}

	//[ContextMenu("SetMaterialAndWidthBasic")]
	//private void SetMaterialBasic()
	//{
	//	m_materialTilingBasic = m_renderer.material.mainTextureScale;
	//}

	//private void OnDrawGizmosSelected()
	//{
	//	// 只执行于编辑器模式下
	//	if (Application.isPlaying == false && m_renderer)
	//		this.PreGenerate();
	//}

	private void Reset()
	{
		if (!m_renderer)
		{ 
			MiscUtils.RequiredComponent(this, out m_renderer);

			m_renderer.loop = true;
			m_renderer.useWorldSpace = false;
		}
	}
#endif
}
