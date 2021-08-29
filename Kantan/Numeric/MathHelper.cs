using System;
using System.Linq.Expressions;

// ReSharper disable InconsistentNaming

// ReSharper disable FieldCanBeMadeReadOnly.Local

// ReSharper disable UnusedMember.Global

namespace Kantan.Numeric
{
	public enum MetricPrefix
	{
		Kilo = 1,
		Mega,
		Giga,
		Tera,
		Peta,
		Exa,
		Zetta,
		Yotta
	}

	public static class MathHelper
	{
		/// <summary>
		/// SI
		/// </summary>
		public const double MAGNITUDE = 1000D;

		/// <summary>
		/// ISO/IEC 80000
		/// </summary>
		public const double MAGNITUDE2 = 1024D;

		public static int HighestOrderBit(int num)
		{
			if (!(num > 0))
				return 0;

			int ret = 1;

			while ((num >>= 1) > 0)
				ret <<= 1;

			return ret;
		}

		public static long[] SimplifyRadical(long insideRoot)
		{
			int outside_root = 1;
			int d            = 2;

			while (d * d <= insideRoot) {
				if (insideRoot % (d * d) == 0) {
					insideRoot   /= (d * d);
					outside_root *= d;
				}

				else
					d++;
			}

			long[] radical = new long[2];
			radical[0] = outside_root;
			radical[1] = insideRoot;

			return radical;
		}

		public static long GCD(long a, long b)
		{
			while (a != 0 && b != 0) {
				if (a > b)
					a %= b;
				else
					b %= a;
			}

			return a | b;
		}

		public static long LCM(long a, long b)
		{
			return (a / GCD(a, b)) * b;
		}

		public static string GetSIUnit(double d, string format = null)
		{
			int degree = (int) Math.Floor(Math.Log10(Math.Abs(d)) / 3);

			double scaled = d * Math.Pow(MAGNITUDE, -degree);

			char? prefix = Math.Sign(degree) switch
			{
				1  => IncPrefixes[degree - 1],
				-1 => DecPrefixes[-degree - 1],
				_  => null
			};

			return scaled.ToString(format) + prefix;
		}


		public static string GetByteUnit(double len)
		{
			//https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net


			int order = 0;

			while (len >= MAGNITUDE && order < BytePrefixes.Length - 1) {
				order++;
				len /= MAGNITUDE;
			}

			// Adjust the format string to your preferences. For example "{0:0.#}{1}" would
			// show a single decimal place, and no space.
			string result = $"{len:0.##} {BytePrefixes[order]}";


			return result;
		}

		/// <summary>
		/// Convert the given bytes to <see cref="MetricPrefix"/>
		/// </summary>
		/// <param name="bytes">Value in bytes to be converted</param>
		/// <param name="type">Unit to convert to</param>
		/// <returns>Converted bytes</returns>
		public static double ConvertToUnit(double bytes, MetricPrefix type)
		{
			// var rg  = new[] { "k","M","G","T","P","E","Z","Y"};
			// var pow = rg.ToList().IndexOf(type) +1;


			int    pow = (int) type;
			double v   = bytes / Math.Pow(MAGNITUDE, pow);


			return v;
		}

		private static readonly string[] BytePrefixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		private static readonly char[] IncPrefixes = { 'k', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y' };

		private static readonly char[] DecPrefixes = { 'm', '\u03bc', 'n', 'p', 'f', 'a', 'z', 'y' };

		#region Generic math

#if !NET6_0_OR_GREATER


		// TODO: Remove when .NET 6 releases

		public static T Add<T>(T a, T b) => MathImplementation<T>.Add(a, b);

		public static T Subtract<T>(T a, T b) => MathImplementation<T>.Sub(a, b);

		public static T Multiply<T>(T a, T b) => MathImplementation<T>.Mul(a, b);

		public static T Divide<T>(T a, T b) => MathImplementation<T>.Div(a, b);

		private static class MathImplementation<T>
		{
			internal static readonly Func<T, T, T> Add;
			internal static readonly Func<T, T, T> Sub;
			internal static readonly Func<T, T, T> Mul;
			internal static readonly Func<T, T, T> Div;

			static MathImplementation()
			{
				Add = Create(Expression.Add);
				Sub = Create(Expression.Subtract);
				Mul = Create(Expression.Multiply);
				Div = Create(Expression.Divide);
			}

			private static Func<T, T, T> Create(Func<ParameterExpression, ParameterExpression, BinaryExpression> fx)
			{
				var paramA = Expression.Parameter(typeof(T));
				var paramB = Expression.Parameter(typeof(T));
				var body   = fx(paramA, paramB);
				return Expression.Lambda<Func<T, T, T>>(body, paramA, paramB).Compile();

			}
		}
#endif

		#endregion

		public static bool IsPrime(int number)
		{
			switch (number) {
				case <= 1:
					return false;
				case 2:
					return true;
			}

			if (number % 2 == 0)
				return false;

			int boundary = (int) Math.Floor(Math.Sqrt(number));

			for (int i = 3; i <= boundary; i += 2)
				if (number % i == 0)
					return false;

			return true;

			/*
			 *	| Method   |     Mean |     Error |    StdDev |
				|---------:|---------:|----------:|----------:|
				| IsPrime  | 2.098 ns | 0.0211 ns | 0.0187 ns |
				| IsPrime2 | 2.568 ns | 0.0074 ns | 0.0061 ns |
			 */

			/*
			 *	if (number == 1) return false;
				if (number == 2) return true;

				double limit = Math.Ceiling(Math.Sqrt(number)); //hoisting the loop limit

				for (int i = 2; i <= limit; ++i)
					if (number % i == 0)
						return false;
				return true;
			 */

		}

		public static float Distance(byte[] first, byte[] second)
		{
			int sum = 0;

			// We'll use which ever array is shorter.
			int length = first.Length > second.Length ? second.Length : first.Length;

			for (int x = 0; x < length; x++) {
				sum += (int) Math.Pow((first[x] - second[x]), 2);
			}

			return sum / (float) length;
		}
	}
}