using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CircleAroundTrigger : MonoBehaviour
{
	[System.Serializable]
	public class TriggerEvent : UnityEvent<CircleAroundTrigger> { };

	public TriggerEvent OnTrigger = new TriggerEvent();

	[SerializeField]
	private float m_recordMaxTime;

	[SerializeField]
	private Transform m_target;

	[SerializeField]
	public Transform m_handler;

	[SerializeField]
	private float m_minSurroundRadius = 0;

	[SerializeField, Range(1, 1000)]
	private float m_triggerThreshold = 360;

	[SerializeField]
	private bool m_clearRecordAngleDeltaOnEnable = true;

	private Queue<float> m_recordAngleDelta = new Queue<float>();

	private Vector2? m_previousPos = null;

	private float m_processPer = 0;

	public float ProcessPer { get => m_processPer; }

	public float TriggerThreshold
	{
		get => m_triggerThreshold;
		set => m_triggerThreshold = Mathf.Max(value, 1);
	}

	// 清理记录的角度差值
	public void ClearRecordAngleDelta()
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

	private void OnEnable()
	{
		if (m_clearRecordAngleDeltaOnEnable)
			this.ClearRecordAngleDelta();
	}

	private void Update()
	{
		m_processPer = 0.0f;

		// 记录当前帧与前一帧的位置差值
		this.RecordAngleDelta();

		// 计算需要记录的最大帧数
		// 这样即时计算帧数可能会有bug
		float maxFrameLength = (1.0f / Time.deltaTime) * m_recordMaxTime;

		// 剔除时间范围外多余的数据
		while (m_recordAngleDelta.Count > maxFrameLength)
			m_recordAngleDelta.Dequeue();

		// 累加有效差值并检查是否大于阈值
		float angle = 0;

		foreach (var d in m_recordAngleDelta)
		{
			angle += d;
			if (angle >= m_triggerThreshold)
			{
				m_processPer = 1.0f;

				this.OnTrigger?.Invoke(this);
				this.ClearRecordAngleDelta();

				return;
			}
		}

		// 计算角度与期望阈值的百分比
		m_processPer = Mathf.Clamp01(Mathf.Abs(angle) / m_triggerThreshold);
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_handler = this.transform;
		m_triggerThreshold = 360;

		m_clearRecordAngleDeltaOnEnable = true;
	}

	private void OnDrawGizmosSelected()
	{
		//string msg = "";

		//float s = 0;
		//foreach (var d in m_recordAngleDelta)
		//{
		//	s += d;
		//	msg += $"{d}\n";
		//}
		//msg += $"= {s}";

		UnityEditor.Handles.Label(this.transform.position, this.m_processPer.ToString());
	}
#endif
}
