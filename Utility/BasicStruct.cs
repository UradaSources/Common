using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace BasicStruct
{
	[System.Serializable]
	public struct Rangef
	{
		public static explicit operator Rangei(Rangef r)
		{
			return new Rangei((int)r.min, (int)r.max);
		}

		public static Rangef Range01 => new Rangef(0, 1); 

		/// <summary>
		/// 求a,b交集
		/// </summary>
		public static bool intersection(Rangef a, Rangef b, out Rangef result)
		{
			var min = Mathf.Max(a.min, b.min);
			var max = Mathf.Min(a.max, b.min);

			if (max > min)
			{
				result = new Rangef(min, max);
				return true;
			}
			result = default;
			return false;
		}

		/// <summary>
		/// 求a,b并集
		/// </summary>
		public static bool union(Rangef a, Rangef b, out Rangef result)
		{
			var min = Mathf.Min(a.min, b.min);
			var max = Mathf.Max(a.max, b.min);

			if (max > min)
			{
				result = new Rangef(min, max);
				return true;
			}
			result = default;
			return false;
		}

		public float min;
		public float max;

		public bool vaild() => this.max > this.min;
		public float random() => Random.Range(min, max);

		public float normalized(float v, bool clamp01 = true)
		{
			var nor = (v - min) / (max - min);
			if (clamp01) nor = Mathf.Clamp01(nor);
			return nor;
		}
		public float map(float t, bool clamp01 = true)
		{
			if (clamp01) t = Mathf.Clamp01(t);
			return Mathf.LerpUnclamped(this.min, this.max, t);
		}

		public Rangef(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
	}

	[System.Serializable]
	public struct Rangei
	{
		public static implicit operator Rangef(Rangei v)
		{
			return new Rangef(v.min, v.max);
		}

		/// <summary>
		/// 求a,b交集
		/// </summary>
		public static bool intersection(Rangei a, Rangei b, out Rangei result)
		{
			var min = Mathf.Max(a.min, b.min);
			var max = Mathf.Min(a.max, b.min);

			if (max > min)
			{
				result = new Rangei(min, max);
				return true;
			}
			result = default;
			return false;
		}

		/// <summary>
		/// 求a,b并集
		/// </summary>
		public static bool union(Rangei a, Rangei b, out Rangei result)
		{
			var min = Mathf.Min(a.min, b.min);
			var max = Mathf.Max(a.max, b.min);

			if (max > min)
			{
				result = new Rangei(min, max);
				return true;
			}
			result = default;
			return false;
		}

		public int min;
		public int max;

		public bool vaild() => this.max > this.min;
		public int random() => Random.Range(min, max + 1);

		public Rangei(int min, int max)
		{
			this.min = min;
			this.max = max;
		}
	}

	[System.Serializable]
	public struct Line
	{
		public static Line From2Point(float x1, float y1, float x2, float y2)
		{
			float a, b, c;

			// 直线垂直于x轴, 斜率不存在
			if (Mathf.Approximately(x1, x2))
			{
				a = 1;
				b = 0;
				c = -x1;
			}
			else
			{
				float k = (y2 - y1) / (x2 - x1);

				a = -k;
				b = 1;
				c = -y1 + k * x1;
			}

			return new Line(a, b, c);
		}
		public static Line From2Point(Vector2 a, Vector2 b)
			=> From2Point(a.x, a.y, b.x, b.y);

		public static bool Intersection(Line l1, Line l2, out Vector2 point)
		{
			if (l1.a * l2.b - l2.a * l1.b == 0)
			{
				point = Vector2.zero;
				return false;
			}

			float x = (l1.b * l2.c - l2.b * l1.c) / (l1.a * l2.b - l2.a * l1.b);
			float y = (l2.a * l1.c - l1.a * l2.c) / (l1.a * l2.b - l2.a * l1.b);
			point = new Vector2(x, y);

			return true;
		}

		public float a, b, c;

		/// <summary>
		/// 直线的斜率
		/// </summary>
		public float slope => -this.a / this.b;

		public Line(float a, float b, float c)
		{
			this.a = a;
			this.b = b;
			this.c = c;
		}
	}

	public struct Triangle
	{
		private Vector2 _a, _b, _c;
		public Vector2 a => _a;
		public Vector2 b => _b;
		public Vector2 c => _c;

		private float _denominator;
		public float denominator => _denominator;

		public float lineA2B => Vector2.Distance(a, b);
		public float lineB2C => Vector2.Distance(b, c);
		public float lineC2A => Vector2.Distance(c, a);

		public bool Contains(Vector2 point)
		{
			float u = ((b.y - c.y) * (point.x - c.x) + (c.x - b.x) * (point.y - c.y)) / denominator;
			float v = ((c.y - a.y) * (point.x - c.x) + (a.x - c.x) * (point.y - c.y)) / denominator;
			float w = 1f - u - v;

			return (u >= 0f && v >= 0f && w >= 0f);
		}
		public Vector2 MapPointCOrig(Vector2 point)
		{
			var x = MathUtils.DistancePointToLine(point, this.c, this.a);
			var y = MathUtils.DistancePointToLine(point, this.c, this.b);
			return new Vector2(x, y);
		}

		public Triangle(Vector2 a, Vector2 b, Vector2 c)
		{
			_a = a;
			_b = b;
			_c = c;
			
			_denominator = ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y));
		}
	}
}