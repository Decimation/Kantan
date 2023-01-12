using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

// ReSharper disable TailRecursiveCall
// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable SuggestVarOrType_SimpleTypes

// ReSharper disable InconsistentNaming

// ReSharper disable FieldCanBeMadeReadOnly.Local

// ReSharper disable UnusedMember.Global

namespace Kantan.Numeric;

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
	/*public static bool ToleranceEqual(double a, double b, double epsilon = double.Epsilon)
	{
		//https://stackoverflow.com/questions/4787125/evaluate-if-two-doubles-are-equal-based-on-a-given-precision-not-within-a-certa

		var absA = Math.Abs(a);
		var absB = Math.Abs(b);
		var diff = Math.Abs(a - b);

		if (a*b==0) {
			return diff < (epsilon*epsilon);
		}
		else {
			return diff / (absA + absB) < epsilon;
		}
	}*/

	public static T Wrap<T>(T i, T n) where T : INumber<T>
	{
		return ((i % n) + n) % n;
	}

	public static bool ToleranceEqual(double a, double b, double epsilon = double.Epsilon)
	{
		//https://stackoverflow.com/questions/4787125/evaluate-if-two-doubles-are-equal-based-on-a-given-precision-not-within-a-certa

		return Math.Abs(a - b) <= epsilon;
	}

	public static T HighestOrderBit<T>(T num) where T : INumber<T>, IShiftOperators<T, T, T>
	{
		// NOTE: redundant? Use BitOperations?

		if (!(num > T.Zero))
			return T.Zero;

		T ret = T.One;

		while ((num >>= T.One) > T.Zero)
			ret <<= T.One;

		return ret;
	}

	public static T[] SimplifyRadical<T>(T insideRoot) where T : INumber<T>
	{
		T outside_root = T.One;

		T d = T.One + T.One;

		while (d * d <= insideRoot) {
			if (insideRoot % (d * d) == T.Zero) {
				insideRoot   /= (d * d);
				outside_root *= d;
			}

			else {
				d++;
			}
		}

		var radical = new T[2];

		radical[0] = outside_root;
		radical[1] = insideRoot;

		return radical;
	}

	public static T GCD<T>(T a, T b) where T : INumber<T>, IBitwiseOperators<T, T, T>
	{
		while (a != T.Zero && b != T.Zero) {
			if (a > b)
				a %= b;
			else
				b %= a;
		}

		return a | b;
	}

	public static T LCM<T>(T a, T b) where T : INumber<T>, IBitwiseOperators<T, T, T> => (a / GCD(a, b)) * b;

	public static BigInteger LCM(BigInteger number1, BigInteger number2)
	{
		if (number1 == 0) {
			return number2;
		}

		if (number2 == 0) {
			return number1;
		}

		var positiveNumber2 = number2 < 0 ? BigInteger.Abs(number2) : number2;
		var positiveNumber1 = number1 < 0 ? BigInteger.Abs(number1) : number1;

		return positiveNumber1 / GCD(positiveNumber1, positiveNumber2) * positiveNumber2;
	}

	public static BigInteger GCD(BigInteger number1, BigInteger number2)
	{
		var positiveNumber2 = number2 < 0 ? BigInteger.Abs(number2) : number2;
		var positiveNumber1 = number1 < 0 ? BigInteger.Abs(number1) : number1;

		if (positiveNumber1 == positiveNumber2)
			return positiveNumber1;

		if (positiveNumber1 == 0)
			return positiveNumber2;

		if (positiveNumber2 == 0)
			return positiveNumber1;

		if ((~positiveNumber1 & 1) != 0) {
			if ((positiveNumber2 & 1) != 0) {
				return GCD(positiveNumber1 >> 1, positiveNumber2);
			}
			else {
				return GCD(positiveNumber1 >> 1, positiveNumber2 >> 1) << 1;
			}
		}

		if ((~positiveNumber2 & 1) != 0) {
			return GCD(positiveNumber1, positiveNumber2 >> 1);
		}

		if (positiveNumber1 > positiveNumber2) {
			return GCD((positiveNumber1 - positiveNumber2) >> 1, positiveNumber2);
		}

		return GCD((positiveNumber2 - positiveNumber1) >> 1, positiveNumber1);
	}

	#region Units

	/// <summary>
	/// SI
	/// </summary>
	public const double MAGNITUDE = 1000D;

	/// <summary>
	/// ISO/IEC 80000
	/// </summary>
	public const double MAGNITUDE2 = 1024D;

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

	#endregion

	public static bool IsPrime<T>(T number)
		where T : INumber<T>, IRootFunctions<T>, IBinaryNumber<T>, IFloatingPoint<T>
	{
		var two = (T.One + T.One);

		if (number <= T.One) {
			return false;
		}
		else if (number == two) {
			return true;
		}

		if (number % two == T.Zero)
			return false;

		T boundary = (T) T.Floor(T.Sqrt(number));

		var three = (two + T.One);

		for (T i = three; i <= boundary; i += two)
			if (number % i == T.Zero)
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