using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public class DebugGUI : MonoBehaviour
{
	[System.Serializable]
	public class DisplayObjectItem
	{
		public enum FieldFilter
		{
			None,
			All,
			OnlySerialized,
			Matched,
		}
		public enum PropertyFilter
		{
			None,
			All,
			OnlyAutoProperty,
			Matched,
		}

		[SerializeField] private Object m_target;

		[SerializeField] private FieldFilter m_fieldFilter = FieldFilter.All;
		[SerializeField] private PropertyFilter m_propertyFilter = PropertyFilter.OnlyAutoProperty;

		[SerializeField] private string[] m_fieldMatchArray;
		[SerializeField] private string[] m_propertyMatchArray;

		private FieldInfo[] _fieldCache;
		public FieldInfo[] fieldInfoCache
		{
			get
			{
				if (_fieldCache == null) _fieldCache = this.GetField().ToArray();
				return _fieldCache;
			}
		}

		private PropertyInfo[] _propertyInfo;
		public PropertyInfo[] propertyInfo
		{
			get
			{
				if (_propertyInfo == null) _propertyInfo = this.GetProperty().ToArray();
				return _propertyInfo;
			}
		}

		public Object target => m_target;

		public Vector2 fieldScrollViewPosition { set; get; }
		public Vector2 propertyScrollViewPosition { set; get; }

		private IEnumerable<FieldInfo> GetField()
		{
			var type = m_target.GetType();

			switch (m_fieldFilter)
			{
			case FieldFilter.All:
				{
					foreach (var t in type.GetFields(BindingFlags.NonPublic))
						yield return t;

					yield break;
				}
			case FieldFilter.OnlySerialized:
				{
					foreach (var t in type.GetFields(BindingFlags.NonPublic))
					{
						if (t.GetCustomAttribute<SerializeField>() != null)
							yield return t;
					}

					yield break;
				}
			case FieldFilter.Matched:
				{
					foreach (var t in type.GetFields(BindingFlags.NonPublic))
					{
						var matched = m_fieldMatchArray.FirstOrDefault(p => Regex.IsMatch(t.Name, p));
						if (!string.IsNullOrEmpty(matched))
							yield return t;
					}

					yield break;
				}
			}
		}
		private IEnumerable<PropertyInfo> GetProperty()
		{
			var type = m_target.GetType();

			switch (m_propertyFilter)
			{
			case PropertyFilter.All:
				{
					foreach (var t in type.GetProperties(BindingFlags.NonPublic))
						yield return t;

					yield break;
				}
			case PropertyFilter.OnlyAutoProperty:
				{
					foreach (var t in type.GetProperties(BindingFlags.NonPublic))
					{
						if (t.Name.Contains("BackingField"))
							yield return t;
					}

					yield break;
				}
			case PropertyFilter.Matched:
				{
					foreach (var t in type.GetProperties(BindingFlags.NonPublic))
					{
						var matched = m_propertyMatchArray.FirstOrDefault(p => Regex.IsMatch(t.Name, p));
						if (!string.IsNullOrEmpty(matched))
							yield return t;
					}

					yield break;
				}
			}
		}
	}

	[SerializeField] private List<DisplayObjectItem> m_objects;

	private bool m_debug;
	private Vector2 m_position;

	private void Awake()
	{
#if UNITY_EDITOR
		m_debug = true;
#else
		m_debug = false;
#endif

		var path = Application.streamingAssetsPath + "/DEBUG";
		if (System.IO.File.Exists(path))
			m_debug = System.IO.File.ReadAllText(path).StartsWith("true");
	}
	private void OnGUI()
	{
		if (!m_debug) return;

		m_position = GUILayout.BeginScrollView(m_position);
		foreach (var item in m_objects)
		{
			GUILayout.Space(10);
			GUILayout.Label($"<b>{item.target.name}({item.target.GetType().Name})</b>");

			GUILayout.Label($"Field({item.fieldInfoCache.Length}):");
			item.fieldScrollViewPosition = GUILayout.BeginScrollView(item.fieldScrollViewPosition);
			foreach (var t in item.fieldInfoCache)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(t.Name);
				GUILayout.FlexibleSpace();
				GUILayout.Label(t.GetValue(item.target).ToString());
				GUILayout.BeginHorizontal();
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndScrollView();
	}
}