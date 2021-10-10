#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using JetBrains.Annotations;
using Kantan.Collections;
using Kantan.Model;
using static Kantan.Internal.Common;

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

// ReSharper disable InconsistentNaming

#pragma warning disable 8509

// ReSharper disable UnusedMember.Local

// ReSharper disable StringIndexOfIsCultureSpecific.1

// ReSharper disable UnusedMember.Global


namespace Kantan.Text
{
	/// <summary>
	///     Utilities for strings (<see cref="string" />).
	/// </summary>
	/// <seealso cref="string"/>
	/// <seealso cref="char"/>
	/// <seealso cref="CharUnicodeInfo"/>
	/// <seealso cref="UnicodeCategory"/>
	/// <seealso cref="UnicodeRanges"/>
	public static partial class Strings
	{
		public static string SelectOnlyDigits(this string s) => s.SelectOnly(Char.IsDigit);

		public static string StripControl(this string s) => s.SelectOnly(c => !Char.IsControl(c));

		public static string SelectOnly(this string s, Func<char, bool> fn)
		{
			return s.Where(fn).Aggregate(String.Empty, (current, t) => current + t);
		}

		public static string CleanString(this string s)
		{
			//return s.Replace("\"", String.Empty);

			return s.Trim('\"');
		}

		public static string Truncate(this string value)
		{
			//return value.Truncate(Console.WindowWidth - 5);
			return value.Truncate(100);
		}

		public static string Truncate(this string value, int maxLength)
		{
			if (String.IsNullOrEmpty(value)) {
				return value;
			}

			return value.Length <= maxLength ? value : value[..maxLength];
		}

		[CanBeNull]
		public static string NormalizeNull([CanBeNull] string str) => String.IsNullOrWhiteSpace(str) ? null : str;

		public static int MeasureRows(string s)
		{
			var bufferWidth = Console.BufferWidth;
			var windowWidth = Console.WindowWidth;

			var nc = s.Count(char.IsControl);

			//var nc = s.Count(c => c=='\n'||c=='\r');
			int nc1 = 0;

			for (int i = 0; i < s.Length - 1; i++) {
				switch (s[i]) {
					case '\r' when s[i + 1] == '\n':
					case '\n':
						nc1++;
						break;
				}

			}

			// var nc1 = s.Count(c => c == '\n');

			// var nc2 = Regex.Matches(s, Environment.NewLine).Count;
			// var nc2x = s.Split(Environment.NewLine).Length;


			var length = s.Length - nc;

			// var length = s.Length - nc1;
			// var length = s.Length;

			var n = Math.DivRem(length, bufferWidth, out var rem);

			//(s.Length % b)

			if (rem > 0) {
				n++;
			}

			n += nc1;
			// n += nc2;

			/*int l = s.Length;
			int c = 1;
			while (--l>0) {
				if (s[l]=='\n') {
					c++;
				}
				else if (l % bufferWidth == 0) {
					c++;
				}
				
			}

			return c;*/

			return n;
		}

		public static bool StringWraps(string s)
		{
			/*
			 * Assuming buffer width equals window width
			 *
			 * If 'Wrap text output on resize' is ticked, this is true
			 */

			return s.Length >= Console.WindowWidth;
		}

		/// <summary>Convert a word that is formatted in pascal case to have splits (by space) at each upper case letter.</summary>
		public static string SplitPascalCase(string convert)
		{
			return Regex.Replace(Regex.Replace(convert, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"),
			                     @"(\p{Ll})(\P{Ll})", "$1 $2");
		}

		public static string CreateRandom(int length)
		{
			return new(Enumerable.Repeat(Constants.Alphanumeric, length)
			                     .Select(s => s[RandomInstance.Next(s.Length)])
			                     .ToArray());
		}

		public static IEnumerable<int> AllIndexesOf(this string str, string search)
		{
			return EnumerableHelper.AllIndexesOf((s, i) => str.IndexOf(s, i, StringComparison.Ordinal),
			                                     search.Length, search);
		}

		public static string RemoveLastOccurrence(this string s, string s2)
			=> s.Remove(s.LastIndexOf(s2, StringComparison.Ordinal));

		/// <summary>
		///     Compute the Levenshtein distance (approximate string matching) between <paramref name="s"/> and <paramref name="t"/>
		/// </summary>
		public static int Compute(string s, string t)
		{
			int    n = s.Length;
			int    m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0)
				return m;

			if (m == 0)
				return n;

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++) { }

			for (int j = 0; j <= m; d[0, j] = j++) { }

			// Step 3
			for (int i = 1; i <= n; i++) //Step 4
			for (int j = 1; j <= m; j++) {
				// Step 5
				int cost = t[j - 1] == s[i - 1] ? 0 : 1;

				// Step 6
				d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
			}

			// Step 7
			return d[n, m];
		}

		#region Substring

		/// <summary>
		///     Simulates Java substring function
		/// </summary>
		public static string JSubstring(this string s, int beginIndex)
		{
			return s[beginIndex..];
		}

		/// <summary>
		///     Simulates Java substring function
		/// </summary>
		public static string JSubstring(this string s, int beginIndex, int endIndex)
		{
			return s.Substring(beginIndex, endIndex - beginIndex + 1);
		}

		/// <summary>
		///     Simulates Java substring function
		/// </summary>
		public static string JSubstring(this string s, Range r)
		{
			return s.JSubstring(r.Start.Value, r.End.Value);
		}

		/// <summary>
		///     Simulates Java substring function
		/// </summary>
		public static string JSubstring(this string s, Index i)
		{
			return s.JSubstring(i.Value);
		}


		/// <summary>
		///     <returns>String value after [last] <paramref name="a" /></returns>
		/// </summary>
		public static string SubstringAfter(this string value, string a)
		{
			int posA = value.LastIndexOf(a, StringComparison.Ordinal);

			if (posA == INVALID) {
				return String.Empty;
			}

			int adjustedPosA = posA + a.Length;
			return adjustedPosA >= value.Length ? String.Empty : value[adjustedPosA..];
		}

		/// <summary>
		///     <returns>String value after [first] <paramref name="a" /></returns>
		/// </summary>
		public static string SubstringBefore(this string value, string a)
		{
			int posA = value.IndexOf(a, StringComparison.Ordinal);
			return posA == INVALID ? String.Empty : value[..posA];
		}

		/// <summary>
		///     <returns>String value between [first] <paramref name="a" /> and [last] <paramref name="b" /></returns>
		/// </summary>
		public static string SubstringBetween(this string value, string a, string b)
		{
			int posA = value.IndexOf(a, StringComparison.Ordinal);
			int posB = value.LastIndexOf(b, StringComparison.Ordinal);

			if (posA == INVALID || posB == INVALID) {
				return String.Empty;
			}

			int adjustedPosA = posA + a.Length;
			return adjustedPosA >= posB ? String.Empty : value[adjustedPosA..posB];
		}

		#endregion

		#region Formatting

		public static string Center(string str, int width)
		{
			//https://stackoverflow.com/questions/48621267/is-there-a-way-to-center-text-in-powershell


			var count = (int) (Math.Max(0, width / 2) - Math.Floor((double) str.Length / 2));

			return $"{new string(' ', count)}{str}";

		}

		public static string Center(string str) => Center(str, Console.BufferWidth);

		#region Outline

		public static string Indent(string s) => Indent(s, Constants.Indentation);

		public static string Indent(string s, string indent)
		{
			string[] split = s.Split('\n');

			string j = String.Join($"\n{indent}", split);

			return indent + j;
		}


		public static string OutlineString(IOutline view)
		{
			var esb = new StringBuilder();

			foreach (var (key, value) in view.Outline) {
				switch (value) {
					case null:
						continue;
					case IOutline view2:
						esb.Append(OutlineString(view2));
						break;
					default:
						esb.Append(key, value);
						break;
				}

			}

			return esb.ToString();
		}

		#endregion

		#region Hex

		private static HexFormatter Hex { get; } = new();

		public sealed class HexFormatter : ICustomFormatter
		{
			public string Format(string fmt, object arg, IFormatProvider formatProvider)
			{
				fmt ??= FMT_P;

				fmt = fmt.ToUpper(CultureInfo.InvariantCulture);
				string hexStr;

				if (arg is IFormattable f) {
					hexStr = f.ToString(HEX_FORMAT_SPECIFIER, null);
				}
				else {
					throw new NotImplementedException();
				}

				var sb = new StringBuilder();


				switch (fmt) {
					case FMT_P:
						sb.Append(HEX_PREFIX);
						goto case FMT_X;
					case FMT_X:
						sb.Append(hexStr);
						break;
					default:
						return arg.ToString();
				}

				return sb.ToString();

			}

			public const string HEX_FORMAT_SPECIFIER = "X";

			public const string HEX_PREFIX = "0x";

			public const string FMT_X = "X";
			public const string FMT_P = "P";
		}

		public static string ToHexString<T>(T t, string s = HexFormatter.FMT_P)
			=> Hex.Format(s, t, CultureInfo.CurrentCulture);

		#endregion

		#endregion


		#region Join

		public static string FormatJoin<T>(this IEnumerable<T> values, string format, IFormatProvider provider = null,
		                                   string delim = Constants.JOIN_COMMA) where T : IFormattable
		{

			return values.Select(v => v.ToString(format, provider)).QuickJoin(delim);
		}

		/// <summary>
		///     Concatenates the strings returned by <paramref name="toString" />
		///     using the specified separator between each element or member.
		/// </summary>
		/// <param name="values">Collection of values</param>
		/// <param name="toString">
		///     Function which returns a <see cref="string" /> given a member of <paramref name="values" />
		/// </param>
		/// <param name="delim">Delimiter</param>
		/// <typeparam name="T">Element type</typeparam>
		public static string FuncJoin<T>(this IEnumerable<T> values, Func<T, string> toString,
		                                 string delim = Constants.JOIN_COMMA)
		{
			return values.Select(toString).QuickJoin(delim);
		}

		public static string QuickJoin<T>(this IEnumerable<T> enumerable, string delim = Constants.JOIN_COMMA)
		{
			return String.Join(delim, enumerable);
		}

		#endregion

		private static readonly Encoding EncodingOEM =
			CodePagesEncodingProvider.Instance.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);

		public static string EncodingConvert(Encoding src, Encoding dest, string str)
			=> dest.GetString(Encoding.Convert(src, dest, src.GetBytes(str)));

		public static string EncodingConvert(Encoding src, string str) => EncodingConvert(src, EncodingOEM, str);

		public static bool IsCharInRange(short c, UnicodeRange r) => IsCharInRange((char) c, r);

		public static bool IsCharInRange(char c, UnicodeRange r)
			=> c < r.FirstCodePoint + r.Length && c >= r.FirstCodePoint;

		internal static string GetUnicodeBoxPipe(IList<string> l, int i)
		{
			string delim;

			if (i == 0 && l.Count == 2) {
				delim = Constants.Horizontal;
				return delim;
			}

			if (l.Skip(i + 1).All(String.IsNullOrWhiteSpace)) {
				return Constants.BottomLeftCorner;
			}

			delim = l switch
			{
				{ Count: 1 } => Constants.Horizontal,
				{ Count: 2 } => i switch
				{
					0 => Constants.UpperLeftCorner,
					1 => Constants.BottomLeftCorner,
				},
				_ => i switch
				{
					0   => Constants.UpperLeftCorner,
					> 0 => Constants.Vertical,
					_   => Constants.BottomLeftCorner,
				}
			};

			return delim;
		}
	}
}