using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MiscUtils
{
	public static Color ClearAlpha { get => new Color(1, 1, 1, 0); }

	public static bool UpdateConfig<T>(string basename, ref T configRef, bool createDefault = true, bool validityCheck = true, string path = null)
		where T : struct
	{
		path = path ?? Application.streamingAssetsPath + "/config/";
		var configFullpath = System.IO.Path.Join(path, basename + ".config");
		try
		{
			if (System.IO.File.Exists(configFullpath))
			{
				var configText = System.IO.File.ReadAllText(configFullpath);
				var config = JsonUtility.FromJson<T>(configText);
				if (!validityCheck || !object.Equals(config, default(T)))
				{
					configRef = config;
					return true;
				}
				else
				{
					Debug.LogWarning($"config {configFullpath} is invaild");
					return false;
				}
			}
			else if (createDefault)
			{
				if (!System.IO.Directory.Exists(path))
					System.IO.Directory.CreateDirectory(path);

				using var file = System.IO.File.Create(configFullpath);
				var config = JsonUtility.ToJson(configRef);
				file.Write(System.Text.UTF8Encoding.UTF8.GetBytes(config));
				file.Flush();

				return true;
			}
		}
		catch (System.Exception exc)
		{
			Debug.LogWarning($"an error occurred while reading or creating the config file: {exc}");
		}

		return false;
	}

	public static bool InLayer(this GameObject go, string layer)
		=> InLayer(go, LayerMask.GetMask(layer));
	public static bool InLayer(this GameObject go, LayerMask layermask)
		=> layermask == (layermask | (1 << go.layer));

	// 获取临近层级的兄弟项
	public static Transform GetSibling(this Transform tr, int indexOffset, bool loopIndex = false)
	{
		var index = tr.GetSiblingIndex() + indexOffset;
		if (loopIndex) index %= tr.parent.childCount;

		return tr.parent.GetChild(index);
	}

	public static void RequiredComponent<ComT>(Component obj, out ComT com)
		where ComT : Component
	{
		if (!obj.TryGetComponent(out com))
			com = obj.gameObject.AddComponent<ComT>();
	}
	public static ComT RequiredComponent<ComT>(Component obj)
		where ComT : Component
	{
		if (!obj.TryGetComponent(out ComT com))
			com = obj.gameObject.AddComponent<ComT>();
		return com;
	}

	public static Color InvertColor(Color c)
	{
		return new Color(1.0f - c.r, 1.0f - c.g, 1.0f - c.b);
	}

	public static bool RandomBool()
	{
		return Random.value > 0.5f;
	}

	// 不重复的向列表添加项
	public static int Intersection<T>(ref List<T> dst, IEnumerable<T> src)
	{
		int count = dst.Count;
		foreach (var v in src)
		{
			if (dst.IndexOf(v) < 0)
				dst.Add(v);
		}
		return dst.Count - count;
	}

	public static IEnumerable<T> ListRange<T>(IList<T> src, int index, int length)
	{
		Debug.Assert(index >= 0 && index < src.Count);

		length = Mathf.Clamp(length, 1, src.Count - index);
		for (int i = index; i < index + length; i++)
			yield return src[i];
	}

	// 待修复
	//public static IEnumerable<T> ListRange<T>(IEnumerable<T> src, int index, int length)
	//{
	//	Debug.Assert(index >= 0);

	//	length = Mathf.Min(length, 1);

	//	int i = 0;
	//	foreach (var v in src)
	//	{
	//		if (i >= index)
	//			yield return v;

	//		if (++i < index + length)
	//			yield break;
	//	}
	//}

	public static int ConverAndAppendList<T1, T2>(ref List<T1> dst, IEnumerable<T2> src, bool allowRepeat = false)
	{
		int count = dst.Count;
		foreach (var v in src)
		{
			if (v is T1 tv)
			{
				if (allowRepeat == false || dst.IndexOf(tv) < 0)
					dst.Add(tv);
			}
		}
		return dst.Count - count;
	}

	public static IEnumerable<T1> TryCast<T1, T2>(this IEnumerable<T2> src, T1 def = default)
	{
		foreach (var i in src)
		{
			if (i is T1 tv)
				yield return tv;
			else
				yield return def;
		}
	}

	public static IEnumerable<Vector2> CastToVec2(this IEnumerable<Vector3> src)
	{
		foreach (var i in src)
			yield return new Vector2(i.x, i.y);
	}
	public static IEnumerable<Vector3> CastToVec3(this IEnumerable<Vector2> src, float z = 0)
	{
		foreach (var i in src)
			yield return new Vector3(i.x, i.y, z);
	}

	public static Sprite CreateSprite(Texture2D tex, int pixelsPerUnit)
	{
		var rect = new Rect(position:Vector2.zero, tex.texelSize);
		var pivot = new Vector2(0.5f, 0.5f);
		return Sprite.Create(tex, rect, pivot, pixelsPerUnit);
	}

	public static Texture2D CreateTexture(int w, int h, Color? color, bool temporary = true)
	{
		color = color ?? Color.clear;

		var tex = new Texture2D(w, h);

		var colors = new Color[w * h];
		for (int i = 0; i < colors.Length; i++)
			colors[i] = color.Value;

		if (temporary) tex.hideFlags = HideFlags.HideAndDontSave;

		tex.SetPixels(colors);
		tex.Apply();

		return tex;
	}

	public static Texture2D CreateColorTexture(Color color)
		=> CreateTexture(4, 4, color, true); // 将纹理大小控制为2的幂, 虽然不是必要的

	public static void GenCircleLine(LineRenderer line, float radius, Vector3? centerOffset = null, int sample = 32)
	{
		var offset = centerOffset ?? Vector3.zero;

		line.positionCount = sample;
		var points = new Vector3[line.positionCount];

		for (int i = 0; i < sample; i++)
		{
			var r = ((float)i / sample - 1) * Mathf.PI * 2.0f;
			var point = offset + (Vector3)MathUtils.Circle(radius, r);

			points[i] = point;
		}
		line.SetPositions(points);
	}

	// 待修复 rect.position指示左上角而不是中间
	public static Rect GetCameraViewport(Camera camera)
	{
		var y = camera.orthographicSize * 2;
		var x = y * camera.aspect;

		var pos = (Vector2)camera.transform.position;

		return new Rect(position: pos, size: new Vector2(x, y));
	}

	public static Vector2 MousePosition()
		=> Camera.main.ScreenToWorldPoint(Input.mousePosition);

	// 在保持与目标距离的同时以forward一面朝向目标
	public static Vector3 Alignment(Vector3 self, Vector3 target, Vector3 forward)
	{
		var dist = (self - target).magnitude;
		var dir = forward;

		return target - dir * dist;
	}

	public static bool BitMaskAnd(int mask, int v)
	{
		return (v & mask) == v;
	}
	public static bool BitMaskOr(int mask, int v)
	{
		return (v & mask) != 0;
	}

	public static IEnumerable<T2> Process<T1, T2>(this IEnumerable<T1> src, System.Func<T1, T2> handle)
	{
		foreach (var i in src)
			yield return handle.Invoke(i);
	}

	public static IEnumerable<Vector3> ExportPosition(IEnumerable<Transform> src, bool local = false)
	{
		foreach (var tr in src)
			yield return local ? tr.localPosition : tr.position;
	}
	public static IEnumerable<Vector3> ExportPositionFromRoot(Transform root)
	{
		foreach (Transform tr in root)
			yield return tr.position;
	}

	public static string FormatContainer<T>(IEnumerable<T> container, System.Func<T, string> toString = null)
	{
		if (toString == null) toString = (T t) => t.ToString();

		bool first = false;

		string str = "{";
		foreach (T child in container)
		{
			if (first)
			{
				str += toString.Invoke(child);
				first = false;
			}
			else
			{
				str += ", " + toString.Invoke(child);
			}
		}
		str += "}";
		return str;
	}

	public static string ListConcat<T>(IList<T> src, string separator = "", int maxLength = 1000)
	{
		var buffer = new System.Text.StringBuilder();

		for (int i = 0; i < Mathf.Min(src.Count, maxLength); i++)
		{
			if (i > 0)
				buffer.Append(separator);

			buffer.Append(src[i].ToString());
		}
		return buffer.ToString();
	}
	public static string ListConcat(string separator = "", params object[] args)
		=> ListConcat(args, separator);

	public static string ListConcat<T>(IEnumerable<T> src, string separator = "")
	{
		var buffer = new System.Text.StringBuilder();

		bool first = true;
		foreach (var v in src)
		{
			if (first) 
				first = false;
			else
				buffer.Append(separator);

			buffer.Append(v.ToString());
		}
		return buffer.ToString();
	}

	public static IEnumerable<float> SampleValue(int sample, float v)
	{
		if (sample < 2)
			yield return 0;
		else
		{ 
			for (int i = 0; i < sample; i++)
			{
				float r = (float)i / (sample - 1);
				yield return v * r;
			}
		}
	}

	// 过渡到正交视图
	public static IEnumerator CameraToOrthViewProcess(Camera camera, float duration, float focusDist, float fovTarget = 1)
	{
		// 记录原始的fov值
		var fovStart = camera.fieldOfView;

		// 计算固定的视角大小
		/* 视口锥台的高度与底边宽度
			Camera
				/+\ angle=fov
				-+- near
			   / + \
			  /  +  \
			 /   +   \
			---Target== far
			/    |    \
		整个锥体的角度为fov
		由+标注的即为dist, 由=标注的即为size
		在计算过程中将其视为一个直角三角形, 由三角函数计算另外2对参数的值
		*/
		var halfFovAngle = fovStart * 0.5f * Mathf.Deg2Rad;
		var size = Mathf.Tan(halfFovAngle) * focusDist;

		var camTr = camera.transform;
		var focusPoint = camTr.position + camTr.forward * focusDist;

		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			float r = Mathf.Clamp01(t / duration);
			camera.fieldOfView = Mathf.Lerp(fovStart, fovTarget, r);

			// 根据fov来更新目标到摄像机的距离
			// 随着fov越来越小, dist将越来越大来保持size不变
			var dist = MathUtils.Cot(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * size;

			// 更新摄像机的位置
			var pos = focusPoint - camTr.forward * dist;
			camTr.position = pos;

			yield return null;
		}

		// 在过渡动画完成后将视角切换为正交
		camera.orthographic = true;
		camera.orthographicSize = size;
	}

	// 过渡到透视视图
	public static IEnumerator CameraToPersViewProcess(Camera camera, float duration, float focusDist, float fovTarget = 60)
	{
		// 记录原始的fov值
		var fovStart = camera.fieldOfView;

		// 计算固定的视角大小
		/* 视口锥台的高度与底边宽度
			Camera
				/+\ angle=fov
				-+- near
			   / + \
			  /  +  \
			 /   +   \
			---Target== far
			/    |    \
		整个锥体的角度为fov
		由+标注的即为dist, 由=标注的即为size
		在计算过程中将其视为一个直角三角形, 由三角函数计算另外2对参数的值
		*/
		var halfFovAngle = fovStart * 0.5f * Mathf.Deg2Rad;
		var size = camera.orthographicSize;

		Debug.Assert(Mathf.Abs(halfFovAngle - 1.0f) < 10);

		var camTr = camera.transform;
		var focusPoint = camTr.position + camTr.forward * focusDist;

		// 初始化, 将相机移动到足够远的距离后设置为正交视角, 再逐渐拉近到正常位置
		var startDist = MathUtils.Cot(halfFovAngle) * size;

		var startPos = focusPoint - camTr.forward * startDist;
		camTr.position = startPos;

		// 设置回正交视角
		camera.orthographic = false;

		// 逐渐拉近
		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			float r = Mathf.Clamp01(t / duration);
			camera.fieldOfView = Mathf.Lerp(fovStart, fovTarget, r);

			// 根据fov来更新目标到摄像机的距离
			// 随着fov越来越大, dist将越来越小来保持size不变
			var dist = MathUtils.Cot(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * size;

			// 更新摄像机的位置
			var pos = focusPoint - camTr.forward * dist;
			camTr.position = pos;

			yield return null;
		}
	}

#if UNITY_EDITOR
	public static float GizmoScale(Vector3 position, Camera camera)
	{
		position = Gizmos.matrix.MultiplyPoint(position);
		if (camera)
		{
			Transform transform = camera.transform;
			Vector3 position2 = transform.position;
			float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
			Vector3 a = camera.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
			Vector3 b = camera.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
			float magnitude = (a - b).magnitude;
			return 80f / Mathf.Max(magnitude, 0.0001f);
		}

		return 20f;
	}
	public static float EditorGizmoScale(Vector3 position)
	{
		Camera camera = null;
		if (SceneView.lastActiveSceneView)
			camera = SceneView.lastActiveSceneView.camera;
		return GizmoScale(position, camera);
	}

	// 按顺序获取被选中的GameObject
	public static IEnumerable<GameObject> GetSelectedGameObjectsByOrder(bool inScene = true, System.Func<GameObject, bool> filter = null)
	{
		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			if (obj is GameObject go)
			{
				// 检查是否是场景中的对象且通过过滤器
				if (inScene && string.IsNullOrEmpty(go.scene.name))
					continue;
				if (filter != null && filter.Invoke(go))
					continue;

				yield return go;
			}
		}
	}

	// 按顺序遍历被选中对象且尝试获取目标组件并返回
	public static IEnumerable<T> GetSelectedComponentsByOrder<T>()
		where T : Component
	{
		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			if (obj is GameObject go && go.TryGetComponent(out T com))
				yield return com;
		}
	}

	// 按顺序获取被选中的任意对象, 必须继承自UnityEngine.Object
	// 使用过滤器进行过滤
	public static IEnumerable<T> GetSelectedObjectByOrder<T>(System.Func<T, bool> filter = null)
		where T : UnityEngine.Object
	{
		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			if (obj is T to && (filter == null || filter.Invoke(to)))
				yield return to;
		}
	}

	// 获取场景中被选中的GameObject
	public static IEnumerable<GameObject> GetSelectedGameObjectInScene()
	{
		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			// 只处理场景中的对象
			if (obj is GameObject go && !string.IsNullOrEmpty(go.scene.name))
				yield return go;
		}
	}
	
	// 绘制纹理
	public static void GUIDrawTexture(Sprite sprite, float height, float? x_offset = null)
	{
		var rect = EditorGUILayout.GetControlRect(false, height);

		// 绘制底色
		EditorGUI.DrawRect(rect, new Color(0.31f, 0.31f, 0.31f));

		if (sprite != null)
		{
			// 计算精灵的绘制大小
			// 为了维持精灵的比例
			// 以高度为标准, 计算实际高度与绘制高度的比值
			// 再将宽度乘以比值
			var r = rect.height / sprite.rect.height;
			var size = new Vector2(r * sprite.rect.width, rect.height);

			// 计算精灵的绘制坐标
			// 若没有提供特定偏移则设为中心
			x_offset = x_offset ?? rect.width * 0.5f;
			var pos = new Vector2(x_offset.Value, rect.y);

			rect.size = size;
			rect.position = pos;

			// 计算精灵在纹理上的归一矩形
			var tex_rect = sprite.textureRect;

			var tex_size = new Vector2(sprite.texture.width, sprite.texture.height);

			tex_rect.position /= tex_size;
			tex_rect.size /= tex_size;

			EditorGUI.DrawRect(rect, new Color(1.0f, 0.0f, 1.0f));
			GUI.DrawTextureWithTexCoords(rect, sprite.texture, tex_rect);
		}
		else
		{
			var content = new GUIContent("No Image");
			var size = EditorStyles.label.CalcSize(content);

			var pos = rect.center - size * 0.5f;

			rect.size = size;
			rect.position = pos;

			GUI.Label(rect, content, EditorStyles.label);
		}
	}

	// 绘制单条条目
	public static bool GUIOptional(bool selected, string content)
	{
		return EditorGUILayout.Toggle(selected, EditorStyles.miniButton);
		// var pos = EditorGUILayout.GetControlRect();

		//Color bg_color = selected ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.2f, 0.2f, 0.2f);
		//if (Event.current.type == EventType.Repaint && pos.Contains(Event.current.mousePosition))
		//{
		//	bg_color = new Color(0.25f, 0.25f, 0.25f);
		//}
		//else if (Event.current.type == EventType.MouseDown)
		//{
		//	selected = true;
		//}

		//// 绘制条目底色
		//EditorGUI.DrawRect(pos, bg_color);
		//GUI.Label(pos, content);

		//return selected;
	}

	// 绘制条目列表
	public static void GUIOptionalList(ref Vector2 scroll_pos, ref int selected, System.Func<int, string> getter, params GUILayoutOption[] options)
	{
		var hs = GUI.skin.verticalScrollbar; // 禁用水平滚动条
		scroll_pos = EditorGUILayout.BeginScrollView(scroll_pos, GUIStyle.none, hs, options);

		int i = 0;
		for (string str = getter(i); str != null && i < 255; str = getter(++i))
		{
			if (MiscUtils.GUIOptional(i == selected, str))
				selected = i;
		}

		// 列表为空时, 绘制占位符None
		if (i == 0)
		{
			MiscUtils.GUIOptional(true, "None");
		}

		EditorGUILayout.EndScrollView();
	}	
	
	// 朝向编辑器视图
	[MenuItem("MiscUtils/Towards editor view")]
	public static void TowardsEditorView()
	{
		var itor = MiscUtils.GetSelectedComponentsByOrder<Transform>();
		var trs = new List<Transform>(itor);

		Undo.RecordObjects(trs.ToArray(), "Spacing Spacing YAxis");

		var camera = SceneView.lastActiveSceneView.camera;
		foreach (var tr in trs)
		{
			var dir = (camera.transform.position - tr.position).normalized;
			tr.forward = dir;
		}
	}

	// 编辑器快速操作
	// 不可用, 待修复
	// 对当前选中的在编辑器中的GameObject的子对象进行反向排序
	// [MenuItem("MiscUtils/Reverse Selected GameObjects Child")]
	private static void ReverseSelectedChild()
	{
		var itor = MiscUtils.GetSelectedComponentsByOrder<Transform>();
		var result = new List<Transform>(itor);

		// 储存位置方便撤销
		List<Transform> childs = new List<Transform>();
		foreach (var tr in result)
		{
			foreach (Transform child in tr)
				childs.Add(child);
		}
		Undo.RecordObjects(childs.ToArray(), "Spacing Spacing YAxis");

		foreach (var tr in result)
		{
			for (int i = 0; i < tr.childCount; i++)
				tr.GetChild(0).SetAsLastSibling();
		}
	}

	// 编辑器快速操作
	// 首先在选中的所有对象中计算ymin和ymax, 再进行均匀分布
	[MenuItem("MiscUtils/Spacing y axis")]
	public static void SpacingYAxis()
	{
		var itor = MiscUtils.GetSelectedComponentsByOrder<Transform>();
		var result = new List<Transform>(itor);

		if (result.Count <= 1) return;

		// 储存位置方便撤销
		Undo.RecordObjects(result.ToArray(), "Spacing Spacing YAxis");

		float yMax = float.MinValue;
		float yMin = float.MaxValue;

		foreach (var i in result)
		{
			var pos = i.transform.localPosition;
			yMax = Mathf.Max(pos.y, yMax);
			yMin = Mathf.Min(pos.y, yMin);
		}

		var yDelta = (yMax - yMin) / (result.Count - 1);

		for (int i = 0; i < result.Count; i++)
		{
			var pos = result[i].transform.localPosition;
			pos.y = yDelta * i + yMin;

			result[i].transform.localPosition = pos;
		}
	}
#endif
}

//public static int GetSelected<T>(ref List<T> results)
//	where T : UnityEngine.Object
//{
//	int count = results.Count;
//	foreach (var obj in UnityEditor.Selection.objects)
//	{
//		if (obj is T go)
//			results.Add(go);
//	}
//	return results.Count - count;
//}
//public static T GetSelected<T>()
//	where T : UnityEngine.Object
//{
//	List<T> results = new List<T>();
//	int count = GetSelected(ref results);

//	if (count == 0) return null;
//	else return results[results.Count - 1];
//}
