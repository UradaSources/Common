/*urada 2023/5/29*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MiscUtils
{
	public static bool InLayer(this GameObject go, string layer)
		=> InLayer(go, LayerMask.GetMask(layer));
	public static bool InLayer(this GameObject go, LayerMask layermask)
		=> layermask == (layermask | (1 << go.layer));

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

	public static int AppendList<T>(ref List<T> dst, IEnumerable<T> src, bool allowRepeat = false)
	{
		int count = dst.Count;
		foreach (var v in src)
		{
			if (allowRepeat || dst.IndexOf(v) < 0)
				dst.Add(v);
		}
		return dst.Count - count;
	}
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

	public static IEnumerable<T1> Process<T1, T2>(this IEnumerable<T2> src, System.Func<T2, T1> export)
	{
		foreach (var i in src)
			yield return export.Invoke(i);
	}

	public static string Connect(string space, params string[] args)
	{
		if (args.Length == 0) return "";
		if (args.Length == 1) return args[0];

		string result = "";
		for (int i = 0; i < args.Length - 1; i++)
		{
			if (!string.IsNullOrEmpty(args[i]))
				result += args[i] + space;
		}
		result += args[args.Length - 1];

		return result;
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
			var point = offset + (Vector3)MathUtility.Circle(radius, r);

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

	public static IEnumerable<T2> Process<T1, T2>(IEnumerable<T1> src, System.Func<T1, T2> handle)
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

#if UNITY_EDITOR
	// 获取当前选中的GO
	// 将保留选择顺序
	public static int GetSelectedGameObjectsByOrder(ref List<GameObject> result, bool inScene = true)
	{
		int count = result.Count;

		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			if (obj is GameObject go)
			{
				// 检查是否是场景中的对象
				if (inScene && string.IsNullOrEmpty(go.scene.name))
					continue;

				result.Add(go);
			}
		}
		return result.Count - count;
	}
	public static int GetSelectedComponentsByOrder<T>(ref List<T> result)
		where T : Component
	{
		int count = result.Count;

		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			if (obj is GameObject go && go.TryGetComponent(out T com))
				result.Add(com);
		}

		return result.Count - count;
	}

	public static int GetSelectedObjectByOrder<T>(ref List<T> result, System.Func<T, bool> checker = null)
		where T : UnityEngine.Object
	{
		int count = result.Count;

		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			// 只处理场景中的对象
			if (obj is T to && (checker == null || checker.Invoke(to)))
				result.Add(to);
		}

		return result.Count - count;
	}

	public static GameObject GetSelectedGameObjectInScene()
	{
		var selected = Selection.objects;
		foreach (var obj in selected)
		{
			// 只处理场景中的对象
			if (obj is GameObject go && !string.IsNullOrEmpty(go.scene.name))
				return go;
		}
		return null;
	}

	public static float GetGizmoSize(Vector3 position, Camera camera = null)
	{
		camera = camera ?? Camera.current;
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
