using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 暂时搁置
public class SpriteFrameExpandEditorWindow : EditorWindow
{
	[MenuItem("Custom/SpriteFrame Expand Editor")]
	private static void ShowWindow()
	{
		var window = (SpriteFrameExpandEditorWindow)EditorWindow.GetWindow(typeof(SpriteFrameExpandEditorWindow));
		window.Show();
	}

	[SerializeField]
	private SpriteAnimation m_target;

	private Vector2 m_scroll_pos;
	private int m_selected_index;

	private bool m_basic_info_foldout;
	private bool m_keyframe_editor_foldout;

	// 帧编辑器中所要显示的条目
	private string[] m_kyeframe_editor_entry;

	// 一个简单的preview动画播放器
	private float m_player_timer;

	private int m_player_keyframe_index;
	private int m_player_frame_counter;

	private float m_last_timestamp;
	private float _delta_time;

	public float delta_time => _delta_time;

	public SpriteAnimation.Keyframe? current_preview_keyframe
	{
		get
		{
			if (m_target && m_target.TryGetKeyframe(m_player_keyframe_index, out var kf))
			{
				return kf;
			}
			return null;
		}
	}

	// 通用资产头部
	private void drawAssetHeader()
	{
		EditorGUILayout.BeginHorizontal();

		var rt = EditorGUILayout.ObjectField("Assets", m_target, typeof(SpriteAnimation), false) as SpriteAnimation;
		//if (GUILayout.Button("Create In Unity Assets"))
		//{
		//	var dir = EditorUtility.SaveFilePanelInProject("Create New KeyframeSequence", "New KeyframeSequence", "asset", "");
		//}

		EditorGUILayout.EndHorizontal();

		if (rt != m_target)
		{
			m_target = rt;
			this.resetPreviewPlayer();
		}
	}

	// 绘制信息栏
	private void drawBaiscInfos()
	{
		// 只读信息折叠组
		m_basic_info_foldout = EditorGUILayout.Foldout(m_target && m_basic_info_foldout, "Info");
		if (m_target && m_basic_info_foldout)
		{
			EditorGUI.indentLevel++;
			// 采样率属性
			m_target.Sample = EditorGUILayout.IntField("Sample", m_target.Sample);

			using (new EditorGUI.DisabledScope(true))
			{
				EditorGUILayout.IntField("Keyframe Count", m_target.KeyframeCount);
				EditorGUILayout.IntField("Frame Count", m_target.TotalFrameCount);
				EditorGUILayout.FloatField("Delta Time", m_target.FrameDeltaTime);
				EditorGUILayout.FloatField("Duration", m_target.Duration);
			}
			EditorGUI.indentLevel--;
		}
	}

	// 绘制帧编辑器
	private void drawKeyframeEditor()
	{
		// EditorGUILayout.ObjectField(null, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));

		m_keyframe_editor_foldout = EditorGUILayout.Foldout(m_target && m_keyframe_editor_foldout, "Keyframe Editor");
		if (m_target && m_keyframe_editor_foldout)
		{
			EditorGUI.indentLevel--;

			// 左侧 =============================
			// if (m_kyeframe_editor_entry != null && m_kyeframe_editor_entry.Length > 0)
			//{
			//	m_scroll_pos = EditorGUILayout.BeginScrollView(m_scroll_pos, GUILayout.Height(150));

			//	m_selected_index = GUILayout.SelectionGrid(m_selected_index, new string[] { "1", "2", "3", "4", "5", }, 1, EditorStyles.toolbarButton);

			//	EditorGUILayout.EndScrollView();
			//}

			// 右侧 =============================


			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.BeginVertical();
			GUILayout.Label("<b>一个简单的侧</b>");
			GUILayout.Label("<b>一个简单的侧</b>");
			GUILayout.Label("<b>一个简单的侧</b>");
			EditorGUILayout.EndVertical();

			var vv = EditorGUILayout.GetControlRect();

			EditorGUI.DrawRect(vv, Color.red);

			EditorGUILayout.EndHorizontal();

			// MiscUtils.GUIDrawTexture(null, 50);

			// =================================
			EditorGUI.indentLevel++;
		}
	}

	// 绘制动画演示
	private void drawPreview()
	{
		using (new EditorGUI.DisabledScope(!m_target))
		{
			var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
			EditorGUILayout.LabelField("---Animation Preview---", style);

			Sprite sp = this.current_preview_keyframe?.sprite;
			MiscUtils.GUIDrawTexture(sp, 80, 0);

			using (new EditorGUI.DisabledScope(true))
			{
				EditorGUILayout.IntField("Keyframe Index", m_player_keyframe_index);
				EditorGUILayout.IntField("Frame Counter", m_player_frame_counter);
			}
		}
	}

	// 重置预览播放器
	private void resetPreviewPlayer()
	{ 
		m_player_timer = 0;

		m_player_keyframe_index = 0;
		m_player_frame_counter = 0;		
	}

	// 更新delta time
	private void updateDeltaTime()
	{
		float timestamp = (float)EditorApplication.timeSinceStartup;
		_delta_time = timestamp - m_last_timestamp;

		m_last_timestamp = timestamp;
	}

	// 更新预览动画播放器
	private void updatePreviewPlayer()
	{
		if (m_target == null || !m_target.TryGetKeyframe(0, out _)) return;

		// 一帧最小持续的时间
		if (m_player_timer >= m_target.FrameDeltaTime)
		{
			// 计算递进帧数
			int skip = Mathf.RoundToInt(m_player_timer / m_target.FrameDeltaTime);

			// 递进帧数
			m_player_frame_counter += skip;

			// 剔除计时器中完整的帧时间
			m_player_timer -= skip * m_target.FrameDeltaTime;

			// 检查帧数是否超过当前的关键帧长度
			var keyframe = this.current_preview_keyframe.Value;
			while (m_player_frame_counter >= keyframe.length)
			{
				// 剔除关键帧残留的帧数
				m_player_frame_counter -= keyframe.length;
				// 递进关键帧
				m_player_keyframe_index = MathUtils.LoopIndex(m_player_keyframe_index, m_target.KeyframeCount);

				this.Repaint();
			}
		}

		// 更新计时器
		m_player_timer += this.delta_time;
	}

	private void OnEnable()
	{
		this.resetPreviewPlayer();
	}

	public void OnGUI()
	{
		this.drawAssetHeader();

		this.drawBaiscInfos();

		this.drawKeyframeEditor();

		this.drawPreview();

		//EditorGUILayout.BeginVertical();
		//// Object Field
		//// Sample Field
		//// keyforme count
		//// frame count
		//GUILayout.Label("<b>name</b>\nSample", m_label_style, GUILayout.Height(50));

		//EditorGUILayout.EndVertical();

		//// 左侧 -------------------------------------------------
		//EditorGUILayout.BeginHorizontal(GUILayout.Height(150));

		//m_scroll_pos = EditorGUILayout.BeginScrollView(m_scroll_pos, GUILayout.Height(150));

		//// 获取左侧滚动视图分配的布局区域大小
		////GUILayout.Label("hack", GUILayout.MaxHeight(0));
		////float left_width = GUILayoutUtility.GetLastRect().width;

		//m_selected_index = GUILayout.SelectionGrid(m_selected_index, new string[] { "ceshi11" }, 1, EditorStyles.toolbarButton);

		//EditorGUILayout.EndScrollView();

		//EditorGUILayout.BeginVertical();

		//// Sprite Filed
		//// length filed
		//// offset readonly filed
		//GUILayout.Label("<b>一个简单的侧</b>", m_label_style);

		//GUILayout.FlexibleSpace();

		//MiscUtils.GUIDrawTexture(null, 50);
		//EditorGUILayout.EndVertical();

		//EditorGUILayout.EndHorizontal();

		//// 新行
		//EditorGUILayout.BeginHorizontal();

		//GUILayout.Button("Move Up");
		//GUILayout.Button("Move Down");

		//GUILayout.FlexibleSpace();

		//GUILayout.Button("Insert");
		//GUILayout.Button("Remove");
		//GUILayout.Button("Append");

		//EditorGUILayout.EndHorizontal();

		//MiscUtils.GUIDrawTexture(null, 100);
	}

	public void Update()
	{
		this.updateDeltaTime();
		this.updatePreviewPlayer();
	}
}
