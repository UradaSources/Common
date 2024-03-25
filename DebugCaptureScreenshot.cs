#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DebugCaptureScreenshot : MonoBehaviour
{
	[SerializeField] private KeyCode m_hotkey;
	[SerializeField] private string m_targetPath;
	[SerializeField] private bool m_selectSavePath;

	public string defaultFileName => DateTime.Now.ToString("T");

	private void Awake()
	{
		if (!Application.isEditor)
		{
			Destroy(this);
			Debug.LogWarning($"Remove legacy editor tools {this.name}");
		}
	}
	private void Update()
	{
		if (Input.GetKeyDown(m_hotkey))
		{
			string path;
			if (m_selectSavePath)
			{
				path = EditorUtility.SaveFilePanel("Capture Screenshot", m_targetPath, this.defaultFileName, "png");
				if (string.IsNullOrEmpty(path)) return;
			}
			else
			{ 
				if (!Directory.Exists(m_targetPath))
					m_targetPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

				path = Path.Combine(m_targetPath, this.defaultFileName + ".png");
			}

			ScreenCapture.CaptureScreenshot(path, 1);

			Debug.Log($"Capture screenshot to {path}");
		}
	}
	private void Reset()
	{
		m_hotkey = KeyCode.Space;
		m_targetPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		m_selectSavePath = true;
	}
}
#endif