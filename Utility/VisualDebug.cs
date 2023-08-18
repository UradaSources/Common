using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace urd.common.utils
{
	// 绘图数据
	[System.Serializable]
	public struct DrawData
	{
		[SerializeField] public Vector3 start;
		[SerializeField] public Vector3 end;

		[SerializeField] public bool is_2d;

		[SerializeField] public Color color;
		[SerializeField] public float duration;
		[SerializeField] public bool deep_test;
	}

	// 绘图数据构建器
	public class DrawDataBuilder
	{
		private bool m_building;
		private bool m_is_2d;
		private List<(Vector3 start, Vector3 end)> m_lines;

		public Color color;
		public float duration;
		public bool deep_test;

		// 开始构建新数据
		public void begin(bool is_2d, bool reset_style = true)
		{
			Debug.Assert(!m_building);
			m_is_2d = is_2d;
			m_building = true;

			if (reset_style)
				this.reset();
		}
		public IEnumerable<DrawData> buildDatas()
		{
			if (m_building)
			{
				foreach (var (start, end) in m_lines)
				{
					yield return new DrawData
					{
						start = start,
						end = end,
						is_2d = m_is_2d,
						color = color,
						duration = duration,
						deep_test = deep_test
					};
				}
				m_lines.Clear();
				m_building = false;
			}
		}

		public DrawDataBuilder line(Vector2 start, Vector2 end)
		{
			Debug.Assert(m_building);
			m_lines.Add((start, end));
			return this;
		}
		public DrawDataBuilder line(Vector3 start, Vector3 end)
		{
			Debug.Assert(m_building);
			m_lines.Add((start, end));
			return this;
		}

		public DrawDataBuilder ray(Vector2 start, Vector2 dir)
		{
			Debug.Assert(m_building);
			m_lines.Add((start, start + dir));
			return this;
		}
		public DrawDataBuilder ray(Vector3 start, Vector3 dir)
		{
			Debug.Assert(m_building);
			m_lines.Add((start, start + dir));
			return this;
		}

		public DrawDataBuilder reset()
		{
			Debug.Assert(m_building);
			this.color = Color.white;
			this.duration = 0;
			this.deep_test = false;
			return this;
		}
		public DrawDataBuilder set(Color color, float duration, bool deep_test)
		{
			Debug.Assert(m_building);
			this.color = color;
			this.duration = duration;
			this.deep_test = deep_test;
			return this;
		}
		public DrawDataBuilder setColor(Color color)
		{
			Debug.Assert(m_building);
			this.color = color;
			return this;
		}
		public DrawDataBuilder setDuration(float duration)
		{
			Debug.Assert(m_building);
			this.duration = duration;
			return this;
		}
		public DrawDataBuilder setDeepTest(bool deep_test)
		{
			Debug.Assert(m_building);
			this.deep_test = deep_test;
			return this;
		}

		public DrawDataBuilder()
		{
			m_lines = new List<(Vector3 start, Vector3 end)>();
		}
	}

	[DefaultExecutionOrder(100), ExecuteAlways]
	public class VisualDebug : MonoBehaviour
	{
		public const float MarkSize = 0.1f;
		public const float ArraySize = 0.2f;

		const float ArrowHeadAngle = 15.0f;

		public static float GizmoScale(Vector3 position, Camera camera)
		{
			position = Gizmos.matrix.MultiplyPoint(position);
			Transform transform = camera.transform;
			Vector3 position2 = transform.position;
			float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
			Vector3 a = camera.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
			Vector3 b = camera.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
			float magnitude = (a - b).magnitude;
			return 80f / Mathf.Max(magnitude, 0.0001f);
		}
		public static float EditorGizmoScale(Vector3 position)
		{
			var editorView = SceneView.lastActiveSceneView;
			if (editorView)
				return Mathf.Min(GizmoScale(position, editorView.camera), GizmoScale(position, Camera.main));
			else
				return GizmoScale(position, Camera.main);
		}

		private List<DrawData> m_draw_queue = new List<DrawData>(20);
		private DrawDataBuilder m_data_builder = new DrawDataBuilder();

		public void drawLater(DrawData data)
		{
			m_draw_queue.Add(data);
		}
		public void drawLater(IEnumerable<DrawData> data)
		{
			m_draw_queue.AddRange(data);
		}

		public DrawDataBuilder line(Vector3 start, Vector3 end)
		{
			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: false);
			m_data_builder.line(start, end);
			return m_data_builder;
		}
		public DrawDataBuilder line(Vector2 start, Vector2 end)
		{
			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: false);
			m_data_builder.line(start, end);
			return m_data_builder;
		}

		public DrawDataBuilder ray(Vector3 start, Vector3 end)
			=> this.line(start, start + end);
		public DrawDataBuilder ray(Vector2 start, Vector2 end)
			=> this.line(start, start + end);

		public DrawDataBuilder mark(Vector3 pos, Transform tr = null)
		{
			if (tr) pos = tr.TransformPoint(pos);

			// 根据当前编辑器视图缩放比例来计算尺寸
			var scale = EditorGizmoScale(pos);
			var half_size = Mathf.Max(scale * MarkSize, MarkSize) * 0.5f;

			var x_offset = new Vector3(half_size, 0, 0);
			var y_offset = new Vector3(0, half_size, 0);
			var z_offset = new Vector3(0, 0, half_size);

			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: false);
			m_data_builder.line(pos - x_offset, pos + x_offset);
			m_data_builder.line(pos - y_offset, pos + y_offset);
			m_data_builder.line(pos - z_offset, pos + z_offset);
			return m_data_builder;
		}

		public DrawDataBuilder arrow(Vector3 pos, Vector3 dir)
		{
			var end = pos + dir;
			var left = (dir * -1).normalized;
			var right = (dir * -1).normalized;

			left = Quaternion.AngleAxis(Vector2.Angle(dir, Vector2.zero) + ArrowHeadAngle, Vector3.forward) * left;
			right = Quaternion.AngleAxis(Vector2.Angle(dir, Vector2.zero) - ArrowHeadAngle, Vector3.forward) * right;

			var scale = EditorGizmoScale(pos);
			var size = Mathf.Max(scale * ArraySize, ArraySize);

			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: false);
			m_data_builder.line(pos, end);
			m_data_builder.ray(end, left * size);
			m_data_builder.ray(end, right * size);
			return m_data_builder;
		}

		public DrawDataBuilder arrowBetween(Vector3 start, Vector3 target)
			=> this.arrow(start, target - start);

		public DrawDataBuilder lines(IEnumerable<Vector3> points, bool is_closed = false, bool mark_point = false)
		{
			bool record_first = false;
			Vector3 first = default;

			Vector3 previous = default;

			// 先绘制标点集合
			if (mark_point)
			{
				foreach (var point in points)
					this.mark(point);
			}

			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: false);

			foreach (var cur in points)
			{
				if (!record_first)
				{
					first = cur;
					record_first = true;
				}
				else
					m_data_builder.line(previous, cur);

				previous = cur;
			}

			if (is_closed)
				m_data_builder.line(previous, first);

			return m_data_builder;
		}
		public DrawDataBuilder lines(IEnumerable<Vector2> points, bool is_closed = false, bool mark_point = false)
		{
			bool record_first = false;
			Vector2 first = default;

			Vector2 previous = default;

			// 先绘制标点集合
			if (mark_point)
			{
				foreach (var point in points)
					this.mark(point);
			}

			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: true);

			foreach (var cur in points)
			{
				if (!record_first)
				{
					first = cur;
					record_first = true;
				}
				else
					m_data_builder.line(previous, cur);

				previous = cur;
			}

			if (is_closed)
				m_data_builder.line(previous, first);

			return m_data_builder;
		}

		public DrawDataBuilder box(Vector2 center, Vector2 size, float angle = 0, bool mark_corners = false)
		{
			float w = size.x * 0.5f;
			float h = size.y * 0.5f;

			var p1 = center + new Vector2(-w, h);
			var p2 = center + new Vector2(w, h);
			var p3 = center + new Vector2(w, -h);
			var p4 = center + new Vector2(-w, -h);

			if (!Mathf.Approximately(angle, 0))
			{
				Quaternion q = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));
				p1 = q * p1;
				p2 = q * p2;
				p3 = q * p3;
				p4 = q * p4;
			}

			if (mark_corners)
			{
				this.mark(p1);
				this.mark(p2);
				this.mark(p3);
				this.mark(p4);
			}

			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: true);
			m_data_builder.line(p1, p2);
			m_data_builder.line(p2, p3);
			m_data_builder.line(p3, p4);
			m_data_builder.line(p4, p1);
			return m_data_builder;
		}
		public DrawDataBuilder box(Rect rect, float angle = 0, bool mark_corners = false)
			=> this.box(rect.center, rect.size, angle, mark_corners);

		public DrawDataBuilder boxMaxMin(Vector2 max, Vector2 min)
		{
			this.mark(min);
			this.mark(max);
			return this.box(Vector2.Lerp(min, max, 0.5f), (max - min));
		}

		public DrawDataBuilder circle(Vector2 center, float r, int sample_num = 32, bool mark_center = false)
		{
			Vector2 first = default;
			Vector2 previous = default;

			if (mark_center)
				this.mark(center);

			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: true);

			// [0, 2PI]
			for (int i = 0; i <= sample_num; i++)
			{
				float radian = (2.0f * Mathf.PI) * (((float)i) / sample_num);

				Vector2 cur = new Vector2(
					r * Mathf.Cos(radian),
					r * Mathf.Sin(radian)
				);
				cur += center;

				if (i == 0)
					first = cur; // 记录第一个采样点
				else if (i == sample_num) // 最后一次绘制时连接最后一个采样点与第一个采样点
					m_data_builder.line(first, previous);
				else
					m_data_builder.line(previous, cur);
				previous = cur;
			}
			return m_data_builder;
		}

		public DrawDataBuilder sampler(System.Func<float, Vector3> sampler, int sample_num, bool is_closed = false)
		{
			Vector3 first = default;
			Vector3 previous = default;

			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: false);

			for (int i = 0; i <= sample_num; i++)
			{
				float t = Mathf.Clamp01(((float)i) / sample_num);

				Vector3 cur = sampler.Invoke(t);

				if (i == 0)
					first = cur; // 记录第一个采样点
				else if (i == sample_num && is_closed) // 如果是封闭曲线, 连接末尾点到起点
					m_data_builder.line(previous, first);
				else
					m_data_builder.line(previous, cur);

				previous = cur;
			}

			return m_data_builder;
		}
		public DrawDataBuilder sampler(System.Func<float, Vector2> sampler, int sample_num, bool is_closed = false)
		{
			Vector2 first = default;
			Vector2 previous = default;

			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: true);

			for (int i = 0; i <= sample_num; i++)
			{
				float t = Mathf.Clamp01(((float)i) / sample_num);

				Vector2 cur = sampler.Invoke(t);

				if (i == 0)
					first = cur; // 记录第一个采样点
				else if (i == sample_num && is_closed) // 如果是封闭曲线, 连接末尾点到起点
					m_data_builder.line(previous, first);
				else
					m_data_builder.line(previous, cur);

				previous = cur;
			}

			return m_data_builder;
		}

		public DrawDataBuilder curve(System.Func<float, float> func_2d, float x_min, float x_max, int sample_num, bool is_closed = false)
		{
			Vector2 first = default;
			Vector2 previous = default;

			this.drawLater(m_data_builder.buildDatas());
			m_data_builder.begin(is_2d: true);

			for (int i = 0; i <= sample_num; i++)
			{
				float t = Mathf.Clamp01(((float)i) / sample_num);
				float x = Mathf.Lerp(x_min, x_max, t);

				var cur = new Vector2(x, func_2d.Invoke(x));

				if (i == 0)
					first = cur; // 记录第一个采样点
				else if (i == sample_num && is_closed) // 如果是封闭曲线, 连接末尾点到起点
					m_data_builder.line(previous, first);
				else
					m_data_builder.line(previous, cur);

				previous = cur;
			}
			return m_data_builder;
		}

		private void LateUpdate()
		{
			// 构建器是惰性弹出的, 仅在开始新的构建前弹出数据
			// 故在执行所有绘制命令前再尝试弹出一次
			this.drawLater(m_data_builder.buildDatas());

			if (m_draw_queue.Count > 0)
			{
				foreach (var data in m_draw_queue)
				{
					//if (data.is_2d)
					//{ 
					//	// ...
					//}
					Debug.DrawLine(data.start, data.end, data.color, data.duration, data.deep_test);
				}
				// 清空绘制队列
				m_draw_queue.Clear();
			}
		}
	}
}
#endif