using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class RuntimeDebuger : Singleton<RuntimeDebuger>
{
	public class Function
	{
		public System.Action action;
		public string text;

		public Function(string text, System.Action action)
		{
			this.text = text;
			this.action = action;
		}
	}

	public int maxLogNumber = 20;
	public float m_widthFactor = 0.333f;

	private Queue<string> m_logs = new Queue<string>();
	private List<Function> m_func = new List<Function>();

	private Vector2 m_logScrollPosition;
	private Vector2 m_bulletinScrollPosition;

	private List<string> m_bulletinMsg = new List<string>();

	public float Width
	{
		get => Screen.width * m_widthFactor;
	}

	public Function AddFunction(Function func)
	{
		m_func.Add(func);
		return func;
	}
	public bool RemoveFunction(Function func)
		=> m_func.Remove(func);

	public void AddBulletinMsg(string msg, params object[] args)
	{
		msg = string.Format(msg, args);
		m_bulletinMsg.Add(msg);
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		string head = $"{System.DateTime.Now.ToString("[MM/dd-hh:mm:ss] ")}";

		logString = head + logString;

		string color = "";
		switch (type)
		{
			case LogType.Log:
				color = "<color=green>";
				break;
			case LogType.Warning:
				color = "<color=yellow>";
				break;
			case LogType.Exception:
			case LogType.Error:
				color = "<color=red>";
				break;
			case LogType.Assert:
				color = "<color=magenta>";
				break;
		}

		if (!string.IsNullOrEmpty(color))
			logString = $"{color}{logString}</color>";

		while (m_logs.Count >= Mathf.Max(maxLogNumber, 1))
			m_logs.Dequeue();

		m_logs.Enqueue(logString);
	}

	private void DrawLogs()
	{
		var style = new GUIStyle(GUI.skin.label);
		style.richText = true;
		style.wordWrap = true;

		var maxHeight = Screen.height * 0.5f;

		var layout = new GUILayoutOption[] { GUILayout.Width(this.Width), GUILayout.MaxHeight(maxHeight) };
		using (var scrollViewScope = new GUILayout.ScrollViewScope(m_logScrollPosition, layout))
		{
			m_logScrollPosition = scrollViewScope.scrollPosition;

			foreach (string log in m_logs)
				GUILayout.Label(log, style);
		}
	}
	private void DrawBulletinMsg()
	{
		var maxHeight = Screen.height * 0.3f;
		var layout = new GUILayoutOption[] { GUILayout.Width(this.Width), GUILayout.MaxHeight(maxHeight) };
		using (var scrollViewScope = new GUILayout.ScrollViewScope(m_bulletinScrollPosition, layout))
		{
			m_bulletinScrollPosition = scrollViewScope.scrollPosition;

			var style = GUI.skin.label;
			style.richText = true;

			foreach (var v in m_bulletinMsg)
				GUILayout.Label(v, style);
		}
	} 
	private void DrawFunc()
	{
		const int ButtonWidth = 70;

		var funcStyle = new GUIStyle(GUI.skin.button);

		// 导出需要绘制的内容
		var contents = new GUIContent[m_func.Count];
		
		int i = 0;
		foreach (var itor in m_func)
		{ 
			var c = new GUIContent(itor.text);
			contents[i++] = c;
		}

		int xCount = Mathf.Clamp((int)(this.Width / ButtonWidth), 1, 5);
		int select = GUILayout.SelectionGrid(-1, contents, xCount);
		if (select > 0)
			m_func[select].action.Invoke();
	}

	protected override void Awake()
	{
		base.Awake();
		Application.logMessageReceived += HandleLog;
	}

	private void OnGUI()
	{
		this.DrawFunc();
		this.DrawBulletinMsg();
		this.DrawLogs();
	}

	private void LateUpdate()
	{
		m_bulletinMsg.Clear();
	}
}