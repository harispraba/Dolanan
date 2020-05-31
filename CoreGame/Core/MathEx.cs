﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace CoreGame.Engine
{
	public static class MathEx
	{
		public static int PosMod(int i1, int i2)
		{
			int i = i1 % i2;
			if ((i < 0 && i2 > 0) || (i > 0 && i2 < 0))
				i += i2;

			return i;
		}
		public static float FPosMod(float f1, float f2)
		{
			float i = f1 % f2;
			if ((i < 0 && f2 > 0) || (i > 0 && f2 < 0))
				i += f2;

			return i;
		}

		public static float Clamp(float val, float min, float max)
		{
			return MathF.Min(MathF.Max(val, min), max);
		}
		
		public static float Min(params float[] floats)
		{
			return floats.Min();
		}
		public static float Max(params float[] floats)
		{
			return floats.Max();
		}

		/// <summary>
		/// Turn vector direction to Angle radian
		/// </summary>
		/// <param name="v"></param>
		/// <returns>Angle radian</returns>
		public static float Angle(this Vector2 v)
		{
			return MathF.Atan2(v.Y, v.X);
		}

		/// <summary>
		/// Similar to LookAt
		/// </summary>
		/// <param name="v"></param>
		/// <param name="point"></param>
		/// <returns>angle radian</returns>
		public static float AngleToPoint(this Vector2 v, Vector2 point)
		{
			return MathF.Atan2(v.Y - point.Y, v.X - point.X);
		}
	}
}