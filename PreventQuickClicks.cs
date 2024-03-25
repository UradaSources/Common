using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ·ÀÖ¹¿ìËÙµã»÷
public class PreventQuickClicks : MonoBehaviour
{
	[SerializeField] private Image[] m_triggerMaskList;
	[SerializeField] private float m_minClickInterval;

	private void Awake()
	{
		foreach (var t in m_triggerMaskList) t.raycastTarget = false;
	}
	private IEnumerator Start()
	{
		var clickInterval = new WaitForSeconds(m_minClickInterval);

		while (true)
		{
			if (Input.GetMouseButtonUp(0))
			{
				foreach (var t in m_triggerMaskList) 
					t.raycastTarget = true;
				
				yield return clickInterval;
				
				foreach (var t in m_triggerMaskList) 
					t.raycastTarget = false;
			}

			yield return null;
		}
	}
}
