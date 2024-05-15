using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Urd.Common;

#if UNITY_EDITOR
using UnityEditor;
#endif

public struct DrawArgs
{
	public static implicit operator DrawArgs(Color c)
		=> new DrawArgs(color: c);

	public static DrawArgs Default => new DrawArgs(
		size: 0.1f,
		color: Color.white,
		duration: 0.0f,
		deepTest: false
	);

	public float size;
	public Color color;
	public float duration;
	public bool deepTest;

	public DrawArgs(float size = 0.1f, Color? color = null, float duration = 0, bool deepTest = false)
	{
		this.size = size;
		this.color = color ?? Color.white;
		this.duration = duration;
		this.deepTest = deepTest;
	}
}

public static class D
{
	public static bool UseGizmosBackend;

	[Conditional("UNITY_EDITOR")]
	public static void Line(Vector3 a, Vector3 b, DrawArgs? args = null)
	{
		var param = args ?? DrawArgs.Default;

		if (!UseGizmosBackend)
			UnityEngine.Debug.DrawLine(a, b, param.color, param.duration, param.deepTest);
		else
		{
			var oldColor = Gizmos.color;
			
			Gizmos.color = param.color;
			Gizmos.DrawLine(a, b);

			Gizmos.color = oldColor;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void Ray(Vector3 pos, Vector3 dir, DrawArgs? args = null)
	{
		Line(pos, pos + dir, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void SymmetricLine(Vector3 center, Vector3 dir, DrawArgs? args = null)
	{
		var pos = center - dir * 0.5f;
		Ray(pos, dir, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Mark(Vector3 pos, DrawArgs? args = null)
	{
		var param = args ?? DrawArgs.Default;

		// 根据当前编辑器视图缩放比例来计算尺寸
		var scale = MiscUtils.EditorGizmoScale(pos);
		var size = param.size * scale;

		Vector3 xOffset = size * 0.5f * Vector3.right;
		Vector3 yOffset = size * 0.5f * Vector3.up;
		Vector3 zOffset = size * 0.5f * Vector3.forward;

		D.Line(pos - xOffset, pos + xOffset, args);
		D.Line(pos - yOffset, pos + yOffset, args);
		D.Line(pos - zOffset, pos + zOffset, args);

		D.Rect(pos, Vector2.one * size, 0, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Arrow(Vector3 pos, Vector3 vec, DrawArgs? args = null)
	{
		const float HeadAngle = 15.0f;
		const float HeadLen = 0.1f;

		CustomArrow(pos, vec, HeadLen, HeadAngle, true, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void CustomArrow(Vector3 pos, Vector3 vec, float arrowLen, float angle, bool scale, DrawArgs? args = null)
	{
		D.Ray(pos, vec, args);

		var left = (vec * -1).normalized;
		var right = (vec * -1).normalized;

		left = Quaternion.AngleAxis(Vector2.Angle(vec, Vector2.zero) + angle, Vector3.forward) * left;
		right = Quaternion.AngleAxis(Vector2.Angle(vec, Vector2.zero) - angle, Vector3.forward) * right;

		var size = arrowLen;
		if (scale) size *= MiscUtils.EditorGizmoScale(pos);

		D.Ray(pos + vec, left * size, args);
		D.Ray(pos + vec, right * size, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void ArrowBetween(Vector3 start, Vector3 target, DrawArgs? args = null)
	{
		var vec = target - start;
		D.Arrow(start, vec, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Points(IEnumerable<Vector3> points, bool isClosed = false, bool markPoint = false, DrawArgs? args = null)
	{
		bool recordFirst = false;
		Vector3 first = default;

		Vector3 previous = default;

		foreach (var cur in points)
		{
			if (markPoint)
				D.Mark(cur, args);

			if (!recordFirst)
			{
				first = cur;
				recordFirst = true;
			}
			else
				D.Line(previous, cur, args);

			previous = cur;
		}

		if (isClosed)
			D.Line(previous, first, args);
	}

	public static void MarkPoints(IEnumerable<Vector3> points, bool isClosed = false, DrawArgs? args = null)
	{
		bool recordFirst = false;
		Vector3 first = default;

		Vector3 previous = default;

		foreach (var cur in points)
		{
			if (!recordFirst)
			{
				var ag = args ?? new DrawArgs();
				ag.color = Color.red;

				D.Mark(cur, ag);
			}
			else
				D.Mark(cur, args);

			if (!recordFirst)
			{
				first = cur;
				recordFirst = true;
			}
			else
				D.ArrowBetween(previous, cur, args);

			previous = cur;
		}

		if (isClosed)
			D.ArrowBetween(previous, first, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Rect(Vector2 center, Vector2 size, float angle = 0, DrawArgs? args = null)
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

		D.Line(p1, p2, args);
		D.Line(p2, p3, args);
		D.Line(p3, p4, args);
		D.Line(p4, p1, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Range(Vector2 min, Vector2 max, DrawArgs? args = null)
	{
		Rect(Vector2.Lerp(min, max, 0.5f), (max - min), 0, args);

		Mark(min, DrawArgs.Default);
		Mark(max, DrawArgs.Default);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Circle(Vector2 centre, float r, int sample = 32, DrawArgs? args = null)
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
				D.Line(first, previous, args);
			else
				D.Line(previous, cur, args);

			previous = cur;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void BoxCast(Vector2 pos, Vector2 size, float angle, Vector2 dir, float dist, DrawArgs? args = null)
	{
		var castPos = pos + dir.normalized * dist;

		D.Rect(pos, size, angle, args); // 起始碰撞盒
		D.Rect(castPos, size, angle, args); // 目标碰撞盒

		D.Arrow(pos, castPos, args);
	}

	[Conditional("UNITY_EDITOR")]
	public static void Curve(System.Func<float, Vector2> curve, int sample, bool isClosed = false, DrawArgs? args = null)
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
				D.Line(previous, first, args);
			else
				D.Line(previous, cur, args);

			previous = cur;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void NormalizedCurveTo(System.Func<float, float> normalizedCurve, int sample, Vector2 max, Vector2 min, bool isClosed = false, DrawArgs? args = null)
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
				D.Line(previous, first, args);
			else
				D.Line(previous, cur, args);

			previous = cur;
		}
	}

	//public static void DrawMesgOnScreen(Vector2 screenPos, string richMesg)
	//{
	//	var center = Camera.main.ScreenToWorldPoint(screenPos);
	//	D.DrawMesg(center, richMesg);
	//}
	//public static void DrawMesg(Vector2 center, string richMesg)
	//{
	//	var style = EditorStyles.label;
	//	style.richText = true;

	//	Handles.Label(center, richMesg, style);
	//}
}