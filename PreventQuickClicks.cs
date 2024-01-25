using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PreventQuickClicks : MonoBehaviour
{
	[SerializeField] private Image[] m_masks;
	[SerializeField] private float m_minClickInterval;

	private void Awake()
	{
		foreach (var t in m_masks) t.raycastTarget = false;
	}
	private IEnumerator Start()
	{
		var clickInterval = new WaitForSeconds(m_minClickInterval);

		while (true)
		{
			if (Input.GetMouseButtonUp(0))
			{
				foreach (var t in m_masks) t.raycastTarget = true;
				yield return clickInterval;
				foreach (var t in m_masks) t.raycastTarget = false;
			}

			yield return null;
		}
	}

	IEnumerable<int> DcQSort(IEnumerable<int> nums)
	{
		if (nums.Count() < 2) return nums;

		int baseValue = nums.First();

		var min = nums.Skip(1).Where(t => t <= baseValue);
		var max = nums.Skip(1).Where(t => t > baseValue);

		return DcQSort(min).Append(baseValue).Concat(DcQSort(max));
	}
}
