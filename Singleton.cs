using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : Singleton where T : MonoBehaviour
{
	private static readonly object Lock = new object();

	private static T m_instance;

	[SerializeField]
	private bool m_persistent = true;

	public static T Instance
	{
		get
		{
			if (Quitting)
			{
				Debug.LogWarning($"[{nameof(Singleton)}<{typeof(T)}>] Instance will not be returned because the application is quitting.");
				// ReSharper disable once AssignNullToNotNullAttribute
				return null;
			}
			lock (Lock)
			{
				if (m_instance != null)
					return m_instance;
				var instances = FindObjectsOfType<T>();
				var count = instances.Length;
				if (count > 0)
				{
					if (count == 1)
						return m_instance = instances[0];
					Debug.LogWarning($"[{nameof(Singleton)}<{typeof(T)}>] There should never be more than one {nameof(Singleton)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");
					for (var i = 1; i < instances.Length; i++)
						Destroy(instances[i]);
					return m_instance = instances[0];
				}

				Debug.Log($"[{nameof(Singleton)}<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
				return m_instance = new GameObject($"({nameof(Singleton)}){typeof(T)}")
						   .AddComponent<T>();
			}
		}
	}

	protected virtual void Awake()
	{
		if (m_persistent) DontDestroyOnLoad(gameObject);
	}
}

public abstract class Singleton : MonoBehaviour
{
	#region  Properties
	public static bool Quitting { get; private set; }
	#endregion

	#region  Methods
	private void OnApplicationQuit()
	{
		Quitting = true;
	}
	#endregion
}