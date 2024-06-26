using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Urd.Common
{
	public struct Result
	{
		public static Result Success => new Result { success = true, code = 0, info = "success" };

		public bool success;
		public int code;
		public string info;

		public Result(bool success, string info, int code = 0)
		{
			this.success = success;
			this.code = code;
			this.info = info;
		}
	}

	public static class MiscUtils
	{
		public static Color ClearAlpha { get => new Color(1, 1, 1, 0); }

		/// <summary>
		/// 设置rectTransform的pivot
		/// 设置会确保rectTransform不变形
		/// </summary>
		public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
		{
			Vector3 deltaPosition = rectTransform.pivot - pivot;
			deltaPosition.Scale(rectTransform.rect.size);
			deltaPosition.Scale(rectTransform.localScale);
			deltaPosition = rectTransform.rotation * deltaPosition;

			rectTransform.pivot = pivot;
			rectTransform.localPosition -= deltaPosition;
		}

		/// <summary>
		/// 节拍器, 按frequency指示的频率返回布尔值
		/// -1时始终返回true
		/// 0时始终返回false
		/// 由t设置时间基准, 一般是Time.time
		/// </summary>
		/// <param name="t">基准时间, 小于0时默认使用Time.time</param>
		/// <returns></returns>
		public static bool Metronome(int frequency, float t = -1)
		{
			switch (frequency)
			{
			case -1: return true;
			case 0: return false;
			}

			if (t < 0) t = Time.time;

			return (int)(t * 2 * frequency) % 2 == 0;
		}

		/// <summary>
		/// 尝试从<item>folderPath/basename.cfg</item>加载配置并覆盖到obj上
		/// 加载成功时加载的配置将被覆盖到obj上并返回
		/// 若文件不存在且createDefaultFromInput为true则尝试使用输入的obj在该位置创建默认的配置文件
		/// 使用JsonUtility对T进行序列化/反序列化, T必须被JsonUtility支持且为class
		/// /// </summary>
		public static Result LoadConfig<T>(
			string basename,
			T obj,
			bool createDefault = true,
			string folderPath = null,
			bool ignoreInEditor = true) where T : class
		{
			const string DefaultFolderPath = "/config/";

#if UNITY_EDITOR
			if (ignoreInEditor) 
				return Result.Success;
#endif

			folderPath = folderPath ?? (Application.streamingAssetsPath + DefaultFolderPath);
			var fullpath = System.IO.Path.Combine(folderPath, basename + ".cfg");

			try
			{
				System.IO.Directory.CreateDirectory(folderPath);

				if (System.IO.File.Exists(fullpath))
				{
					var jsonstr = System.IO.File.ReadAllText(fullpath);
					JsonUtility.FromJsonOverwrite(jsonstr, obj);

					Debug.Log($"Load {obj.GetType().Name} config complete, from {fullpath}");

					return Result.Success;
				}
				else if (createDefault)
				{
					using (var file = System.IO.File.Create(fullpath))
					{
						var configJson = JsonUtility.ToJson(obj);

						// 将文本使用utf编码并写入
						var bytes = System.Text.Encoding.UTF8.GetBytes(configJson);
						file.Write(bytes, 0, bytes.Length);
						file.Flush();

						file.Close();
					}

					var info = $"Create {obj.GetType().Name} default config, write to {fullpath}";
					Debug.LogWarning(info);

					return new Result(true, info);
				}
			}
			catch (System.Exception exc)
			{
				var info = $"Load {obj.GetType().Name} config faild: {exc}, from {fullpath}";
				
				Debug.LogError(info);
				return new Result(false, info, -1);
			}

			return new Result(false, "unknow", -2);
		}

		public static Result SaveConfig<T>(string basename, T obj, string folderPath = null)
		{
			const string DefaultFolderPath = "/config/";

			folderPath = folderPath ?? (Application.streamingAssetsPath + DefaultFolderPath);
			var fullpath = System.IO.Path.Combine(folderPath, basename + ".cfg");

			try
			{
				string directoryPath = System.IO.Path.GetDirectoryName(fullpath);
				if (!System.IO.Directory.Exists(directoryPath))
				{
					System.IO.Directory.CreateDirectory(directoryPath);
					Debug.Log("Created config directory: " + directoryPath);
				}

				if (!System.IO.File.Exists(fullpath))
				{
					System.IO.File.Create(fullpath);
					Debug.Log($"Config file does not exist, will create : {fullpath}");
				}

				var configJson = JsonUtility.ToJson(obj);

				// 将文本使用utf编码并写入
				var bytes = System.Text.Encoding.UTF8.GetBytes(configJson);

				System.IO.File.WriteAllBytes(fullpath, bytes);
				Debug.Log($"Config written to file successfully: {fullpath}");

				return new Result(true, "");
			}
			catch (System.Exception exc)
			{
				var info = $"Save {obj.GetType().Name} config faild: {exc}, from {fullpath}";

				Debug.LogError(info);
				return new Result(false, info, -1);
			}
		}

		/// <summary>
		/// 围绕pivot旋转angle度
		/// angle为目标的localEulerAngles.z
		/// </summary>
		public static void RotateAround(Transform tr, Vector2 pivot, float degAngle)
		{
			var curAngle = tr.localEulerAngles.z;
			var angleDelta = Mathf.DeltaAngle(curAngle, degAngle);

			var rotatePivot = tr.TransformPoint(pivot);
			tr.RotateAround(rotatePivot, Vector3.forward, angleDelta);
		}
		public static void RotateAroundGlobal(Transform tr, Vector2 pivot, float degAngle)
		{
			var curAngle = tr.eulerAngles.z;
			var angleDelta = Mathf.DeltaAngle(curAngle, degAngle);
			tr.RotateAround(pivot, Vector3.forward, angleDelta);
		}

		/// <summary>
		/// 无视pivot计算当前RectTransform在父RectTransform中的位置
		/// 注意, 此值将会受到锚点的影响, 视锚点为计算的中心点
		/// </summary>
		public static void CalculateMaxMin(this RectTransform tr, out Vector2 max, out Vector2 min)
		{
			Debug.Assert(tr.anchorMax == tr.anchorMin, $"{tr.name}");

			var halfSizeDelta = tr.sizeDelta * 0.5f;

			//float x = Mathf.Lerp(-1, 1, tr.pivot.x) * halfSizeDelta.x;
			//float y = Mathf.Lerp(-1, 1, tr.pivot.y) * halfSizeDelta.y;

			// var pivotOffset = new Vector2(x, y);
			var center = (Vector2)tr.localPosition; // tr.anchoredPosition - pivotOffset;
			max = center + halfSizeDelta;
			min = center - halfSizeDelta;
		}

		public static bool InLayer(this GameObject go, string layer)
			=> InLayer(go, LayerMask.GetMask(layer));
		public static bool InLayer(this GameObject go, LayerMask layermask)
			=> layermask == (layermask | (1 << go.layer));

		public static void Required<ComT>(this Component obj, out ComT com)
			where ComT : Component
		{
			if (!obj.TryGetComponent(out com))
				com = obj.gameObject.AddComponent<ComT>();
		}
		public static ComT Required<ComT>(this Component obj)
			where ComT : Component
		{
			if (!obj.TryGetComponent(out ComT com))
				com = obj.gameObject.AddComponent<ComT>();
			return com;
		}

		/// <summary>
		/// 从索引偏移获取同一父级下的子对象
		/// loopIndex确保索引能进行循环
		/// </summary>
		public static Transform GetSiblingFromOffset(this Transform tr, int indexOffset, bool loopIndex = false)
		{
			var index = tr.GetSiblingIndex() + indexOffset;
			if (loopIndex) index %= tr.parent.childCount;

			return tr.parent.GetChild(index);
		}

		public static Color InvertColor(Color c)
		{
			return new Color(1.0f - c.r, 1.0f - c.g, 1.0f - c.b);
		}

		public static bool RandomBool()
		{
			return Random.value > 0.5f;
		}

		public static Sprite CreateSprite(Texture2D tex, int pixelsPerUnit)
		{
			var rect = new Rect(position: Vector2.zero, tex.texelSize);
			var pivot = new Vector2(0.5f, 0.5f);
			return Sprite.Create(tex, rect, pivot, pixelsPerUnit);
		}

		public static Texture2D CreateTexture(int w, int h, Color? color = null, bool temporary = true)
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

		public static Texture2D CreateSmallColorTexture(Color color)
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

		//// 待修复 rect.position指示左上角而不是中间
		//public static Rect GetCameraViewport(Camera camera)
		//{
		//	var y = camera.orthographicSize * 2;
		//	var x = y * camera.aspect;

		//	var pos = (Vector2)camera.transform.position;

		//	return new Rect(position: pos, size: new Vector2(x, y));
		//}

		// 在保持与目标距离的同时以forward一面朝向目标
		public static Vector3 Alignment(Vector3 self, Vector3 target, Vector3 forward)
		{
			var dist = (self - target).magnitude;
			var dir = forward;

			return target - dir * dist;
		}

		public static IEnumerable<T> Foreach<T>(this IEnumerable<T> src, System.Action<T> fun)
		{
			foreach (var item in src)
			{
				fun.Invoke(item);
				yield return item;
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
					/+\ degAngle=fov
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
					/+\ degAngle=fov
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

		public static float EditorGizmoScale(Vector3 position)
		{
#if UNITY_EDITOR
			position = Gizmos.matrix.MultiplyPoint(position);

			Camera camera = null;
			if (SceneView.lastActiveSceneView)
				camera = SceneView.lastActiveSceneView.camera;
			else if (SceneView.currentDrawingSceneView)
				camera = SceneView.currentDrawingSceneView.camera;

			if (camera)
			{
				Transform transform = camera.transform;
				Vector3 position2 = transform.position;
				float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
				Vector3 a = camera.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
				Vector3 b = camera.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
				float magnitude = (a - b).magnitude;

				float factor = 80f / Mathf.Max(magnitude, 0.0001f);
				factor = Mathf.Clamp(factor, 0.2f, 2.0f); // 启动时该值显示80000, 正常应该在1上下
				return factor;
			}
#endif
			return 0.7f;
		}

#if UNITY_EDITOR

		/// <summary>
		/// 按顺序获取被选中的GameObject
		/// </summary>
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
					if (filter != null && !filter.Invoke(go))
						continue;

					yield return go;
				}
			}
		}

		/// <summary>
		/// 按顺序遍历被选中对象且尝试获取目标组件并返回
		/// </summary>
		public static IEnumerable<T> GetSelectedComponentsByOrder<T>(bool inScene = true)
			where T : Component
		{
			foreach (var go in GetSelectedGameObjectsByOrder(inScene))
			{
				if (go.TryGetComponent(out T com))
					yield return com;
			}
		}

		/// <summary>
		/// 按顺序获取被选中的任意对象, 必须继承自UnityEngine.Object
		/// 使用过滤器进行过滤
		/// </summary>
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
		[MenuItem("Function/MiscUtils/Towards Editor View")]
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
		//private static void ReverseSelectedChild()
		//{
		//	var itor = MiscUtils.GetSelectedComponentsByOrder<Transform>();
		//	var result = new List<Transform>(itor);

		//	// 储存位置方便撤销
		//	List<Transform> childs = new List<Transform>();
		//	foreach (var tr in result)
		//	{
		//		foreach (Transform child in tr)
		//			childs.Add(child);
		//	}
		//	Undo.RecordObjects(childs.ToArray(), "Spacing Spacing YAxis");

		//	foreach (var tr in result)
		//	{
		//		for (int i = 0; i < tr.childCount; i++)
		//			tr.GetChild(0).SetAsLastSibling();
		//	}
		//}

		// 编辑器快速操作
		// 首先在选中的所有对象中计算ymin和ymax, 再进行均匀分布
		[MenuItem("Function/MiscUtils/Spacing Y axis")]
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
}
