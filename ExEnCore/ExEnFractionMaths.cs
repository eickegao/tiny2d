using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Microsoft.Xna.Framework
{
	internal struct Fraction
	{
		public int Numerator;
		public int Denominator;

		public Fraction(int numerator, int denominator)
		{
			this.Numerator = numerator;
			this.Denominator = denominator;
			Debug.Assert(denominator != 0);
			Simplify();
		}

		public static implicit operator Fraction(int value)
		{
			return new Fraction() { Numerator = value, Denominator = 1 };
		}

		static int GCD(int a, int b)
		{
			// Abs
			if(a < 0) a = -a;
			if(b < 0) b = -b;

			while(b != 0)
			{
				int m = a % b;
				a = b;
				b = m;
			}
			return a;
		}

		void Simplify()
		{
			Debug.Assert(Denominator != 0);

			if(Denominator < 0)
			{
				Numerator *= -1;
				Denominator *= -1;
			}

			if(Numerator == 0)
			{
				Denominator = 1;
				return;
			}

			int gcd = GCD(Numerator, Denominator);
			Numerator /= gcd;
			Denominator /= gcd;
		}

		public int ToInteger()
		{
			return Numerator / Denominator;
		}

		public override string ToString()
		{
			if(Denominator == 1)
				return Numerator.ToString();
			else
				return Numerator + "/" + Denominator;
		}

		#region Operators

		public static Fraction operator +(Fraction f1, Fraction f2)
		{
			var result = new Fraction();
			result.Numerator = (f1.Numerator * f2.Denominator) + (f2.Numerator * f1.Denominator);
			result.Denominator = (f1.Denominator * f2.Denominator);
			result.Simplify();
			return result;
		}

		public static Fraction operator -(Fraction f1, Fraction f2)
		{
			var result = new Fraction();
			result.Numerator = (f1.Numerator * f2.Denominator) - (f2.Numerator * f1.Denominator);
			result.Denominator = (f1.Denominator * f2.Denominator);
			result.Simplify();
			return result;
		}

		public static Fraction operator *(Fraction f1, Fraction f2)
		{
			var result = new Fraction();
			result.Numerator = (f1.Numerator * f2.Numerator);
			result.Denominator = (f1.Denominator * f2.Denominator);
			result.Simplify();
			return result;
		}

		public static Fraction operator /(Fraction f1, Fraction f2)
		{
			var result = new Fraction();
			result.Numerator = (f1.Numerator * f2.Denominator);
			result.Denominator = (f1.Denominator * f2.Numerator);
			result.Simplify();
			return result;
		}

		#endregion
	}


	internal struct FractionTransform2D
	{
		#region Data and Construction

		// http://www.senocular.com/flash/tutorials/transformmatrix/
		// Where a = M11, b = M12, c = M21, d = M22

		/// <summary>Element at Row 1, Column 1</summary>
		public Fraction M11;
		/// <summary>Element at Row 1, Column 2</summary>
		public Fraction M12;
		/// <summary>Element at Row 2, Column 1</summary>
		public Fraction M21;
		/// <summary>Element at Row 2, Column 2</summary>
		public Fraction M22;
		/// <summary>Element at Row 3, Column 1; Translation on the X axis</summary>
		public Fraction OffsetX;
		/// <summary>Element at Row 3, Column 2; Translation on the Y axis</summary>
		public Fraction OffsetY;

		public FractionTransform2D(Fraction M11, Fraction M12, Fraction M21, Fraction M22, Fraction OffsetX, Fraction OffsetY)
		{
			this.M11 = M11;
			this.M12 = M12;
			this.M21 = M21;
			this.M22 = M22;
			this.OffsetX = OffsetX;
			this.OffsetY = OffsetY;
		}

		private static readonly FractionTransform2D _identity = new FractionTransform2D(1, 0, 0, 1, 0, 0);
		public static FractionTransform2D Identity { get { return _identity; } }

		public override string ToString()
		{
			return string.Format("{{ {{M11:{0} M12:{1}}} {{M21:{2} M22:{3}}} {{OffsetX:{4} OffsetY:{5}}} }}",
					M11, M12, M21, M22, OffsetX, OffsetY);
		}

		#endregion


		#region Transforms

		public Point Transform(Point position)
		{
			Point result;
			result.X = (position.X * M11 + position.Y * M21 + OffsetX).ToInteger();
			result.Y = (position.X * M12 + position.Y * M22 + OffsetY).ToInteger();
			return result;
		}

		#endregion


		#region Operations

		static public FractionTransform2D operator *(FractionTransform2D t1, FractionTransform2D t2)
		{
			FractionTransform2D result;
			result.M11 = t1.M11 * t2.M11 + t1.M12 * t2.M21;
			result.M12 = t1.M11 * t2.M12 + t1.M12 * t2.M22;
			result.M21 = t1.M21 * t2.M11 + t1.M22 * t2.M21;
			result.M22 = t1.M21 * t2.M12 + t1.M22 * t2.M22;
			result.OffsetX = t1.OffsetX * t2.M11 + t1.OffsetY * t2.M21 + t2.OffsetX;
			result.OffsetY = t1.OffsetX * t2.M12 + t1.OffsetY * t2.M22 + t2.OffsetY;
			return result;
		}

		#endregion


		#region Creation

		public static FractionTransform2D CreateOrthographic(int left, int right, int bottom, int top)
		{
			return new FractionTransform2D()
			{
				M11 = new Fraction(2, right - left),
				M12 = new Fraction(0, 1),
				M21 = new Fraction(0, 1),
				M22 = new Fraction(2, top - bottom),
				OffsetX = new Fraction(right + left, left - right),
				OffsetY = new Fraction(top + bottom, bottom - top),
			};
		}

		public static FractionTransform2D CreateOrthographicInverse(int left, int right, int bottom, int top)
		{
			return new FractionTransform2D()
			{
				M11 = new Fraction(right - left, 2),
				M12 = new Fraction(0, 1),
				M21 = new Fraction(0, 1),
				M22 = new Fraction(top - bottom, 2),
				OffsetX = new Fraction(left + right, 2),
				OffsetY = new Fraction(top + bottom, 2),
			};
		}

		public static FractionTransform2D CreateRotation(int cos, int sin)
		{
			FractionTransform2D result;
			result.M11 =  cos; result.M12 = sin;
			result.M21 = -sin; result.M22 = cos;
			result.OffsetX = 0; result.OffsetY = 0;
			return result;
		}

		#endregion

	}

}
