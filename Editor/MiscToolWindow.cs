using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MiscToolWindow : EditorWindow
{
	[MenuItem("Custom/Misc Tool Window")]
	private static void Init()
	{
		MiscToolWindow window = (MiscToolWindow)EditorWindow.GetWindow(typeof(MiscToolWindow));
		window.Show();
	}

	// private bool m_openPosAlignmentTool = false;
	private PositionAlignmentTool m_positionAlignmentTool = new PositionAlignmentTool();

	private void OnGUI()
	{
		m_positionAlignmentTool.Draw(this.position);
	}
}
