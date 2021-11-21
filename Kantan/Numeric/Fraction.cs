using System;
using System.Globalization;

// ReSharper disable NonReadonlyMemberInGetHashCode

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global

// ReSharper disable IdentifierTypo

namespace Kantan.Numeric;
/* Adapted from https://stackoverflow.com/questions/7564906/convert-double-to-fraction-as-string-in-c-sharp
 *
 * Originally developed by Syed Mehroz Alam (smehrozalam@yahoo.com)
 */

public struct Fraction : IEquatable<Fraction>, ICloneable, IComparable<Fraction>, IComparable
{
	private long m_iDenominator;

	public Fraction(long wholeNumber) : this(wholeNumber, 1) { }

	public Fraction(double decimalValue) : this(ToFraction(decimalValue)) { }

	public Fraction(string strValue) : this(ToFraction(strValue)) { }

	public Fraction(Fraction f) : this(f.Numerator, f.Denominator) { }

	public Fraction(long numerator, long denominator)
	{
		Numerator      = numerator;
		m_iDenominator = denominator;
		Reduce(this);
	}

	public long Denominator
	{
		get => m_iDenominator;
		set => EnsureDenominator(value);
	}

	private void EnsureDenominator(long value)
	{
		if (value != 0)
			m_iDenominator = value;
		else {

			//throw new FractionException("Denominator cannot be assigned a ZERO value");
		}
	}

	public long Numerator { get; set; }

	public long Int64Bits
	{
		get => BitConverter.DoubleToInt64Bits(ToDouble());
		set => this = BitConverter.Int64BitsToDouble(value);
	}

	public long WholeNumberValue
	{
		set
		{
			Numerator      = value;
			m_iDenominator = 1;
		}
	}

	public static readonly Fraction Zero = new(0, 0);

	public double Value
	{
		get => ToDouble();
		set => this = ToFraction(value);
	}

	public double ToDouble()
	{
		return (double) Numerator / Denominator;
	}

	public float ToFloat()
	{
		return (float) Numerator / Denominator;
	}

	public decimal ToDecimal() => (decimal) (ToDouble());

	public override string ToString()
	{
		return Denominator switch
		{
			1                     => Numerator.ToString(),
			0 when Numerator == 0 => 0.ToString(),
			_                     => Numerator + "/" + Denominator,
		};
	}

	public int CompareTo(object obj)
	{
		if (obj is Fraction f) {
			return Value.CompareTo(f);
		}

		throw new ArgumentException();
	}

	public object Clone() => Copy();

	public static Fraction ToFraction(string strValue)
	{
		int i;

		for (i = 0; i < strValue.Length; i++) {
			if (strValue[i] == '/') {
				break;
			}
		}

		// if string is not in the form of a fraction
		if (i == strValue.Length) {
			// then it is double or integer
			return Convert.ToDouble(strValue);
		}
		//return ( ToFraction( Convert.ToDouble(strValue) ) );

		// else string is in the form of Numerator/Denominator
		long iNumerator   = Convert.ToInt64(strValue[..i]);
		long iDenominator = Convert.ToInt64(strValue[(i + 1)..]);
		return new Fraction(iNumerator, iDenominator);
	}

	public static Fraction ToFraction(double value)
	{
		try {
			checked {
				Fraction frac;

				// if whole number
				if (value % 1 == 0) {
					frac = new Fraction((long) value);
				}
				else {
					double dTemp     = value;
					long   iMultiple = 1;
					string strTemp   = value.ToString(CultureInfo.InvariantCulture);

					while (strTemp.IndexOf("E", StringComparison.Ordinal) > 0) // if in the form like 12E-9
					{
						dTemp     *= 10;
						iMultiple *= 10;
						strTemp   =  dTemp.ToString(CultureInfo.InvariantCulture);
					}

					int i = 0;

					while (strTemp[i] != '.')
						i++;
					int iDigitsAfterDecimal = strTemp.Length - i - 1;

					while (iDigitsAfterDecimal > 0) {
						dTemp     *= 10;
						iMultiple *= 10;
						iDigitsAfterDecimal--;
					}

					frac = new Fraction((int) Math.Round(dTemp), iMultiple);
				}

				return frac;
			}
		}
		catch (OverflowException) {
			throw new FractionException("Conversion not possible due to overflow");
		}
		catch (Exception) {
			throw new FractionException("Conversion not possible");
		}
	}

	public Fraction Copy()
	{
		var frac = new Fraction
		{
			Numerator   = Numerator,
			Denominator = Denominator
		};
		return frac;
	}

	public Fraction Invert() => Inverse(this);

	public static Fraction Inverse(Fraction frac1)
	{
		if (frac1.Numerator == 0)
			throw new FractionException("Operation not possible (Denominator cannot be assigned a ZERO Value)");

		long iNumerator   = frac1.Denominator;
		long iDenominator = frac1.Numerator;
		return new Fraction(iNumerator, iDenominator);
	}

	#region Operators

	public static Fraction operator -(Fraction frac1) => Negate(frac1);

	public static Fraction operator +(Fraction frac1, Fraction frac2) => Add(frac1, frac2);

	public static Fraction operator +(int iNo, Fraction frac1) => Add(frac1, new Fraction(iNo));

	public static Fraction operator +(Fraction frac1, int iNo) => Add(frac1, new Fraction(iNo));

	public static Fraction operator +(double dbl, Fraction frac1) => Add(frac1, ToFraction(dbl));

	public static Fraction operator +(Fraction frac1, double dbl) => Add(frac1, ToFraction(dbl));

	public static Fraction operator -(Fraction frac1, Fraction frac2) => Add(frac1, -frac2);

	public static Fraction operator -(int iNo, Fraction frac1) => Add(-frac1, new Fraction(iNo));

	public static Fraction operator -(Fraction frac1, int iNo) => Add(frac1, -new Fraction(iNo));

	public static Fraction operator -(double dbl, Fraction frac1) => Add(-frac1, ToFraction(dbl));

	public static Fraction operator -(Fraction frac1, double dbl) => Add(frac1, -ToFraction(dbl));

	public static Fraction operator *(Fraction frac1, Fraction frac2) => Multiply(frac1, frac2);

	public static Fraction operator *(int iNo, Fraction frac1) => Multiply(frac1, new Fraction(iNo));

	public static Fraction operator *(Fraction frac1, int iNo) => Multiply(frac1, new Fraction(iNo));

	public static Fraction operator *(double dbl, Fraction frac1) => Multiply(frac1, ToFraction(dbl));

	public static Fraction operator *(Fraction frac1, double dbl) => Multiply(frac1, ToFraction(dbl));

	public static Fraction operator /(Fraction frac1, Fraction frac2) => Multiply(frac1, Inverse(frac2));

	public static Fraction operator /(int iNo, Fraction frac1) => Multiply(Inverse(frac1), new Fraction(iNo));

	public static Fraction operator /(Fraction frac1, int iNo) => Multiply(frac1, Inverse(new Fraction(iNo)));

	public static Fraction operator /(double dbl, Fraction frac1) => Multiply(Inverse(frac1), ToFraction(dbl));

	public static Fraction operator /(Fraction frac1, double dbl) => Multiply(frac1, Inverse(ToFraction(dbl)));

	public static bool operator ==(Fraction frac1, Fraction frac2) => frac1.Equals(frac2);

	public static bool operator !=(Fraction frac1, Fraction frac2) => !frac1.Equals(frac2);

	public static bool operator ==(Fraction frac1, int iNo) => frac1.Equals(new Fraction(iNo));

	public static bool operator !=(Fraction frac1, int iNo) => !frac1.Equals(new Fraction(iNo));

	public static bool operator ==(Fraction frac1, double dbl) => frac1.Equals(new Fraction(dbl));

	public static bool operator !=(Fraction frac1, double dbl) => !frac1.Equals(new Fraction(dbl));

	public static bool operator <(Fraction frac1, Fraction frac2)
		=> frac1.Numerator * frac2.Denominator < frac2.Numerator * frac1.Denominator;

	public static bool operator >(Fraction frac1, Fraction frac2)
		=> frac1.Numerator * frac2.Denominator > frac2.Numerator * frac1.Denominator;

	public static bool operator <=(Fraction frac1, Fraction frac2)
		=> frac1.Numerator * frac2.Denominator <= frac2.Numerator * frac1.Denominator;

	public static bool operator >=(Fraction frac1, Fraction frac2)
		=> frac1.Numerator * frac2.Denominator >= frac2.Numerator * frac1.Denominator;

	#endregion

	public static implicit operator Fraction(long num) => new(num);

	public static implicit operator Fraction(double num) => new(num);

	public static implicit operator Fraction(string strNum) => new(strNum);

	public static explicit operator double(Fraction frac) => frac.ToDouble();

	public static explicit operator float(Fraction frac) => frac.ToFloat();

	public static explicit operator decimal(Fraction frac) => frac.ToDecimal();

	public static implicit operator string(Fraction frac) => frac.ToString();

	public int CompareTo(Fraction other)
	{
		return Value.CompareTo(other.Value);
	}

	public override bool Equals(object obj)
	{
		if (obj is Fraction frac) {
			return Numerator == frac.Numerator && Denominator == frac.Denominator;
		}

		return false;
	}

	public override int GetHashCode()
	{
		unchecked {
			return (m_iDenominator.GetHashCode() * 397) ^ Numerator.GetHashCode();
		}
	}

	private static Fraction Negate(Fraction frac1) => new(-frac1.Numerator, frac1.Denominator);

	private static Fraction Add(Fraction frac1, Fraction frac2)
	{
		try {
			checked {
				long numerator   = frac1.Numerator * frac2.Denominator + frac2.Numerator * frac1.Denominator;
				long denominator = frac1.Denominator * frac2.Denominator;
				return new Fraction(numerator, denominator);
			}
		}
		catch (OverflowException) {
			throw new FractionException("Overflow occurred while performing arithmetic operation");
		}
		catch (Exception) {
			throw new FractionException("An error occurred while performing arithmetic operation");
		}
	}

	private static Fraction Multiply(Fraction frac1, Fraction frac2)
	{
		try {
			checked {
				long numerator   = frac1.Numerator * frac2.Numerator;
				long denominator = frac1.Denominator * frac2.Denominator;
				return new Fraction(numerator, denominator);
			}
		}
		catch (OverflowException) {
			throw new FractionException("Overflow occurred while performing arithmetic operation");
		}
		catch (Exception) {
			throw new FractionException("An error occurred while performing arithmetic operation");
		}
	}

	private static long GCD(long iNo1, long iNo2)
	{
		// take absolute values
		if (iNo1 < 0) {
			iNo1 = -iNo1;
		}

		if (iNo2 < 0) {
			iNo2 = -iNo2;
		}

		do {
			if (iNo1 < iNo2) {
				(iNo1, iNo2) = (iNo2, iNo1);
			}

			iNo1 %= iNo2;
		} while (iNo1 != 0);

		return iNo2;
	}

	public Fraction Reduce() => Reduce(this);

	public static Fraction Reduce(Fraction frac1)
	{
		var frac = frac1.Copy();

		try {
			if (frac.Numerator == 0) {
				frac.Denominator = 1;
				return frac;
			}

			long iGCD = GCD(frac.Numerator, frac.Denominator);
			frac.Numerator   /= iGCD;
			frac.Denominator /= iGCD;

			// if -ve sign in denominator
			if (frac.Denominator < 0) {
				//pass -ve sign to numerator
				frac.Numerator   *= -1;
				frac.Denominator *= -1;
			}
		} // end try
		catch (Exception exp) {
			throw new FractionException("Cannot reduce Fraction: " + exp.Message);
		}

		return frac;
	}

	public bool Equals(Fraction other)
	{
		return m_iDenominator == other.m_iDenominator && Numerator == other.Numerator;
	}
}

public sealed class FractionException : Exception
{
	public FractionException() : base() { }

	public FractionException(string message) : base(message) { }

	public FractionException(string message, Exception innerException) : base(message, innerException) { }
}