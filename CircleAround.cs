using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CircleAround : MonoBehaviour
{
	[System.Serializable]
	public class SurroundedEvent : UnityEvent<CircleAround> { };

	public SurroundedEvent OnSurrounded = new SurroundedEvent();

	[SerializeField]
	private float m_recordMaxFrameLength;

	[SerializeField]
	private Transform m_target;

	[SerializeField]
	public Transform m_handler;

	[SerializeField]
	private float m_minSurroundRadius = 0;

	private Queue<float> m_recordAngleDelta = new Queue<float>();

	private Vector2? m_previousPos = null;

	private float m_processPer = 0;

	public float ProcessPer { get => m_processPer; }

	// 清理记录的角度差值
	private void ClearRecordAngleDelta()
	{
		m_recordAngleDelta.Clear();
		m_previousPos = null;
	}

	// 记录每帧的角度差值
	private void RecordAngleDelta()
	{
		// 将当前的位置转换为目标空间下的位置
		Vector2 localPos = m_target.InverseTransformDirection(m_handler.position);

		// 若绕圈半径小于最小半径 清空角度值
		if (localPos.magnitude < m_minSurroundRadius)
			this.ClearRecordAngleDelta();
		else 
		{
			if (m_previousPos.HasValue)
			{
				float angleDelta = Vector2.Angle(localPos, m_previousPos.Value);
				m_recordAngleDelta.Enqueue(angleDelta);
			}
			m_previousPos = localPos;
		}
	}

	private void Update()
	{
		m_processPer = 0.0f;

		this.RecordAngleDelta();

		while (m_recordAngleDelta.Count >= m_recordMaxFrameLength)
			m_recordAngleDelta.Dequeue();

		float angle = 0;

		foreach (var d in m_recordAngleDelta)
		{
			angle += d;
			if (angle >= 360.0f)
			{
				m_processPer = 1.0f;

				this.OnSurrounded?.Invoke(this);
				this.ClearRecordAngleDelta();

				return;
			}
		}
		m_processPer = Mathf.Clamp01(Mathf.Abs(angle) / 360.0f);
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		string msg = "";

		float s = 0;
		foreach (var d in m_recordAngleDelta)
		{
			s += d;
			msg += $"{d}\n";
		}
		msg += $"= {s}";

		UnityEditor.Handles.Label(Vector3.zero, msg);
	}
#endif
}
