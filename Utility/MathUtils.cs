using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public static class MathUtils
{
	public static bool SameSign(float a, float b)
	{
		var t = a * b;
		// Approximately用于排除误差造成的极小值的情况
		return Mathf.Approximately(a, b) || t > 0;
	}

	// 计算在一个循环范围中值的最小间隔
	// 符号代表方向, 返回值以绝对值大小排序
	// 必须确保from和to在[start, end]内
	public static (float max, float min) LoopValueInterval(float from, float to, float end, float start = 0)
	{
		if (Mathf.Approximately(from, to))
			return (end - start, 0);

		float d1 = to - from;
		float d2;

		if (to > from)
			d2 = to - end + start - from;
		else
			d2 = to - start + end - from;

		return Mathf.Abs(d1) > Mathf.Abs(d2) ? (d1, d2) : (d2, d1);
	}

	// 计算射线所在直线与矩形相交信息
	// 返回交点与origin距离, tmax, tmin
	// 检查射线相交情况时使用 tmax >= Mathf.Max(tmin, 0)
	// 获取命中点时
	// var hitPoint = origin + ray * Mathf.Max(tmin, 0.0f);
	public static (float, float) IntersectRay(Rect b, Vector2 origin, Vector2 dir)
	{
		var norInv = new Vector2(1.0f / dir.x, 1.0f / dir.y);

		float tx1 = (b.min.x - origin.x) * norInv.x;
		float tx2 = (b.max.x - origin.x) * norInv.x;

		float tmin = Mathf.Min(tx1, tx2);
		float tmax = Mathf.Max(tx1, tx2);

		float ty1 = (b.min.y - origin.y) * norInv.y;
		float ty2 = (b.max.y - origin.y) * norInv.y;

		tmin = Mathf.Max(tmin, Mathf.Min(ty1, ty2));
		tmax = Mathf.Min(tmax, Mathf.Max(ty1, ty2));

		return (tmax, tmin);
	}

	public static Vector2 SpacePointMap(Vector2 source, Vector2 target, Vector2 point, bool clamp = true)
	{
		var nor = point / source;
		if (clamp)
		{
			nor.x = Mathf.Clamp01(nor.x);
			nor.y = Mathf.Clamp01(nor.y);
		}
		return nor * target;
	}

	// 将位于同一空间的source矩形内的点映射到target矩形内
	// 应当保持点在矩形内的相对位置不变
	public static Vector2 SpacePointMap(Rect source, Rect target, Vector2 point, bool clamp = true, bool retGolbalPoint = true, bool yReverse = false, bool xReverse = false)
	{
		point -= source.position;
		var nor = point / source.size;
		if (clamp)
		{
			nor.x = Mathf.Clamp01(nor.x);
			nor.y = Mathf.Clamp01(nor.y);
		}

		if (xReverse) nor.x = 1 - nor.x;
		if (yReverse) nor.y = 1 - nor.y;

		point = (nor * target.size);

		if(retGolbalPoint) 
			point += target.position;

		return point;
	}

	// 波峰函数
	// 顶点在(0.5f, 1.0f), 开口向下
	public static float SinPeakWave(float x)
		=> -4.0f * Mathf.Pow((x - 0.5f), 2) + 1.0f;

	public static System.Func<float, float> Modulation(System.Func<float, float> basic, System.Func<float, float> ease)
		=> (float x) => basic.Invoke(ease.Invoke(x));

	public static Vector3 ToVec3(this Vector2 vec, float z)
		=> new Vector3(vec.x, vec.y, z);

	// 斜距式
	public static Vector2 Point(float x, float k, float b)
	{
		float y = x * k + b;
		return new Vector2(x, y);
	}

	// 检查是否在范围中
	public static bool InRange(float v, Vector2 range)
	{ 
		return InRange(v, range.x, range.y);
	}
	public static bool InRange(float v, float max, float min)
	{
		Debug.Assert(max > min);
		return v >= min && v <= max;
	}
	public static bool InRange(Vector2 v, Vector2 max, Vector2 min, bool contain = true)
	{
		if (contain)
		{
			return v.x <= max.x && v.x >= min.x
				&& v.y <= max.y && v.y >= min.y;
		}
		else
		{
			return v.x < max.x && v.x > min.x
				&& v.y < max.y && v.y > min.y;
		}
	}

	public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
	{
		var s = (p0.x - p2.x) * (p.y - p2.y) - (p0.y - p2.y) * (p.x - p2.x);
		var t = (p1.x - p0.x) * (p.y - p0.y) - (p1.y - p0.y) * (p.x - p0.x);

		if ((s < 0) != (t < 0) && s != 0 && t != 0)
			return false;

		var d = (p2.x - p1.x) * (p.y - p1.y) - (p2.y - p1.y) * (p.x - p1.x);
		return d == 0 || (d < 0) == (s + t <= 0);
	}

	// 在abs(v - unit*n) <= tolerance时返回unit*n, 其他情况下返回v
	// 即当v值足够靠近unit的倍数时, 将返回unit的倍数, 否则返回v, 由tolerance决定距离
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

	public static float LoopValue(float v, float max, float min)
	{
		if (v < min)
		{
			float dist = Mathf.Abs(v - min) % (max - min);
			return max - dist;
		}
		else if (v > max)
		{
			float dist = Mathf.Abs(max - v) % (max - min);
			return min + dist;
		}
		else return v;
	}
	public static float LoopValue(float v, float max)
		=> LoopValue(v, max, 0);

	public static int LoopIndex(int i, int length, int step = 0)
	{
		i += step;
		if (i < 0) i = length + (i % length);
		return i % length;
	}

	public static int ClampIndex(int i, int length)
		=> Mathf.Clamp(i, 0, length - 1);

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

	// 计算v绕pivot逆时针旋转angle后的值
	public static Vector3 RotationPoint(Vector3 point, Vector3 pivot, Vector3 angle)
	{
		return Quaternion.Euler(angle) * (point - pivot) + pivot;
	}
	public static Vector2 RotationDirect(Vector2 dir, float angle)
		=> RotationPoint(dir, Vector3.zero, Vector3.forward * angle);

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

	// 点到直线的距离
	public static float DistancePointToLine(Vector2 point, Vector2 linePointA, Vector2 linePointB)
	{
		float lineLengthSquared = Vector2.Distance(linePointA, linePointB);
		if (lineLengthSquared == 0f)
		{
			// 线段长度为零，点与线段的距离为点到线段端点的距离
			return Vector2.Distance(point, linePointA);
		}

		float t = Mathf.Clamp01(Vector2.Dot(point - linePointA, linePointB - linePointA) / lineLengthSquared);
		Vector2 projection = linePointA + t * (linePointB - linePointA);
		return Vector2.Distance(point, projection);
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

	// 采样计算曲线的近似长度
	public static float ApproximateCurveLength(System.Func<float, Vector2> norCurve, int sample, bool isClosed = false)
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
			else if (i == sample && isClosed)
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

	// 对路径进行平滑
	public static int SmoothPath(ref List<Vector3> dst, ICollection<Vector3> src, int unitSimple, float frontInsterFactor = 0.5f, float backInsterFactor = 0.5f)
	{
		Debug.Assert(frontInsterFactor + backInsterFactor > 0
			&& frontInsterFactor + backInsterFactor <= 1,
			"The sum of the facor must be (0-1]");

		int srcCount = src.Count;
		int dstCount = dst.Count;

		if (srcCount <= 2)
		{
			dst.AddRange(src);
			return dst.Count - dstCount;
		}

		var itor = src.GetEnumerator();

		// 添加起始点
		itor.MoveNext();
		dst.Add(itor.Current);

		// 将迭代器推进到第二个元素
		// 进入循环后该操作在预读next时执行
		itor.MoveNext();

		// 在循环中跳过第一个和最后一个位置
		for (int i = 1; i < srcCount - 1; i++)
		{
			// 获取当前点的前后点
			var cur = itor.Current;

			itor.MoveNext();
			var next = itor.Current;

			var last = dst[dst.Count - 1];

			// 根据系数在last-cur和cur-next之间计算插入点
			var ins1 = cur - frontInsterFactor * (cur - last);
			var ins2 = next - (1.0f - backInsterFactor) * (next - cur);

			// 计算采样数
			//var len = (ins2 - ins1).magnitude;
			//int sample = Mathf.Max(1, Mathf.FloorToInt(len * unitSimple));

			// 根据采样数对2个插入点之间的生成的贝塞尔曲线进行采样
			for (int j = 0; j < unitSimple; j++)
			{
				float t = ((float)j) / (unitSimple - 1);
				var tmp = MathUtils.BezierCurve(ins1, cur, ins2, t);

				// 拐角曲线的采样点
				dst.Add(tmp);
			}
		}

		// 添加未尾点
		dst.Add(itor.Current);

		return dstCount - dst.Count;
	}

	// 计算路径点的长度
	public static float CalculatePathLength(IEnumerable<Vector3> waypoints, bool isClosed = false)
	{
		float distance = 0f;

		Vector3 previousWaypoint = Vector3.zero;
		bool isFirstWaypoint = true;

		foreach (Vector3 currentWaypoint in waypoints)
		{
			if (isFirstWaypoint)
			{
				previousWaypoint = currentWaypoint;
				isFirstWaypoint = false;
				continue;
			}

			distance += Vector3.Distance(previousWaypoint, currentWaypoint);
			previousWaypoint = currentWaypoint;
		}

		if (isClosed)
			distance += Vector3.Distance(previousWaypoint, waypoints.GetEnumerator().Current);

		return distance;
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

	public static float Cot(float x)
	{
		return 1.0f / Mathf.Tan(x);
	}

	public static int GetMaxMin(out float max, out float min, IEnumerable<float> values)
	{
		max = float.NegativeInfinity;
		min = float.PositiveInfinity;

		int count = 0;
		foreach (var v in values)
		{
			max = Mathf.Max(max, v);
			min = Mathf.Min(min, v);

			count++;
		}
		return count;
	}

	public static int GetMaxMin(out Vector2 max, out Vector2 min, IEnumerable<Vector2> values)
	{
		max = Vector2.negativeInfinity;
		min = Vector2.positiveInfinity;

		int count = 0;
		foreach (var v in values)
		{
			max = Vector2.Max(max, v);
			min = Vector2.Min(min, v);

			count++;
		}
		return count;
	}
	public static int GetMaxMin(out Vector3 max, out Vector3 min, IEnumerable<Vector3> values)
	{
		max = Vector3.negativeInfinity;
		min = Vector3.positiveInfinity;

		int count = 0;
		foreach (var v in values)
		{
			max = Vector3.Max(max, v);
			min = Vector3.Min(min, v);

			count++;
		}
		return count;
	}

	public static float AngleDiff(float from, float to)
	{
		return (360 + to - from) % 360;
	}

}

/*
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
		return $"WRect{{{centre}, {size}}}";
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

 */