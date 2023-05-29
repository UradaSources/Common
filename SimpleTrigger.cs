using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTrigger : MonoBehaviour
{
	[System.Serializable]
	public class Condition
	{
		public enum Mode
		{
			None,

			// 摧毁或禁用gameObject
			DisableGoOnTrigger,
			DestroyGoOnTrigger,

			// 移除当前Condition
			RemoveThisCondition,

			// 销毁整个Trigger
			DestroyTriggerOnTrigger
		}

		public UnityEvent<GameObject> OnTrigger = new UnityEvent<GameObject>();

		public Mode mode;

		[Header("Range data")]
		public bool triggerWhenOutOfRange;
		public Collider2D range;

		public bool rangeReverse;

		[Header("Lifetime data")]
		public bool useLifetime;
		public float maxLifetime;

		private float m_timer = 0;

		public void SetRectRange(Collider2D range, bool reverse, bool? useRange = null)
		{
			this.range = range;
			this.rangeReverse = reverse;

			if (useRange.HasValue)
				this.triggerWhenOutOfRange = useRange.Value;
		}
		public void SetLifetime(float lifetime, bool? useLifetime = null)
		{
			this.maxLifetime = lifetime;
			if (useLifetime.HasValue)
				this.useLifetime = useLifetime.Value;
		}

		public void _Reset()
		{
			m_timer = 0;
		}
		public bool Check(GameObject go)
		{
			if (this.triggerWhenOutOfRange)
			{
				// 计算当前位置是否在碰撞体内
				var pos = (Vector2) go.transform.position;

				// 不在碰撞体内时触发回调函数, 或是由m_rangeReverse取反标志
				bool flag = !this.range.OverlapPoint(pos);
				if (this.rangeReverse) flag = !flag;

				return flag;
			}
			if (this.useLifetime)
			{
				m_timer += Time.deltaTime;
				return m_timer >= this.maxLifetime;
			}
			return false;
		}
	}

	[SerializeField]
	private GameObject m_go;

	[SerializeField]
	private List<Condition> m_conditions;

	private List<int> _toBeRemoveConditionIndex = new List<int>();

	public void ResetLocalPositionToOrig()
	{
		if (this.gameObject.TryGetComponent(out Rigidbody2D rb))
			rb.velocity = Vector3.zero;
		this.transform.localPosition = Vector3.zero;
	}

	private void OnEnable()
	{
		foreach (var c in m_conditions)
			c._Reset();
	}

	private void Update()
	{
		if (m_go == null) return;

		int i = 0;
		foreach (var c in m_conditions)
		{
			if (c.Check(m_go))
			{
				switch (c.mode)
				{
					case Condition.Mode.DisableGoOnTrigger:
						m_go.SetActive(false);
						break;

					case Condition.Mode.DestroyGoOnTrigger:
						GameObject.Destroy(m_go);
						break;

					case Condition.Mode.RemoveThisCondition:
						_toBeRemoveConditionIndex.Add(i);
						break;

					case Condition.Mode.DestroyTriggerOnTrigger:
						Component.Destroy(this);
						break;
				}
				c.OnTrigger?.Invoke(m_go);
			}
			i++;
		}

		// 移除需要移除的条件
		if (_toBeRemoveConditionIndex.Count > 0)
		{
			for (int j = _toBeRemoveConditionIndex.Count - 1; j >= 0; j--)
			{
				var index = _toBeRemoveConditionIndex[j];
				m_conditions.RemoveAt(index);
			}
			_toBeRemoveConditionIndex.Clear();
		}
	}

#if UNITY_EDITOR
	private void Reset()
	{
		m_go = this.gameObject;
	}
#endif
}
