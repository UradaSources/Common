using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InstancePool<T> where T : Component
{
	[SerializeField]
	private Transform m_root;

	[SerializeField]
	private T m_template;

	[SerializeField, HideInInspector]
	private List<T> m_instances = new List<T>();

	public int Count { get => m_instances.Count; }

	public int ActivedCount
	{
		get
		{
			int i = 0;
			foreach (var obj in m_instances)
			{
				if (obj.gameObject.activeSelf)
					i += 1;
			}
			return i;
		}
	}

	public void Resize(int size)
	{
		if (this.Count >= size) return;
		for (int i = this.Count; i < size; i++)
		{
			var obj = this.Creator(m_template, m_root);
			m_instances.Add(obj);
		}
	}

	public T NewInstance(System.Action<T> beforeEnable = null)
	{
		foreach(var obj in m_instances)
		{
			if (!obj.gameObject.activeSelf)
			{
				beforeEnable?.Invoke(obj);
				obj.gameObject.SetActive(true);

				return obj;
			}
		}

		int newSize = Mathf.Max((int)(this.Count * 1.5f), this.Count + 2);
		this.Resize(newSize);

		return this.NewInstance(beforeEnable);
	}

	public IEnumerable<T> FindInstance(System.Func<T, bool> condition)
	{
		foreach (var obj in m_instances)
		{
			if (condition.Invoke(obj))
				yield return obj;
		}
	}

	public IEnumerable<T> GetActivedInstance()
	{
		foreach (var obj in m_instances)
		{
			if (obj.gameObject.activeSelf)
				yield return obj;
		}
	}

	public void DisableAll(System.Action<T> onReadyDisable = null)
	{
		foreach (var obj in m_instances)
		{
			if (obj.gameObject.activeSelf)
			{
				onReadyDisable?.Invoke(obj);
				obj.gameObject.SetActive(false);
			}
		}
	}

	private T Creator(T template, Transform root)
	{
		var obj = GameObject.Instantiate(template, root);
		obj.gameObject.SetActive(false);

		return obj;
	}
}
