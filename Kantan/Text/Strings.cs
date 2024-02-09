#nullable disable
using System.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using JetBrains.Annotations;
using Kantan.Collections;
using Kantan.Text;
using Kantan.Utilities;
using Kantan.Model;

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Local

// ReSharper disable StringIndexOfIsCultureSpecific.1

// ReSharper disable UnusedMember.Global

namespace Kantan.Text;

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

	public static string RemoveNewLines(this string s)
		=> s.Remove(LineControlCharacters, (current, t) => current.Remove(t));

	public static string Remove<T>(this string s, T[] rg, Func<string, T, string> func)
		=> rg.Aggregate(s, func);

	// public static string Remove(this string s, char[] rg)   => rg.Aggregate(s, (current, t) => current.Remove(t));

	// public static string Remove(this string s, string[] rg) => rg.Aggregate(s, (current, t) => current.Remove(t));

	public static string Remove(this string s, string s2)
		=> s.Replace(s2, string.Empty);

	public static string Remove(this string s, char s2)
		=> s.Remove(s2.ToString());

	public static string SelectOnlyDigits(this string s)
		=> s.WhereAggregate(Char.IsDigit);

	public static string StripControl(this string s)
		=> s.WhereAggregate(c => !Char.IsControl(c));

	/// <remarks>Select only</remarks>
	public static string WhereAggregate(this string s, Func<char, bool> fn)
		=> s.Where(fn).Aggregate(String.Empty, (current, t) => current + t);

	public static string CleanString(this string s)
		=> s.Trim('\"');

	[CBN]
	public static string NormalizeNull([CBN] string str)
		=> String.IsNullOrWhiteSpace(str) ? null : str;

	/// <summary>Convert a word that is formatted in pascal case to have splits (by space) at each upper case letter.</summary>
	public static string SplitPascalCase(string convert)
	{
		return Regex.Replace(_rgx1().Replace(convert, "$1 $2"),
		                     @"(\p{Ll})(\P{Ll})", "$1 $2");
	}

	public static string CreateRandom(int length)
	{
		return new(Enumerable.Repeat(Constants.Alphanumeric, length)
			           .Select(s => s[KantanInit.RandomInstance.Next(s.Length)])
			           .ToArray());
	}

	public static IEnumerable<int> IndexOfAll(this string str, string search,
	                                          StringComparison c = StringComparison.Ordinal)
	{
		return EnumerableHelper.IndexOfAll((s, i) => str.IndexOf(s, i, c),
		                                   search.Length, search);
	}

	public static string RemoveLastOccurrence(this string s, string s2, StringComparison c = StringComparison.Ordinal)
		=> s.Remove(s.LastIndexOf(s2, c));

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

		for (int i = 1; i <= n; i++) {
			// Step 4

			for (int j = 1; j <= m; j++) {
				// Step 5
				int cost = t[j - 1] == s[i - 1] ? 0 : 1;

				// Step 6
				d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
			}
		}

		// Step 7
		return d[n, m];
	}

	public static string GetMapString(Dictionary<string, string> map, Color? c = null)
	{
		return map.Select(kv =>
		{
			var key = kv.Key;

			if (c.HasValue) {
				key = key.AddColor(c.Value);
			}

			return $"{key}: {kv.Value}";
		}).QuickJoin(" | ");
	}

	#region Substring

	/// <summary>
	///     Simulates Java substring function
	/// </summary>
	public static string JSubstring(this string s, int beginIndex)
		=> s[beginIndex..];

	/// <summary>
	///     Simulates Java substring function
	/// </summary>
	public static string JSubstring(this string s, int beginIndex, int endIndex)
		=> s.Substring(beginIndex, endIndex - beginIndex + 1);

	/// <summary>
	///     Simulates Java substring function
	/// </summary>
	public static string JSubstring(this string s, Range r)
		=> s.JSubstring(r.Start.Value, r.End.Value);

	/// <summary>
	///     Simulates Java substring function
	/// </summary>
	public static string JSubstring(this string s, Index i)
		=> s.JSubstring(i.Value);

	/// <summary>
	///     <returns>String value after [last] <paramref name="a" /></returns>
	/// </summary>
	public static string SubstringAfter(this string value, string a)
	{
		int posA = value.LastIndexOf(a, StringComparison.Ordinal);

		if (posA == KantanInit.INVALID) {
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
		return posA == KI.INVALID ? String.Empty : value[..posA];
	}

	/// <summary>
	///     <returns>String value between [first] <paramref name="a" /> and [last] <paramref name="b" /></returns>
	/// </summary>
	public static string SubstringBetween(this string value, string a, string b)
	{
		int posA = value.IndexOf(a, StringComparison.Ordinal);
		int posB = value.LastIndexOf(b, StringComparison.Ordinal);

		if (posA == KantanInit.INVALID || posB == KantanInit.INVALID) {
			return String.Empty;
		}

		int adjustedPosA = posA + a.Length;
		return adjustedPosA >= posB ? String.Empty : value[adjustedPosA..posB];
	}

	#endregion

#if OTHER

	public static string Truncate(this string value, int maxLength)
	{
		if (String.IsNullOrEmpty(value)) {
			return value;
		}

		return value.Length <= maxLength ? value : value[..maxLength];
	}

	#region Formatting

	public static string Center(string str, int width)
	{
		//https://stackoverflow.com/questions/48621267/is-there-a-way-to-center-text-in-powershell

		var count = (int) (Math.Max(0, width / 2) - Math.Floor((double) str.Length / 2));

		return $"{new string(' ', count)}{str}";

	}

	#region Outline

	public static string Indent(string s)
		=> Indent(s, Constants.Indentation);

	public static string Indent(string s, string indent)
	{
		string[] split = s.Split('\n');

		string j = String.Join($"\n{indent}", split);

		return indent + j;
	}

	#endregion

	/// <param name="paragraph">The value to write.</param>
	/// <param name="tabSize">The value that indicates the column width of tab characters.</param>
	public static IEnumerable<string> GetWrappedWordsTab(string paragraph, int tabSize = 8)
	{
		//https://stackoverflow.com/questions/20534318/make-console-writeline-wrap-words-instead-of-letters

		string[] lines = paragraph.Replace("\t", new string(' ', tabSize))
			.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

		for (int i = 0; i < lines.Length; i++) {
			string process = lines[i];
			var    wrapped = new List<string>();

			while (process.Length > Console.WindowWidth) {
				int wrapAt = process.LastIndexOf(' ', Math.Min(Console.WindowWidth - 1, process.Length));
				if (wrapAt <= 0) break;

				wrapped.Add(process[..wrapAt]);
				process = process.Remove(0, wrapAt + 1);
			}

			foreach (string wrap in wrapped) {
				yield return wrap;
			}

		}
	}

	public static IEnumerable<string> GetWrappedWordsWidth(string text, int width)
	{
		//https://stackoverflow.com/questions/20534318/make-console-writeline-wrap-words-instead-of-letters
		var forcedZones = _rgx2().Matches(text).ToList();
		var normalZones = _rgx3().Matches(text).ToList();

		int start = 0;

		while (start < text.Length) {
			var zone =
				forcedZones.Find(z => z.Index     >= start && z.Index <= start + width) ??
				normalZones.FindLast(z => z.Index >= start && z.Index <= start + width);

			if (zone == null) {
				yield return text.Substring(start, width);

				start += width;
			}
			else {
				yield return text[start..zone.Index];

				start = zone.Index + zone.Length;
			}
		}
	}

	#endregion

#endif

	#region Join

	public static string FormatJoin<T>(this IEnumerable<T> values, string format, IFormatProvider provider = null,
	                                   string delim = Constants.JOIN_COMMA) where T : IFormattable
		=> values.Select(v => v.ToString(format, provider)).QuickJoin(delim);

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
		=> values.Select(toString).QuickJoin(delim);

	public static string QuickJoin<T>(this IEnumerable<T> enumerable, string delim = Constants.JOIN_COMMA)
		=> String.Join(delim, enumerable);

	public static string QuickJoin(this IEnumerable enumerable, string delim = Constants.JOIN_COMMA)
		=> String.Join(delim, enumerable);

	#endregion

	private static readonly char[] LineControlCharacters = Environment.NewLine.ToCharArray();

	private static readonly Encoding EncodingOEM =
		CodePagesEncodingProvider.Instance.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);

	public static string EncodingConvert(Encoding src, Encoding dest, string str)
		=> dest.GetString(Encoding.Convert(src, dest, src.GetBytes(str)));

	public static string EncodingConvert(Encoding src, string str)
		=> EncodingConvert(src, EncodingOEM, str);

	public static bool IsCharInRange(ushort c, UnicodeRange r)
		=> c < r.FirstCodePoint + r.Length && c >= r.FirstCodePoint;

	public static bool IsCharInRange(short c, UnicodeRange r)
		=> IsCharInRange(c: unchecked((ushort) c), r);

	/*public static byte[] GetBytes(this string s, Encoding encoding)
		=> encoding.GetBytes(s);

	public static byte[] GetBytes(this string s)
		=> s.GetBytes(Encoding.UTF8);*/

	[GeneratedRegex("(\\P{Ll})(\\P{Ll}\\p{Ll})")]
	private static partial Regex _rgx1();

	[GeneratedRegex("\\n")]
	private static partial Regex _rgx2();

	[GeneratedRegex("\\s+|(?<=[-,.;])|$")]
	private static partial Regex _rgx3();

}