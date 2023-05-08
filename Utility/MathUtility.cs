#define MATH_UTILITY_DEFINED
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[System.Serializable]
public struct WRect
{
	public static WRect CreateByMinMax(Vector2 max, Vector2 min)
	{
		var rect = new WRect
		{
			Max = max,
			Min = min
		};
		return rect;
	}

	public static bool Overlap(WRect r1, WRect r2)
	{
		return !(r1.Left > r2.Right
			|| r1.Right < r2.Left
			|| r1.Bottom > r2.Top
			|| r1.Top < r2.Bottom);
	}

	public static bool Contains(WRect r1, WRect r2)
	{
		return r1.Left <= r2.Left
			&& r1.Right >= r2.Right
			&& r1.Bottom <= r2.Bottom
			&& r1.Top >= r2.Top;
	}

	public static WRect Intersection(WRect r1, WRect r2)
	{
		float left = Mathf.Max(r1.Left, r2.Left);
		float right = Mathf.Min(r1.Right, r2.Right);
		float bottom = Mathf.Max(r1.Bottom, r2.Bottom);
		float top = Mathf.Min(r1.Top, r2.Top);

		return WRect.CreateByMinMax(new Vector2(right, top), new Vector2(left, bottom));
	}

	public override string ToString()
	{
		return $"Rect{{{centre}, {size}}}";
	}

	public Vector2 centre;
	public Vector2 size;

	public Vector2 HalfSize { get => this.size * 0.5f; }

	public Vector2 Min
	{
		set
		{
			Debug.Assert(value.x <= this.Max.x && value.y <= this.Max.y);

			var max = this.Max;

			this.size = max - value;
			this.centre = Vector2.Lerp(value, max, 0.5f);
		}
		get => centre - this.HalfSize;
	}
	public Vector2 Max
	{
		set
		{
			Debug.Assert(value.x >= this.Min.x && value.y >= this.Min.y);

			var min = this.Min;

			this.size = value - min;
			this.centre = Vector2.Lerp(min, value, 0.5f);
		}
		get => centre + this.HalfSize;
	}

	public float Left
	{
		set => this.Min = new Vector2(value, this.Min.y);
		get => this.centre.x - this.HalfSize.x;
	}
	public float Right
	{
		set => this.Max = new Vector2(value, this.Max.y);
		get => this.centre.x + this.HalfSize.x;
	}

	public float Bottom
	{
		set => this.Min = new Vector2(this.Min.x, value);
		get => this.centre.y - this.HalfSize.y;
	}
	public float Top
	{
		set => this.Max = new Vector2(this.Max.x, value);
		get => this.centre.y + this.HalfSize.y;
	}

	public void SetPositionFromMinPoint(Vector2 value)
	{
		this.centre = value + this.HalfSize;
	}
	public void SetPositionFromMaxPoint(Vector2 value)
	{
		this.centre = value - this.HalfSize;
	}
}

public static class MathUtility
{
	public static Vector3 SetValue(this ref Vector3 basic, float? x = null, float? y = null, float? z = null)
	{
		if (x.HasValue) basic.x = x.Value;
		if (y.HasValue) basic.y = y.Value;
		if (z.HasValue) basic.z = z.Value;
		return basic;
	}
	public static Vector3 SetValue(this ref Vector3 basic, Vector2 v, float? z = null)
	{
		basic.x = v.x;
		basic.y = v.y;
		if (z.HasValue) basic.z = z.Value;
		return basic;
	}

	public static Vector3 SetValue(Vector3 basic, float? x = null, float? y = null, float? z = null)
		=> SetValue(ref basic, x, y, z);
	public static Vector3 SetValue(Vector3 basic, Vector2 v, float? z = null)
		=> SetValue(ref basic, v, z);

	public static Vector2 SetValue(this ref Vector2 basic, float? x = null, float? y = null)
	{
		if (x.HasValue) basic.x = x.Value;
		if (y.HasValue) basic.y = y.Value;
		return basic;
	}
	public static Vector2 SetValue(Vector2 basic, float? x = null, float? y = null)
		=> SetValue(basic, x, y);

	// 斜距式
	public static Vector2 Point(float x, float k, float b)
	{
		float y = x * k + b;
		return new Vector2(x, y);
	}

	// 检查是否在范围中
	public static bool InRange(float v, float r1, float r2)
	{
		return v >= Mathf.Min(r1, r2) && v <= Mathf.Max(r1, r2);
	}
	public static bool InRange(Vector2 v, Vector2 r1, Vector2 r2)
	{
		return InRange(v.x, Mathf.Max(r1.x, r2.x), Mathf.Min(r1.x, r2.x))
			&& InRange(v.y, Mathf.Max(r1.y, r2.y), Mathf.Min(r1.y, r2.y));
	}

	// 在abs(v - unit*n) <= tolerance时返回unit*n, 其他情况下返回v
	// 该函数一般用于网格坐标点对齐
	public static float Align(float v, float unit, float tolerance)
	{
		int q = Mathf.CeilToInt(v / unit);
		float r = v % unit;

		if (r >= unit - tolerance || r <= tolerance)
			return unit * q;
		return v;
	}
	public static int Align(int v, int unit, int tolerance)
	{
		int q = Mathf.CeilToInt(v / unit);
		int r = v % unit;

		if (r >= unit - tolerance || r <= tolerance)
			return unit * q;
		return v;
	}

	public static Vector2 Clamp(Vector2 v, Vector2 max, Vector2 min)
	{
		v.x = Mathf.Clamp(v.x, min.x, max.x);
		v.y = Mathf.Clamp(v.y, min.y, max.y);
		return v;
	}


	public static float Loop(float v, float max, float min)
	{
		if (v < min)
		{
			float dist = Mathf.Abs(v - min) % (max-min);
			return max - dist;
		}
		else if (v > max)
		{
			float dist = Mathf.Abs(max - v) % (max - min);
			return min + dist;
		}
		else return v;
	}
	public static float Loop(float v, float max)
		=> Loop(v, max, 0);

	public static int LoopIndex(int i, int length, int step = 1)
	{
		i += step;

		if (i < 0) i = length + (i % length);

		return i % length;
	}

	static public float MapToRange(float rate, float max, float min)
	{
		return rate * (max - min) + min;
	}
	static public float MapToRate(float v, float max, float min)
	{
		return (v - min) / (max - min);
	}

	static public float FullAngle(Vector2 dir, Vector2? basic = null, bool cw = false)
	{
		basic = basic ?? Vector2.right;

		var angle = Vector2.SignedAngle(dir, basic.Value);
		angle = angle < 0 ? Mathf.Abs(angle) : (360.0f - angle);

		if (cw) angle = 360 - angle;
		return angle;
	}

	// 曼哈顿距离
	public static float ManhattanDistance(Vector2 a, Vector2 b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

	// 计算v绕pivot旋转angle后的值
	public static Vector3 RotationPoint(Vector3 point, Vector3 pivot, Vector3 angle)
	{
		return Quaternion.Euler(angle) * (point - pivot) + pivot;
	}

	// 向下舍入到基数的倍数
	public static float NearestRound(float v, float unit)
	{
		return v - v % unit;
	}
	public static int NearestRound(int v, int unit)
	{
		return v - v % unit;
	}

	// 返回符号
	public static float Sign(float v)
	{
		return Mathf.Approximately(v, 0) ? 0 : v > 0 ? 1 : -1;
	}
	public static int Sign(int v)
	{
		return v == 0 ? 0 : v > 0 ? 1 : -1;
	}
	public static int SignInt(float v)
	{
		return Mathf.Approximately(v, 0) ? 0 : v > 0 ? 1 : -1;
	}

	// 圆的参数方程
	public static Vector2 Circle(float radius, float angleRad)
	{
		// x = x0 + r cos ⁡θ , y = y0 + r sin θ
		return new Vector2(radius * Mathf.Cos(angleRad), radius * Mathf.Sin(angleRad));
	}

	// 椭圆的参数方程
	public static Vector2 Ellipse(float longAxis, float shortAxis, float angleRad)
	{
		return new Vector2(longAxis * Mathf.Cos(angleRad), shortAxis * Mathf.Sin(angleRad));
	}

	// 二次贝塞尔
	public static Vector2 BezierCurve(Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		t = Mathf.Clamp01(t);
		// B(t) = (1-t)^2 * P1 + 2t * (1-t) * P2 + t^2 * P3
		return Mathf.Pow(1 - t, 2) * p1 + 2 * t * (1 - t) * p2 + Mathf.Pow(t, 2) * p3;
	}
	public static Vector3 BezierCurve(Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		t = Mathf.Clamp01(t);
		// B(t) = (1-t)^2 * P1 + 2t * (1-t) * P2 + t^2 * P3
		return Mathf.Pow(1 - t, 2) * p1 + 2 * t * (1 - t) * p2 + Mathf.Pow(t, 2) * p3;
	}

	// 三次贝塞尔
	public static Vector2 BezierCurve(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float t)
	{
		t = Mathf.Clamp01(t);
		// B(t) = (1-t)^3 * P1 + 3t * (1-t)^2 * P2 + 3 * t^2 * (1-t) * P3 + t^3 * P4
		return Mathf.Pow(1 - t, 3) * p1 + 3 * t * Mathf.Pow(1 - t, 2) * p2 + 3 * Mathf.Pow(t, 2) * (1 - t) * p3 + Mathf.Pow(t, 3) * p4;
	}

	// 采样分段计算曲线的近似长度
	public static float ApproximateLength(System.Func<float, Vector2> norCurve, int sample, bool isClose = false)
	{
		float length = 0;

		Vector2 first = default;
		Vector2 previous = default;

		for (int i = 0; i <= sample; i++)
		{
			float t = Mathf.Clamp01(((float)i) * sample);

			Vector2 cur = norCurve(t);

			if (i == 0)
			{
				first = cur; // 记录第一个采样点
			}
			else if (i == sample && isClose)
			{
				// 如果是封闭曲线, 记录末尾点到起点的距离
				length += Vector2.Distance(previous, first);
			}
			else
			{
				length += Vector2.Distance(previous, cur);
			}

			previous = cur;
		}

		return length;
	}

	// 自定义振幅与频率的sin波函数
	// y in [-amplitude + yOffset, amplitude + yOffset]
	public static float SinWave(float amplitude, float cycle, float x, float xOffset = 0, float yOffset = 0)
	{
		x += xOffset;
		return amplitude * Mathf.Sin(2 * Mathf.PI * x / cycle) + yOffset;
	}

	// 自定义振幅与频率的sin波函数
	// y in [0, amplitude + yOffset]
	public static float AbsSinWave(float amplitude, float cycle, float x, float xOffset = 0, float yOffset = 0)
	{
		x += xOffset;
		return amplitude * Mathf.Abs(Mathf.Sin(Mathf.PI * x / cycle)) + yOffset;
	}
}
