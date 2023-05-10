using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AutoRecycle : MonoBehaviour
{
	public UnityEvent<GameObject> OnReadyRecycle = new UnityEvent<GameObject>();

	[SerializeField]
	private bool m_forceDisable = true; 

	[Space(10)]

	[Header("Rect Range")]
	[SerializeField] private bool m_useRectRange;
	[SerializeField] private Vector2 m_rangeMax;
	[SerializeField] private Vector2 m_rangeMin;

	[Tooltip("dies out of range when selected")]
	[SerializeField] private bool m_reverse;

	[Space(10)]

	[Header("Lifetime")]
	[SerializeField] private bool m_useLifetime;
	[SerializeField] private float m_maxLifetime;

	private float m_timer = 0;

	public void SetRectRange(Vector2 max, Vector2 min, bool? useRange = null)
	{
		m_rangeMax = max;
		m_rangeMin = min;
		if (useRange.HasValue)
			m_useRectRange = useRange.Value;
	}
	public void SetLifetime(float lifetime, bool? useLifetime = null)
	{
		m_maxLifetime = lifetime;
		if (useLifetime.HasValue)
			m_useLifetime = useLifetime.Value;
	}

	public void Recycle()
	{
		this.OnReadyRecycle?.Invoke(this.gameObject);

		if (m_forceDisable)
			this.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		m_timer = 0;
	}

	private void Update()
	{
		if (this.m_useRectRange)
		{
			var pos = (Vector2)this.transform.position;

			bool flag = MathUtility.InRange(pos, m_rangeMin, m_rangeMax);
			if (m_reverse) flag = !flag;

			if (flag) this.Recycle();
		}
		if (this.m_useLifetime)
		{
			m_timer += Time.deltaTime;
			if (m_timer >= m_maxLifetime)
				this.Recycle();
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		if (m_useRectRange)
			DebugUtility.DrawRange(m_rangeMin, m_rangeMax);
	}
#endif
}
