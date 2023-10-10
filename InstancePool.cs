using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InstancePool<T>
	where T : Component
{
	[SerializeField] private Transform m_root;
	[SerializeField] private T m_template;

	[SerializeField, HideInInspector]
	private List<T> m_instances = new List<T>();

	public int InstanceCount => m_instances.Count;
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

	public Transform Root => m_root;

	public int Resize(int size, bool defaultActiveCreated = false, bool destroy = false)
	{
		if (this.InstanceCount <= size)
		{
			for (int i = this.InstanceCount; i < size; i++)
			{
				var obj = this.Creator(m_template, m_root, defaultActiveCreated);
				m_instances.Add(obj);
			}
		}
		else if (destroy)
		{
			int count = this.InstanceCount - size;
			for (int i = 0; i < count; i++)
				Object.Destroy(m_instances[i]);
			m_instances.RemoveRange(m_instances.Count - count, count);
		}
		return this.InstanceCount;
	}

	public T GetOrCreate(System.Action<T> beforeEnable = null)
	{
		foreach (var obj in m_instances)
		{
			if (!obj.gameObject.activeSelf)
			{
				beforeEnable?.Invoke(obj);
				obj.gameObject.SetActive(true);

				return obj;
			}
		}

		int newSize = Mathf.Max((int)(this.InstanceCount * 1.5f), this.InstanceCount + 2);
		this.Resize(newSize);

		return this.GetOrCreate(beforeEnable);
	}

	public IEnumerable<T> GetActived()
	{
		foreach (var obj in m_instances)
		{
			if (obj.gameObject.activeSelf)
				yield return obj;
		}
	}
	public IEnumerable<T> GetFiltered(System.Func<T, bool> filter)
	{
		foreach (var obj in m_instances)
		{
			if (filter.Invoke(obj))
				yield return obj;
		}
	}

	public void Maintain(int num)
	{
		Debug.Assert(num > 0);
		int activeCount = this.ActivedCount;
		if (activeCount > num)
			this.Disable(activeCount - num);
		else
		{
			for (int i = 0; i< num - activeCount; i++) 
				this.GetOrCreate();
		}
	}

	public int Disable(int maxNum, System.Func<T, bool> filter = null, System.Action<T> beforeDisable = null)
	{
		int count = 0;
		foreach (var ins in m_instances)
		{
			if (ins.gameObject.activeSelf && (filter == null || filter.Invoke(ins)))
			{
				beforeDisable?.Invoke(ins);
				ins.gameObject.SetActive(false);

				if (++count >= maxNum)
					break;
			}
		}
		return count;
	}
	public void DisableAll(System.Action<T> beforeDisable = null)
	{
		foreach (var obj in m_instances)
		{
			if (obj.gameObject.activeSelf)
			{
				beforeDisable?.Invoke(obj);
				obj.gameObject.SetActive(false);
			}
		}
	}

	private T Creator(T template, Transform root, bool defaultActiveCreated = false)
	{
		var obj = GameObject.Instantiate(template, root);
		obj.gameObject.SetActive(defaultActiveCreated);
		return obj;
	}
}
