using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Kantan.Numeric;

// ReSharper disable TailRecursiveCall
// ReSharper disable ConvertIfStatementToReturnStatement

// ReSharper disable UnusedMember.Global
// ReSharper disable UseStringInterpolation
// ReSharper disable PossibleNullReferenceException

namespace Kantan.Numeric;

[Serializable]
public readonly struct RationalNumber : IFormattable, IEquatable<RationalNumber>, IComparable<RationalNumber>,
	IComparable
{
	/*
	 * Adapted from:
	 * https://codereview.stackexchange.com/questions/95681/rational-number-implementation
	 */

	#region Static fields

	private const string POSITIVE_INFINITY_LITERAL = "Infinity";
	private const string NEGATIVE_INFINITY_LITERAL = "-Infinity";
	private const string NA_N_LITERAL              = "NaN";

	private const int FROM_DOUBLE_MAX_ITERATIONS = 25;

	#endregion

	#region Instance fields

	private readonly BigInteger m_numerator, m_denominator;

	private readonly bool m_explicitConstructorCalled, m_isDefinitelyIrreducible;

	#endregion

	#region Constructors

	[DebuggerStepThrough]
	public RationalNumber(BigInteger numerator)
		: this(numerator, 1, true) { }

	[DebuggerStepThrough]
	public RationalNumber(BigInteger numerator, BigInteger denominator)
		: this(numerator, denominator, false) { }

	[DebuggerStepThrough]
	private RationalNumber(RationalNumber numerator, RationalNumber denominator)
		: this(numerator.m_numerator * denominator.Denominator, numerator.Denominator * denominator.m_numerator,
		       false) { }

	private RationalNumber(BigInteger numerator, BigInteger denominator, bool isIrreducible)
	{
		if (denominator < 0) //normalize to positive denominator
		{
			m_denominator = -denominator;
			m_numerator   = -numerator;
		}
		else {
			m_numerator   = numerator;
			m_denominator = denominator;
		}

		m_explicitConstructorCalled = true;
		m_isDefinitelyIrreducible   = isIrreducible;
	}

	#endregion

	#region Instance properties

	public BigInteger Denominator
		=> m_explicitConstructorCalled ? m_denominator : 1; //We want default value to be zero, not NaN.

	public RationalNumber FractionalPart
	{
		get
		{
			if (Denominator != 0) {
				if (IsProper(this))
					return new RationalNumber(m_numerator % Denominator, Denominator);

				return new RationalNumber(BigInteger.Abs(m_numerator % Denominator), Denominator);
			}

			if (m_numerator == 0)
				return NaN;

			if (m_numerator > 0)
				return PositiveInfinity;

			return NegativeInfinity;
		}
	}

	public RationalNumber IntegerPart
	{
		get
		{
			if (Denominator != 0)
				return (BigInteger) this;

			if (m_numerator == 0)
				return NaN;

			if (m_numerator > 0)
				return PositiveInfinity;

			return NegativeInfinity;
		}
	}

	public BigInteger Numerator => m_numerator;

	public RationalNumber Sign => m_numerator.Sign;

	#endregion

	#region Instance methods

	public int CompareTo(RationalNumber other)
	{
		//Even though neither infinities nor NaNs are equal to themselves, for 
		//comparison's sake it makes sense to return 0 when comparing PositiveInfinities
		//or NaNs, etc. The only other option would be to throw an exception...yuck.

		if (IsNaN(other))
			return IsNaN(this) ? 0 : 1;

		if (IsNaN(this))
			return IsNaN(other) ? 0 : -1;

		if (IsPositiveInfinity(this))
			return IsPositiveInfinity(other) ? 0 : 1;

		if (IsNegativeInfinity(this))
			return IsNegativeInfinity(other) ? 0 : -1;

		if (IsPositiveInfinity(other))
			return IsPositiveInfinity(this) ? 0 : -1;

		if (IsNegativeInfinity(other))
			return IsNegativeInfinity(this) ? 0 : 1;

		return (m_numerator * other.Denominator).CompareTo(Denominator * other.m_numerator);
	}

	public int CompareTo(object obj)
	{
		if (obj is RationalNumber rational)
			return CompareTo(rational);

		if (obj == null)
			return 1;

		throw new ArgumentException("obj is not a RationalNumber.", nameof(obj));
	}

	public bool Equals(RationalNumber other)
	{
		if (Denominator == 0 || other.Denominator == 0) //By definition NaNs and infinities are not equal.
			return false;

		return m_numerator * other.Denominator == Denominator * other.m_numerator;
	}

	public override bool Equals(object obj)
	{
		if (obj is RationalNumber rational)
			return Equals(rational);

		return false;
	}

	[DebuggerStepThrough]
	public override int GetHashCode()
	{
		if (m_isDefinitelyIrreducible) {
			unchecked {
				return m_numerator.GetHashCode() ^ Denominator.GetHashCode();
			}
		}

		return GetReducedForm(this).GetHashCode();
	}

	[DebuggerStepThrough]
	public override string ToString() => ToString(null, null);

	[DebuggerStepThrough]
	public string ToString(string format) => ToString(format, null);

	[DebuggerStepThrough]
	public string ToString(IFormatProvider formatProvider) => ToString(null, formatProvider);

	public string ToString(string format, IFormatProvider formatProvider)
	{
		try {
			if (formatProvider is RationalFormatProvider provider)
				return provider.Format(
					format ?? "G", this, CultureInfo.CurrentCulture);

			var rationalFormatProvider = new RationalFormatProvider();
			return rationalFormatProvider.Format(format ?? "G", this, formatProvider ?? CultureInfo.CurrentCulture);
		}
		catch (FormatException e) {
			throw new FormatException($"The specified format string '{format}' is invalid.", e);
		}
	}

	#endregion

	#region Static properties

	public static bool IsInfinity(RationalNumber rationalNumber)
	{
		return IsPositiveInfinity(rationalNumber) ||
		       IsNegativeInfinity(rationalNumber);
	}

	public static bool IsIrreducible(RationalNumber rationalNumber)
	{
		if (rationalNumber.m_isDefinitelyIrreducible)
			return true;

		return rationalNumber.Denominator == 1 ||
		       (rationalNumber.Denominator == 0 &&
		        (rationalNumber.m_numerator == 1 || rationalNumber.m_numerator == -1 ||
		         rationalNumber.m_numerator == 0)) ||
		       MathHelper.GCD(rationalNumber.m_numerator, rationalNumber.Denominator) == 1;

	}

	public static bool IsPositiveInfinity(RationalNumber rationalNumber)
	{
		return rationalNumber.Denominator == 0 &&
		       rationalNumber.m_numerator >
		       0; //Can not check using rationalNumber == positiveInfinity because by definition
		//infinities are not equal.
	}

	public static bool IsProper(RationalNumber rationalNumber)
	{
		return BigInteger.Abs(rationalNumber.IntegerPart.m_numerator) < 1;
	}

	public static bool IsNaN(RationalNumber rationalNumber)
	{
		return rationalNumber.Denominator == 0 &&
		       rationalNumber.m_numerator ==
		       0; //Can not check using rationalNumber == naN because by definition NaN are not equal.
	}

	public static bool IsNegativeInfinity(RationalNumber rationalNumber)
	{
		return rationalNumber.Denominator == 0 &&
		       rationalNumber.m_numerator <
		       0; //Can not check using rationalNumber == negativeInfinity because by definition
		//infinities are not equal.
	}

	public static RationalNumber One { get; } = new(1);

	public static RationalNumber PositiveInfinity { get; } = new(1, 0, true);

	public static RationalNumber NaN { get; } = new(0, 0, true);

	public static RationalNumber NegativeInfinity { get; } = new(-1, 0, true);

	public static RationalNumber Zero { get; } = new(0);

	#endregion

	#region Static methods

	public static RationalNumber Abs(RationalNumber number)
	{
		return new(BigInteger.Abs(number.m_numerator), number.Denominator);
	}

	public static RationalNumber Add(RationalNumber left, RationalNumber right, bool reduceOutput = false)
	{
		return reduceOutput ? GetReducedForm(left + right) : left + right;
	}

	public static RationalNumber Ceiling(RationalNumber number)
	{
		if (number.FractionalPart == Zero)
			return number.IntegerPart;

		if (number < Zero)
			return number.IntegerPart;

		return number.IntegerPart + 1;
	}

	public static RationalNumber Divide(RationalNumber left, RationalNumber right, bool reduceOutput = false)
	{
		return reduceOutput ? GetReducedForm(left / right) : left / right;
	}

	public static RationalNumber Floor(RationalNumber number)
	{
		if (number.FractionalPart == Zero)
			return number.IntegerPart;

		if (number < Zero)
			return number.IntegerPart - 1;

		return number.IntegerPart;
	}

	public static RationalNumber FromDouble(double target, double precision)
	{

		if (!TryFromDouble(target, precision, out RationalNumber result))
			throw new ArgumentException("Can not find a rational approximation with the specified precision.",
			                            nameof(precision));

		return result;
	}

	public static RationalNumber GetReciprocal(RationalNumber rationalNumber)
	{
		return new(rationalNumber.Denominator, rationalNumber.m_numerator,
		           rationalNumber.m_isDefinitelyIrreducible);
	}

	public static RationalNumber GetReducedForm(RationalNumber rationalNumber)
	{
		if (rationalNumber.m_isDefinitelyIrreducible) {
			return rationalNumber;
		}

		var greatestCommonDivisor =
			MathHelper.GCD(rationalNumber.m_numerator, rationalNumber.Denominator);

		return new RationalNumber(rationalNumber.m_numerator / greatestCommonDivisor,
		                          rationalNumber.Denominator / greatestCommonDivisor, true);
	}

	public static RationalNumber Max(RationalNumber first, RationalNumber second)
	{
		return first >= second ? first : second;

	}

	public static RationalNumber Min(RationalNumber first, RationalNumber second)
		=> first <= second ? first : second;

	public static RationalNumber Multiply(RationalNumber left, RationalNumber right, bool reduceOutput = false)
		=> reduceOutput ? GetReducedForm(left * right) : left * right;

	public static RationalNumber Negate(RationalNumber right, bool reduceOutput = false)
		=> reduceOutput ? GetReducedForm(-right) : -right;

	public static RationalNumber Pow(RationalNumber r, int n, bool reduceOutput = false)
	{
		if (IsNaN(r)) {
			return NaN;
		}

		if (n > 0) {
			var result = new RationalNumber(BigInteger.Pow(r.m_numerator, n), BigInteger.Pow(r.Denominator, n),
			                                false);
			return reduceOutput ? GetReducedForm(result) : result;
		}

		if (n < 0) {
			return Pow(GetReciprocal(r), -n, reduceOutput);
		}

		if (r == Zero || IsInfinity(r)) {
			return NaN;
		}

		return One;
	}

	public static RationalNumber Subtract(RationalNumber left, RationalNumber right, bool reduceOutput = false)
		=> reduceOutput ? GetReducedForm(left - right) : left - right;

	public static double ToDouble(RationalNumber rationalNumber)
		=> ((double) rationalNumber.m_numerator) / (double) rationalNumber.Denominator;

	public static RationalNumber Truncate(RationalNumber number) => number.IntegerPart;

	public static bool TryFromDouble(double target, double precision, out RationalNumber result)
	{
		//Continued fraction algorithm: http://en.wikipedia.org/wiki/Continued_fraction
		//Implemented recursively. Problem is figuring out when precision is met without unwinding each solution. Haven't figured out how to do that.
		//Current implementation computes rational number approximations for increasing algorithm depths until precision criteria is met, maximum depth is reached (fromDoubleMaxIterations)
		//or an OverflowException is thrown. Efficiency is probably improvable but this method will not be used in any performance critical code. No use in optimizing it unless there is
		//a good reason. Current implementation works reasonably well.

		result = Zero;
		int steps = 0;

		while (Math.Abs(target - ToDouble(result)) > precision) {
			if (steps > FROM_DOUBLE_MAX_ITERATIONS) {
				result = Zero;
				return false;
			}

			result = GetNearestRationalNumber(target, 0, steps++);
		}

		return true;
	}

	private static RationalNumber GetNearestRationalNumber(double number, int currentStep, int maximumSteps)
	{
		var    integerPart    = (BigInteger) number;
		double fractionalPart = number - Math.Truncate(number);

		while (currentStep < maximumSteps && fractionalPart != 0) {
			return integerPart +
			       new RationalNumber(1, GetNearestRationalNumber(1 / fractionalPart, ++currentStep, maximumSteps));
		}

		return new RationalNumber(integerPart);
	}

	#endregion

	#region Operators

	public static explicit operator double(RationalNumber rationalNumber)
	{
		return ToDouble(rationalNumber);
	}

	public static implicit operator RationalNumber(BigInteger number)
	{
		return new RationalNumber(number);
	}

	public static implicit operator RationalNumber(long number)
	{
		return new RationalNumber(number);
	}

	public static explicit operator BigInteger(RationalNumber rationalNumber)
	{
		return rationalNumber.m_numerator / rationalNumber.Denominator;
	}

	public static bool operator ==(RationalNumber left, RationalNumber right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(RationalNumber left, RationalNumber right)
	{
		return !left.Equals(right);
	}

	public static bool operator >(RationalNumber left, RationalNumber right)
	{
		return left.CompareTo(right) > 0;
	}

	public static bool operator >=(RationalNumber left, RationalNumber right)
	{
		return left.CompareTo(right) >= 0;
	}

	public static bool operator <(RationalNumber left, RationalNumber right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator <=(RationalNumber left, RationalNumber right)
	{
		return left.CompareTo(right) <= 0;
	}

	public static RationalNumber operator +(RationalNumber right)
	{
		return right;
	}

	public static RationalNumber operator -(RationalNumber right)
	{
		return new RationalNumber(-right.m_numerator, right.Denominator, right.m_isDefinitelyIrreducible);
	}

	public static RationalNumber operator +(RationalNumber left, RationalNumber right)
	{
		if ((IsPositiveInfinity(left) &&
		     IsPositiveInfinity(
			     right)) || //Otherwise the sum of two equally signed infinities would return NaN which is not correct.
		    (IsNegativeInfinity(left) && IsNegativeInfinity(right)))
			return left;

		return new RationalNumber(left.Numerator * right.Denominator + right.m_numerator * left.Denominator,
		                          left.Denominator * right.Denominator, false);
	}

	public static RationalNumber operator -(RationalNumber left, RationalNumber right)
	{
		return left + (-right);
	}

	public static RationalNumber operator *(RationalNumber left, RationalNumber right)
	{
		return new RationalNumber(left.m_numerator * right.m_numerator, left.Denominator * right.Denominator,
		                          false);
	}

	public static RationalNumber operator /(RationalNumber left, RationalNumber right)
	{
		if ((IsInfinity(left) && IsInfinity(right)) ||
		    (left == Zero && right == 0))
			return NaN;

		return new RationalNumber(left.m_numerator * right.Denominator, left.Denominator * right.m_numerator,
		                          false);
	}

	#endregion

	[DebuggerStepThrough]
	private class RationalFormatProvider : IFormatProvider, ICustomFormatter
	{
		public object GetFormat(Type formatType)
		{
			if (formatType == typeof(ICustomFormatter))
				return this;

			return null;
		}

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			var upperFormat = format != null ? format.ToUpperInvariant().TrimStart() : "G";

			if (arg is not RationalNumber rational)
				return HandleOtherFormats(format, arg, formatProvider);

			if (rational.Denominator == 0) {
				if (rational.m_numerator == 0) {
					return NA_N_LITERAL;
				}

				if (rational.m_numerator > 0) {
					return POSITIVE_INFINITY_LITERAL;
				}

				return NEGATIVE_INFINITY_LITERAL;
			}

			string innerFormat = format;

			if (upperFormat[0] == 'M') {
				innerFormat = format.Replace('M', 'G');
				var integerPart = rational.IntegerPart.Numerator;

				if (integerPart != 0) {
					var fractionalPart = rational.FractionalPart;

					return String.Format("{0} [{1}/{2}]", integerPart.ToString(innerFormat, formatProvider),
					                     BigInteger.Abs(fractionalPart.m_numerator)
					                               .ToString(innerFormat, formatProvider),
					                     fractionalPart.Denominator.ToString(innerFormat, formatProvider));
				}
			}

			return String.Format("{0}/{1}", rational.m_numerator.ToString(innerFormat, formatProvider),
			                     rational.Denominator.ToString(innerFormat, formatProvider));
		}

		private static string HandleOtherFormats(string format, object arg, IFormatProvider formatProvider)
		{
			if (arg is IFormattable formattable)
				return formattable.ToString(format, formatProvider);

			if (arg != null)
				return arg.ToString();

			return String.Empty;
		}
	}
}