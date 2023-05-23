#define DEBUG_UTILITY_DEFINED
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;

public static class DebugUtility
{
	public struct DrawParam
	{
		public static readonly DrawParam Default = new DrawParam
		{
			size = 0.1f,
			color = Color.white,
			duration = 0.0f,
			deepTest = false,
		};

		public float size;
		public Color color;
		public float duration;
		public bool deepTest;

		public DrawParam(float size = 0.1f, Color? color = null, float duration = 0, bool deepTest = false)
		{
			this.size = size;
			this.color = color ?? Color.white;
			this.duration = duration;
			this.deepTest = deepTest;
		}
	}

	public static void DrawMark(Vector3 pos, DrawParam? args = null)
	{
		var param = args ?? DrawParam.Default;

		// 根据当前编辑器视图缩放比例来计算尺寸
		float scale = HandleUtility.GetHandleSize(pos);
		param.size *= scale;

		Vector3 xOffset = param.size * 0.5f * Vector3.right;
		Vector3 yOffset = param.size * 0.5f * Vector3.up;
		Vector3 zOffset = param.size * 0.5f * Vector3.forward;

		Debug.DrawLine(pos - xOffset, pos + xOffset, param.color, param.duration);
		Debug.DrawLine(pos - yOffset, pos + yOffset, param.color, param.duration);
		Debug.DrawLine(pos - zOffset, pos + zOffset, param.color, param.duration);
	}

	public static void DrawArrow(Vector2 pos, Vector2 vec, DrawParam? args = null)
	{
		const float ArrowHeadAngle = 15.0f;

		var param = args ?? DrawParam.Default;

		DebugUtility.DrawRay(pos, vec, param.color, param.duration);

		var left = (vec * -1).normalized;
		var right = (vec * -1).normalized;

		left = Quaternion.AngleAxis(Vector2.Angle(vec, Vector2.zero) + ArrowHeadAngle, Vector3.forward) * left;
		right = Quaternion.AngleAxis(Vector2.Angle(vec, Vector2.zero) - ArrowHeadAngle, Vector3.forward) * right;

		float scale = HandleUtility.GetHandleSize(pos);
		param.size *= scale;

		DebugUtility.DrawRay(pos + vec, left * param.size, param.color, param.duration);
		DebugUtility.DrawRay(pos + vec, right * param.size, param.color, param.duration);
	}
	public static void DrawArrowBetween(Vector2 start, Vector2 target, DrawParam? args = null)
	{
		var vec = target - start;
		DebugUtility.DrawArrow(start, vec, args);
	}

	public static void DrawLine(Vector3 a, Vector3 b, Color? color = null, float duration = 0, bool depthTest = false)
	{
		Debug.DrawLine(a, b, color ?? Color.white, duration, depthTest);
	}
	public static void DrawRay(Vector2 pos, Vector2 vec, Color? color = null, float duration = 0, bool depthTest = false)
	{
		Debug.DrawRay(pos, vec, color ?? Color.white, duration, depthTest);
	}
	
	public static void DrawLines(bool isClosed, float duration, Color c, IEnumerable<Vector3> points)
	{
		bool recordFirst = false;
		Vector3 first = default;

		Vector3 previous = default;

		foreach (var cur in points)
		{
			DebugUtility.DrawMark(cur, 0.01f, Color.red);

			if (!recordFirst)
			{ 
				first = cur;
				recordFirst = true;
			}
			else
				DebugUtility.DrawLine(previous, cur, c, duration);

			previous = cur;
		}

		if (isClosed) DebugUtility.DrawLine(previous, first, c, duration);
	}
	public static void DrawLines(bool isClosed, float duration, Color c, IEnumerable<Vector2> points)
	{
		bool recordFirst = false;
		Vector2 first = default;

		Vector2 previous = default;

		foreach (var cur in points)
		{
			DebugUtility.DrawMark(cur, 0.01f, Color.red);

			if (!recordFirst)
			{
				first = cur;
				recordFirst = true;
			}
			else
				DebugUtility.DrawLine(previous, cur, c, duration);

			previous = cur;
		}

		if (isClosed) DebugUtility.DrawLine(previous, first, c, duration);
	}

	//public static void DrawArrowBetween(Vector2 start, Vector2 target, float arrowHeadLength = 0.2f, float arrowHeadAngle = 15.0f, Color? color = null, float duration = 0)
	//{
	//	var vec = target - start;
	//	DebugUtility.DrawArrow(start, vec, arrowHeadLength, arrowHeadAngle, color, duration);
	//}
	public static void DrawArrow(Vector2 pos, Vector2 vec, float arrowHeadLength = 0.2f, float arrowHeadAngle = 15.0f, Color? color = null, float duration = 0)
	{
		DebugUtility.DrawRay(pos, vec, color, duration);

		var left = (vec * -1).normalized;
		var right = (vec * -1).normalized;

		left = Quaternion.AngleAxis(Vector2.Angle(vec, Vector2.zero) + arrowHeadAngle, Vector3.forward) * left;
		right = Quaternion.AngleAxis(Vector2.Angle(vec, Vector2.zero) - arrowHeadAngle, Vector3.forward) * right;

		DebugUtility.DrawRay(pos + vec, left * arrowHeadLength, color, duration);
		DebugUtility.DrawRay(pos + vec, right * arrowHeadLength, color, duration);
	}

	public static void DrawBox(Vector2 pos, Vector2 size, float angle = 0, Color? color = null, float duration = 0)
	{
		color = color ?? Color.white;

		float w = size.x * 0.5f;
		float h = size.y * 0.5f;

		var p1 = pos + new Vector2(-w, h);
		var p2 = pos + new Vector2(w, h);
		var p3 = pos + new Vector2(w, -h);
		var p4 = pos + new Vector2(-w, -h);

		if (!Mathf.Approximately(angle, 0))
		{
			Quaternion q = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));
			p1 = q * p1;
			p2 = q * p2;
			p3 = q * p3;
			p4 = q * p4;
		}

		DebugUtility.DrawLine(p1, p2, color, duration);
		DebugUtility.DrawLine(p2, p3, color, duration);
		DebugUtility.DrawLine(p3, p4, color, duration);
		DebugUtility.DrawLine(p4, p1, color, duration);
	}
	public static void DrawRange(Vector2 min, Vector2 max, Color? color = null, float duration = 0)
	{ 
		DrawBox(Vector2.Lerp(min, max, 0.5f), (max - min), 0, color, duration);

		DrawMark(min, 0.1f, color: Color.green);
		DrawMark(max, 0.1f, color: Color.red);
	}

	public static void DrawCircle(Vector2 centre, float r, int sample = 32, Color? color = null, float duration = 0)
	{
		color = color ?? Color.white;

		Vector2 first = default;
		Vector2 previous = default;

		// [0, 2PI]
		for (int i = 0; i <= sample; i++)
		{
			float radian = (2.0f * Mathf.PI) * (((float)i) / sample);

			Vector2 cur = new Vector2(
				r * Mathf.Cos(radian),
				r * Mathf.Sin(radian)
			);
			cur += centre;

			if (i == 0)
			{
				first = cur; // 记录第一个采样点
			}
			else if (i == sample) // 最后一次绘制时不再进行采样, 连接最后一个采样点与第一个采样点封闭图形
			{
				DebugUtility.DrawLine(first, previous, color, duration);
			}
			else
			{
				DebugUtility.DrawLine(previous, cur, color, duration);
			}

			previous = cur;
		}
	}

	public static void DrawMark(Vector3 pos, float size, Color? color = null, float duration = 0)
	{
		color = color ?? Color.white;

		float half = size * 0.5f;

		DebugUtility.DrawLine(pos + Vector3.left * half, pos + Vector3.right * half, color, duration);
		DebugUtility.DrawLine(pos + Vector3.down * half, pos + Vector3.up * half, color, duration);
		DebugUtility.DrawLine(pos + Vector3.back * half, pos + Vector3.forward * half, color, duration);

		// 一个小斜线
		float diagonalFactor = 0.2f;

		var p1 = pos + new Vector3(-1, 1, 1) * half * diagonalFactor;
		var p2 = pos + new Vector3(1, -1, 1) * half * diagonalFactor;
		DebugUtility.DrawLine(p1, p2, color, duration);

		var p3 = pos + new Vector3(1, 1, -1) * half * diagonalFactor;
		var p4 = pos + new Vector3(-1, 1, 1) * half * diagonalFactor;
		DebugUtility.DrawLine(p3, p4, color, duration);
	}

	public static void DrawBoxCast(Vector2 pos, Vector2 size, float angle, Vector2 dir, float dist)
	{
		DebugUtility.DrawBox(pos, size, angle, Color.green); // 起始碰撞盒
		DebugUtility.DrawBox(pos + dir.normalized * dist, size, angle, Color.red); // 目标碰撞盒
	}

	public static float DrawCurve(System.Func<float, Vector2> normalizedCurve, int sample, bool isClosed = false)
	{
		float length = 0;

		Vector2 first = default;
		Vector2 previous = default;

		for (int i = 0; i <= sample; i++)
		{
			float t = Mathf.Clamp01(((float)i) / sample);

			Vector2 cur = normalizedCurve(t);

			if (i == 0)
			{
				first = cur; // 记录第一个采样点
			}
			else if (i == sample && isClosed)
			{
				// 如果是封闭曲线, 连接末尾点到起点
				DebugUtility.DrawLine(previous, first);
			}
			else
			{
				DebugUtility.DrawLine(previous, cur);
			}

			previous = cur;
		}

		return length;
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

	//public static void DrawMesgOnScreen(Vector2 screenPos, string richMesg)
	//{
	//	var pos = Camera.main.ScreenToWorldPoint(screenPos);
	//	DebugUtility.DrawMesg(pos, richMesg);
	//}
	//public static void DrawMesg(Vector2 pos, string richMesg)
	//{
	//	var style = EditorStyles.label;
	//	style.richText = true;

	//	Handles.Label(pos, richMesg, style);
	//}
}