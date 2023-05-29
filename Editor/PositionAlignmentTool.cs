using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PositionAlignmentTool2
{
	private bool m_alignX = false;
	private bool m_alignY = false;
	private bool m_alignZ = true;

	private bool m_srcLocal = false; // 目标是否使用局部坐标系
	private bool m_dstLocal = false; // 基准来源是否使用局部坐标系

	private Vector2 m_scrollPosition = Vector2.zero;

	private List<Transform> m_selectedTrList = new List<Transform>();
	private Transform m_baseObject = null;

	private Rect _windowRect;

	private bool _autoSetBase = true;

	public bool AutoSetBase
	{
		get => _autoSetBase;
		set
		{
			if (_autoSetBase == value) return;

			_autoSetBase = value;
			this.TryUpdateBaseObject();
		}
	}

	public Rect WindowRect
	{
		get => _windowRect;
	}

	private void TryUpdateBaseObject()
	{
		if (_autoSetBase)
		{
			if (m_selectedTrList.Count > 0)
			{
				int lastIndex = m_selectedTrList.Count - 1;
				m_baseObject = m_selectedTrList[lastIndex];
			}
			else
				m_baseObject = null;
		}
	}

	private Rect OptionPanel(Vector2 pos, Color? c)
	{
		var rect = new Rect(pos.x + 1, pos.y, this.WindowRect.width - 2, 55);

		GUILayout.BeginArea(rect);
		if (c.HasValue)
		{
			var bg = new Rect(0, 0, rect.width, rect.height);
			EditorGUI.DrawRect(bg, c.Value);
		}

		GUILayout.Label("Operation Option", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
		m_alignX = GUILayout.Toggle(m_alignX, "Align the X");
		m_alignY = GUILayout.Toggle(m_alignY, "Align the Y");
		m_alignZ = GUILayout.Toggle(m_alignZ, "Align the Z");
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		var _tmp3 = GUILayout.Toggle(m_dstLocal && m_srcLocal, "All local");
		if (_tmp3 != (m_dstLocal && m_srcLocal))
		{
			m_dstLocal = _tmp3;
			m_srcLocal = _tmp3;
		}

		m_dstLocal = GUILayout.Toggle(m_dstLocal, "Dst local");
		m_srcLocal = GUILayout.Toggle(m_srcLocal, "Src local");
		EditorGUILayout.EndHorizontal();

		GUILayout.EndArea();

		return rect;
	}

	private Rect SelectedTrListPanel(Vector2 pos, Color? c)
	{
		var rect = new Rect(pos.x, pos.y, this.WindowRect.width, 155);

		GUILayout.BeginArea(rect);
		if (c.HasValue)
		{
			var bg = new Rect(0, 0, rect.width, rect.height);
			EditorGUI.DrawRect(bg, c.Value);
		}

		GUILayout.Label($"Selected Transform List({m_selectedTrList.Count})", EditorStyles.boldLabel);

		m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
		for (int i = 0; i < m_selectedTrList.Count; i++)
		{
			var go = m_selectedTrList[i];
			if (go)
			{
				var _tmp1 = EditorGUILayout.ObjectField("", go, typeof(Transform), true);
				m_selectedTrList[i] = _tmp1 as Transform;
			}
			else
				m_selectedTrList.RemoveAt(i);
		}
		GUILayout.EndScrollView();

		this.AutoSetBase = EditorGUILayout.Toggle("Set last as base", this.AutoSetBase);

		var _tmp2 = EditorGUILayout.ObjectField("Base", m_baseObject, typeof(Transform), true);
		m_baseObject = _tmp2 as Transform;

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Update Selected"))
		{
			// 获取当前选中的对象
			m_selectedTrList.Clear();
			if (MiscUtils.GetSelectedComponentsByOrder(ref m_selectedTrList) > 0)
				this.TryUpdateBaseObject();
		}
		if (GUILayout.Button("Append Selected"))
		{
			var result = new List<Transform>();
			if (MiscUtils.GetSelectedComponentsByOrder(ref result) > 0)
			{
				MiscUtils.AppendList(ref m_selectedTrList, result);
				this.TryUpdateBaseObject();
			}
		}
		if (GUILayout.Button("Remove Selected"))
		{
			var go = MiscUtils.GetSelectedGameObjectInScene();
			if (go) m_selectedTrList.Remove(go.transform);

			this.TryUpdateBaseObject();
		}
		if (GUILayout.Button("Remove All"))
		{
			m_selectedTrList.Clear();
			this.TryUpdateBaseObject();
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.EndArea();

		return rect;
	}

	private Rect OperationPanel(Vector2 pos, Color? c)
	{
		var rect = new Rect(pos.x, pos.y, this.WindowRect.width, 20);

		GUILayout.BeginArea(rect);
		if (c.HasValue)
		{
			var bg = new Rect(0, 0, rect.width, rect.height);
			EditorGUI.DrawRect(bg, c.Value);
		}

		if (GUILayout.Button("Alignment"))
		{
			if (m_baseObject != null)
			{
				// 记录对象位置用以撤销操作
				Undo.RecordObjects(m_selectedTrList.ToArray(), "Reset aligned position");

				var datumPos = m_dstLocal ? m_baseObject.transform.localPosition : m_baseObject.transform.position;
				foreach (var go in m_selectedTrList)
				{
					var targetPos = m_srcLocal ? go.transform.localPosition : go.transform.position;

					if (m_alignX) targetPos.x = datumPos.x;
					if (m_alignY) targetPos.y = datumPos.y;
					if (m_alignZ) targetPos.z = datumPos.z;

					if (m_srcLocal)
						go.transform.localPosition = targetPos;
					else
						go.transform.position = targetPos;
				}
			}
			else
				Debug.LogWarning("There must be a base object");
		}

		GUILayout.EndArea();

		return rect;
	}

	public void Draw(Rect position)
	{
		// 更新窗口矩形
		_windowRect = position;

		var bgc = new Color32(80, 80, 80, 255);

		var lastPos = this.OptionPanel(Vector2.zero, bgc).yMax;
		lastPos = this.SelectedTrListPanel(new Vector2(0, lastPos + 5), bgc).yMax;
		lastPos = this.OperationPanel(new Vector2(0, lastPos + 5), bgc).yMax;
	}
}

//private void DrawBaseObejctAxis()
//{
//	if (!m_baseObject) return;
//	var pos = m_baseObject.transform.position;

//	// Handles.BeginGUI();

//	if (m_alignX)
//	{
//		// Handles.color = Color.red;
//		Debug.DrawLine(pos - new Vector3(1000f, 0f, 0f), pos + new Vector3(1000f, 0f, 0f));
//	}
//	if (m_alignY)
//	{
//		Handles.color = Color.green;
//		Debug.DrawLine(pos - new Vector3(0f, 1000f, 0f), pos + new Vector3(0f, 1000f, 0f));
//	}
//	if (m_alignZ)
//	{
//		Handles.color = Color.blue;
//		Debug.DrawLine(pos - new Vector3(0f, 0f, 1000f), pos + new Vector3(0f, 0f, 1000f));
//	}

//	// Handles.EndGUI();
//}


public class PositionAlignmentTool
{
	private bool m_alignX = false;
	private bool m_alignY = false;
	private bool m_alignZ = true;

	private bool m_srcLocal = false; // 目标是否使用局部坐标系
	private bool m_dstLocal = false; // 基准来源是否使用局部坐标系

	private Vector2 m_scrollPosition = Vector2.zero;

	private List<Transform> m_selectedTrList = new List<Transform>();
	private Transform m_baseObject = null;

	private Rect _windowRect;

	private bool _autoSetBase = true;

	public bool AutoSetBase
	{
		get => _autoSetBase;
		set
		{
			if (_autoSetBase == value) return;

			_autoSetBase = value;
			this.TryUpdateBaseObject();
		}
	}

	public Rect WindowRect
	{
		get => _windowRect;
	}

	private void TryUpdateBaseObject()
	{
		if (_autoSetBase)
		{
			if (m_selectedTrList.Count > 0)
			{
				int lastIndex = m_selectedTrList.Count - 1;
				m_baseObject = m_selectedTrList[lastIndex];
			}
			else
				m_baseObject = null;
		}
	}

	private Rect OptionPanel(Vector2 pos, Color? c)
	{
		var rect = new Rect(pos.x + 1, pos.y, this.WindowRect.width - 2, 55);

		GUILayout.BeginArea(rect);
		if (c.HasValue)
		{
			var bg = new Rect(0, 0, rect.width, rect.height);
			EditorGUI.DrawRect(bg, c.Value);
		}

		GUILayout.Label("Operation Option", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal();
		var style = new GUIStyle(GUI.skin.toggle);
		style.richText = true;
		style.fontStyle = FontStyle.Bold;

		m_alignX = GUILayout.Toggle(m_alignX, "<color=red>Align the X</color>", style);
		m_alignY = GUILayout.Toggle(m_alignY, "<color=lime>Align the Y</color>", style);
		m_alignZ = GUILayout.Toggle(m_alignZ, "<color=blue>Align the Z</color>", style);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		var _tmp3 = GUILayout.Toggle(m_dstLocal && m_srcLocal, "All local");
		if (_tmp3 != (m_dstLocal && m_srcLocal))
		{
			m_dstLocal = _tmp3;
			m_srcLocal = _tmp3;
		}

		m_dstLocal = GUILayout.Toggle(m_dstLocal, "Dst local");
		m_srcLocal = GUILayout.Toggle(m_srcLocal, "Src local");
		EditorGUILayout.EndHorizontal();

		GUILayout.EndArea();

		return rect;
	}

	private Rect SelectedTrListPanel(Vector2 pos, Color? c)
	{
		var rect = new Rect(pos.x, pos.y, this.WindowRect.width, 155);

		GUILayout.BeginArea(rect);
		if (c.HasValue)
		{
			var bg = new Rect(0, 0, rect.width, rect.height);
			EditorGUI.DrawRect(bg, c.Value);
		}

		GUILayout.Label($"Selected Transform List({m_selectedTrList.Count})", EditorStyles.boldLabel);

		m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
		for (int i = 0; i < m_selectedTrList.Count; i++)
		{
			var go = m_selectedTrList[i];
			if (go)
			{
				var _tmp1 = EditorGUILayout.ObjectField("", go, typeof(Transform), true);
				m_selectedTrList[i] = _tmp1 as Transform;
			}
			else
				m_selectedTrList.RemoveAt(i);
		}
		GUILayout.EndScrollView();

		this.AutoSetBase = EditorGUILayout.Toggle("Set last as base", this.AutoSetBase);

		var _tmp2 = EditorGUILayout.ObjectField("Base", m_baseObject, typeof(Transform), true);
		m_baseObject = _tmp2 as Transform;

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Update Selected"))
		{
			// 获取当前选中的对象
			m_selectedTrList.Clear();
			if (MiscUtils.GetSelectedComponentsByOrder(ref m_selectedTrList) > 0)
				this.TryUpdateBaseObject();
		}
		if (GUILayout.Button("Append Selected"))
		{
			var result = new List<Transform>();
			if (MiscUtils.GetSelectedComponentsByOrder(ref result) > 0)
			{
				MiscUtils.AppendList(ref m_selectedTrList, result);
				this.TryUpdateBaseObject();
			}
		}
		if (GUILayout.Button("Remove Selected"))
		{
			var go = MiscUtils.GetSelectedGameObjectInScene();
			if (go) m_selectedTrList.Remove(go.transform);

			this.TryUpdateBaseObject();
		}
		if (GUILayout.Button("Remove All"))
		{
			m_selectedTrList.Clear();
			this.TryUpdateBaseObject();
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.EndArea();

		return rect;
	}

	private Rect OperationPanel(Vector2 pos, Color? c)
	{
		var rect = new Rect(pos.x, pos.y, this.WindowRect.width, 20);

		GUILayout.BeginArea(rect);
		if (c.HasValue)
		{
			var bg = new Rect(0, 0, rect.width, rect.height);
			EditorGUI.DrawRect(bg, c.Value);
		}

		if (GUILayout.Button("Alignment"))
		{
			if (m_baseObject != null)
			{
				// 记录对象位置用以撤销操作
				Undo.RecordObjects(m_selectedTrList.ToArray(), "Reset aligned position");

				var datumPos = m_dstLocal ? m_baseObject.transform.localPosition : m_baseObject.transform.position;
				foreach (var go in m_selectedTrList)
				{
					var targetPos = m_srcLocal ? go.transform.localPosition : go.transform.position;

					if (m_alignX) targetPos.x = datumPos.x;
					if (m_alignY) targetPos.y = datumPos.y;
					if (m_alignZ) targetPos.z = datumPos.z;

					if (m_srcLocal)
						go.transform.localPosition = targetPos;
					else
						go.transform.position = targetPos;
				}
			}
			else
				Debug.LogWarning("There must be a base object");
		}

		GUILayout.EndArea();

		return rect;
	}

	public void Draw(Rect position)
	{
		// 更新窗口矩形
		_windowRect = position;

		var bgc = new Color32(80, 80, 80, 255);

		var lastPos = this.OptionPanel(Vector2.zero, bgc).yMax;
		lastPos = this.SelectedTrListPanel(new Vector2(0, lastPos + 5), bgc).yMax;
		lastPos = this.OperationPanel(new Vector2(0, lastPos + 5), bgc).yMax;
	}
}
