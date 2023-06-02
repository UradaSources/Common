using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-11)]
public class RuntimeDebuger : Singleton<RuntimeDebuger>
{
	public class Function
	{
		public System.Action action;
		public string text;

		public Function(System.Action action, string text)
		{
			this.action = action;
			this.text = text;
		}
	}

	public int maxLogNumber = 20;
	public float m_widthFactor = 0.333f;

	private Queue<string> m_logs = new Queue<string>();
	private List<Function> m_func = new List<Function>();

	private Vector2 m_scrollPosition;

	public Function AddFunction(Function func)
	{
		m_func.Add(func);
		return func;
	}
	public bool RemoveFunction(Function func)
		=> m_func.Remove(func);

	public float width
	{
		get => Screen.width * m_widthFactor;
	}

	protected override void Awake()
	{
		base.Awake();
		Application.logMessageReceived += HandleLog;
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

		var layout = new GUILayoutOption[] { GUILayout.Width(this.width), GUILayout.MaxHeight(maxHeight) };
		using (var scrollViewScope = new GUILayout.ScrollViewScope(m_scrollPosition, layout))
		{
			m_scrollPosition = scrollViewScope.scrollPosition;

			foreach (string log in m_logs)
				GUILayout.Label(log, style);
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

		int xCount = Mathf.Clamp((int)(this.width / ButtonWidth), 1, 5);
		int select = GUILayout.SelectionGrid(-1, contents, xCount);
		if (select > 0)
			m_func[select].action.Invoke();
	}

	private void OnGUI()
	{
		this.DrawFunc();
		this.DrawLogs();
	}
}