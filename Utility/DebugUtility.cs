/*urada 2023/5/29*/

#define DEBUG_UTILITY_DEFINED

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

public static class DebugUtility
{
	[Conditional("UNITY_EDITOR")]
	public static void DrawMark(Vector3 pos, DrawParam? args = null)
	{
		var param = args ?? DrawParam.Default;

		// 根据当前编辑器视图缩放比例来计算尺寸
		float scale = HandleUtility.GetHandleSize(pos);
		param.size *= scale;

		Vector3 xOffset = param.size * 0.5f * Vector3.right;
		Vector3 yOffset = param.size * 0.5f * Vector3.up;
		Vector3 zOffset = param.size * 0.5f * Vector3.forward;

		DebugUtility.DrawLine(pos - xOffset, pos + xOffset, args);
		DebugUtility.DrawLine(pos - yOffset, pos + yOffset, args);
		DebugUtility.DrawLine(pos - zOffset, pos + zOffset, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawArrow(Vector2 pos, Vector2 vec, DrawParam? args = null)
	{
		const float ArrowHeadAngle = 15.0f;

		var param = args ?? DrawParam.Default;

		DebugUtility.DrawRay(pos, vec, args);

		var left = (vec * -1).normalized;
		var right = (vec * -1).normalized;

		left = Quaternion.AngleAxis(Vector2.Angle(vec, Vector2.zero) + ArrowHeadAngle, Vector3.forward) * left;
		right = Quaternion.AngleAxis(Vector2.Angle(vec, Vector2.zero) - ArrowHeadAngle, Vector3.forward) * right;

		float scale = MiscUtils.GizmoScale(pos, SceneView.lastActiveSceneView.camera);
		param.size *= scale;

		DebugUtility.DrawRay(pos + vec, left * param.size, args);
		DebugUtility.DrawRay(pos + vec, right * param.size, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawArrowBetween(Vector2 start, Vector2 target, DrawParam? args = null)
	{
		var vec = target - start;
		DebugUtility.DrawArrow(start, vec, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawLine(Vector3 a, Vector3 b, DrawParam? args = null)
	{
		var param = args ?? DrawParam.Default;
		UnityEngine.Debug.DrawLine(a, b, param.color, param.duration, param.deepTest);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawRay(Vector2 pos, Vector2 vec, DrawParam? args = null)
	{
		var param = args ?? DrawParam.Default;
		UnityEngine.Debug.DrawRay(pos, vec, param.color, param.duration, param.deepTest);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawLines(IEnumerable<Vector3> points, bool isClosed = false, bool markPoint = false, DrawParam? args = null)
	{
		bool recordFirst = false;
		Vector3 first = default;

		Vector3 previous = default;

		foreach (var cur in points)
		{
			if (markPoint)
				DebugUtility.DrawMark(cur, args);

			if (!recordFirst)
			{
				first = cur;
				recordFirst = true;
			}
			else
				DebugUtility.DrawLine(previous, cur, args);
			
			previous = cur;
		}

		if (isClosed)
			DebugUtility.DrawLine(previous, first, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawBox(Vector2 pos, Vector2 size, float angle = 0, DrawParam? args = null)
	{
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

		DebugUtility.DrawLine(p1, p2, args);
		DebugUtility.DrawLine(p2, p3, args);
		DebugUtility.DrawLine(p3, p4, args);
		DebugUtility.DrawLine(p4, p1, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawRange(Vector2 min, Vector2 max, DrawParam? args = null)
	{
		DrawBox(Vector2.Lerp(min, max, 0.5f), (max - min), 0, args);

		DrawMark(min, args);
		DrawMark(max, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawCircle(Vector2 centre, float r, int sample = 32, DrawParam? args = null)
	{
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
				first = cur; // 记录第一个采样点
			else if (i == sample) // 最后一次绘制时连接最后一个采样点与第一个采样点
				DebugUtility.DrawLine(first, previous, args);
			else
				DebugUtility.DrawLine(previous, cur, args);

			previous = cur;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawBoxCast(Vector2 pos, Vector2 size, float angle, Vector2 dir, float dist, DrawParam? args = null)
	{
		var castPos = pos + dir.normalized * dist;

		DebugUtility.DrawBox(pos, size, angle, args); // 起始碰撞盒
		DebugUtility.DrawBox(castPos, size, angle, args); // 目标碰撞盒

		DebugUtility.DrawArrow(pos, castPos, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawCurve(System.Func<float, Vector2> curve, int sample, bool isClosed = false, DrawParam? args = null)
	{
		Vector2 first = default;
		Vector2 previous = default;

		for (int i = 0; i <= sample; i++)
		{
			float t = Mathf.Clamp01(((float)i) / sample);

			Vector2 cur = curve(t);

			if (i == 0)
				first = cur; // 记录第一个采样点
			else if (i == sample && isClosed)
				// 如果是封闭曲线, 连接末尾点到起点
				DebugUtility.DrawLine(previous, first, args);
			else
				DebugUtility.DrawLine(previous, cur, args);

			previous = cur;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawNormalizedCurveTo(System.Func<float, float> normalizedCurve, int sample, Vector2 max, Vector2 min, bool isClosed = false, DrawParam? args = null)
	{
		Vector2 first = default;
		Vector2 previous = default;

		for (int i = 0; i <= sample; i++)
		{
			// 计算归一点并将其映射到范围中
			float t = Mathf.Clamp01(((float)i) / sample);
			var r = normalizedCurve(t); // y值

			var x = Mathf.LerpUnclamped(min.x, max.x, t);
			var y = Mathf.LerpUnclamped(min.y, max.y, r);

			var cur = new Vector2(x, y);

			if (i == 0)
				first = cur; // 记录第一个采样点
			else if (i == sample && isClosed)
				// 如果是封闭曲线, 连接末尾点到起点
				DebugUtility.DrawLine(previous, first, args);
			else
				DebugUtility.DrawLine(previous, cur, args);

			previous = cur;
		}
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