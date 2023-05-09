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

	[SerializeField]
	private string m_sendMessage;

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

	private T DefaultCreator()
	{
		var obj = GameObject.Instantiate(m_template);
		obj.gameObject.SetActive(false);

		if(!string.IsNullOrEmpty(m_sendMessage))
			obj.SendMessage(m_sendMessage, SendMessageOptions.DontRequireReceiver);

		return obj;
	}

	public void Resize(int size)
	{
		if (this.Count >= size) return;
		for (int i = this.Count; i < size; i++)
		{
			var obj = this.DefaultCreator();

			obj.transform.SetParent(m_root);
			m_instances.Add(obj);
		}
	}

	public T GetInstance(System.Action<T> onReadyEnable = null)
	{
		foreach(var obj in m_instances)
		{
			if (!obj.gameObject.activeSelf)
			{
				onReadyEnable?.Invoke(obj);
				obj.gameObject.SetActive(true);

				return obj;
			}
		}

		int newSize = Mathf.Max((int)(this.Count * 1.5f), this.Count + 2);
		this.Resize(newSize);

		return this.GetInstance(onReadyEnable);
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
}
