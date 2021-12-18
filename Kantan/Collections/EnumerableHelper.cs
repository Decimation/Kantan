using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using static Kantan.Internal.Common;
using Map = System.Collections.Generic.Dictionary<object, object>;

// ReSharper disable SuggestVarOrType_Elsewhere

// ReSharper disable AssignNullToNotNullAttribute

// ReSharper disable PossibleMultipleEnumeration

// ReSharper disable UnusedMember.Global

namespace Kantan.Collections;

/// <summary>
/// Utilities for collections (<see cref="IEnumerable{T}"/>) and associated types.
/// </summary>
public static class EnumerableHelper
{
	/// <summary>
	/// Determines whether <paramref name="list"/> ends with <paramref name="sequence"/>.
	/// </summary>
	/// <typeparam name="T"><see cref="List{T}"/> type</typeparam>
	/// <param name="list">Larger <see cref="List{T}"/></param>
	/// <param name="sequence">Smaller <see cref="List{T}"/></param>
	/// <returns><c>true</c> if <paramref name="list"/> ends with <paramref name="sequence"/>; <c>false</c> otherwise</returns>
	public static bool EndsWith<T>(this IList<T> list, IList<T> sequence)
		=> list.TakeLast(sequence.Count).SequenceEqual(sequence);

	/// <summary>
	/// Determines whether <paramref name="list"/> starts with <paramref name="sequence"/>.
	/// </summary>
	/// <typeparam name="T"><see cref="List{T}"/> type</typeparam>
	/// <param name="list">Larger <see cref="List{T}"/></param>
	/// <param name="sequence">Smaller <see cref="List{T}"/></param>
	/// <returns><c>true</c> if <paramref name="list"/> starts with <paramref name="sequence"/>; <c>false</c> otherwise</returns>
	public static bool StartsWith<T>(this IList<T> list, IList<T> sequence)
		=> list.Take(sequence.Count).SequenceEqual(sequence);

	public static T[] Generate<T>(Func<T, T> f, int n)
	{
		var rg = new T[n];

		for (int i = 0; i < n; i++) {
			T t = i == 0 ? default : rg[i - 1];
			rg[i] = f(t);
		}

		return rg;
	}

	public static T[] Generate<T>(Func<T> f, int n) => Generate<T>(x => f(), n);

	/// <summary>
	/// Retrieves a random element from <paramref name="list"/>.
	/// </summary>
	/// <typeparam name="T"><see cref="List{T}"/> type</typeparam>
	/// <param name="list"><see cref="List{T}"/> from which to retrieve a random element</param>
	/// <returns>A random element</returns>
	public static T GetRandomElement<T>(this IList<T> list)
	{
		int i = RandomInstance.Next(list.Count);

		return list[i];
	}

	public static object[] CastObjectArray(this Array r)
	{
		var rg = new object[r.Length];
		r.CopyTo(rg, 0);

		return rg;
	}

	public delegate int IndexOfCallback<in T>(T search, int start);

	public static IEnumerable<int> AllIndexesOf<T>(IndexOfCallback<T> callback, int inc, T search)
	{
		int minIndex = callback(search, 0);

		while (minIndex != -1) {
			yield return minIndex;
			minIndex = callback(search, minIndex + inc);
		}
	}

	public static IEnumerable<int> AllIndexesOf<T>(this List<T> list, T search)
		=> AllIndexesOf(list.IndexOf, 1, search);

	/// <summary>
	/// Replaces all occurrences of sequence <paramref name="sequence"/> within <paramref name="rg"/> with <paramref name="replace"/>.
	/// </summary>
	/// <param name="rg">Original <see cref="List{T}"/></param>
	/// <param name="sequence">Sequence to search for</param>
	/// <param name="replace">Replacement sequence</param>
	public static IList<T> ReplaceAllSequences<T>(this List<T> rg, IList<T> sequence, IList<T> replace)
		where T : IEquatable<T>
	{

		/*
			 | Method |     Mean |   Error |   StdDev |
			|------- |---------:|--------:|---------:|
			|   Test | 454.5 ns | 8.87 ns | 10.56 ns |
		 */

		/*
			 | Method |     Mean |   Error |  StdDev |
			|------- |---------:|--------:|--------:|
			|   Test | 236.0 ns | 1.93 ns | 1.81 ns |
		 */

		/*
			 | Method |     Mean |   Error |  StdDev |
			|------- |---------:|--------:|--------:|
			|   Test | 219.9 ns | 1.06 ns | 0.94 ns |
		 */

		int i = 0;

		var seqSpan = CollectionsMarshal.AsSpan(sequence as List<T>);
		var rgSpan  = CollectionsMarshal.AsSpan(rg);

		do {
			//i = rg.IndexOf(sequence[0], i);

			// var sp = new Span<T>(rg, i, sequence.Count);
			// var b  = rg.GetRange(i, sequence.Count).SequenceEqual(sequence);

			var b = rgSpan.Slice(i, sequence.Count).SequenceEqual(seqSpan);

			if (b) {
				rg.RemoveRange(i, sequence.Count);
				rg.InsertRange(i, replace);
				i += sequence.Count;
			}
		} while (!(++i >= rg.Count));

		return rg;
	}

	/*public static bool IndexOutOfBounds<T>(this IList<T> rg, int idx)
	{
		//idx < io.Length && idx >= 0
		//(idx < rg.Count && idx >= 0)
		//!(idx > rg.Count || idx < 0)

		return idx < rg.Count && idx >= 0;
	}*/

	public static IEnumerator<T> Cast<T>(this IEnumerator iterator)
	{
		while (iterator.MoveNext()) {
			yield return (T) iterator.Current;
		}
	}

	/*[CBN]
	public static T TryIndex<T>(this Span<T> s, Index i)
	{
		if (i.Value >= s.Length) {
			return default;
		}

		return s[i];
	}*/

	private static bool TryIndex__<T>(Index i, int max, Func<T> fx2, out T value)
	{
		if (i.Value >= max) {
			value = default;

			return false;
		}

		value = fx2();

		return true;
	}

	public static bool TryIndex<T>(this IList<T> span, Index i, [CBN] out T value)
	{
		/*if (i.Value >= span.Count) {
			value = default;
			return false;
		}

		value = span[i];
		return true;*/

		return TryIndex__(i, span.Count, () => span[i], out value);
	}

	public static bool TryIndex<T>(this Span<T> span, Index i, [CBN] out T value)
	{
		/*if (i.Value >= span.Length) {
			value = default;
			return false;
		}

		value = span[i];
		return true;*/

		return TryIndex__(i, span.Length, () => span[i], out value);

	}

	public static IEnumerable<T> Difference<T>(this IEnumerable<T> a, IEnumerable<T> b) => b.Where(c => !a.Contains(c));

	public static void Replace<T>(this List<T> list, Predicate<T> oldItemSelector, T newItem)
	{
		//check for different situations here and throw exception
		//if list contains multiple items that match the predicate
		//or check for nullability of list and etc ...
		int oldItemIndex = list.FindIndex(oldItemSelector);
		list[oldItemIndex] = newItem;
	}

	#region Dictionary

	#region Serialize

	/// <summary>
	/// Writes a <see cref="Dictionary{TKey,TValue}"/> to file <paramref name="filename"/>.
	/// </summary>
	public static void WriteDictionary(IDictionary<string, string> dictionary, string filename)
	{
		string[] lines = dictionary.Select(kvp => kvp.Key + DICT_DELIM + kvp.Value).ToArray();
		File.WriteAllLines(filename, lines);
	}

	/// <summary>
	/// Reads a <see cref="Dictionary{TKey,TValue}"/> written by <see cref="WriteDictionary"/> to <paramref name="filename"/>.
	/// </summary>
	public static IDictionary<string, string> ReadDictionary(string filename)
	{
		string[] lines = File.ReadAllLines(filename);

		var dict = lines.Select(l => l.Split(DICT_DELIM))
		                .ToDictionary(a => a[0], a => a[1]);

		return dict;
	}

	private const string DICT_DELIM = "=";

	#endregion


	public static bool TryCastDictionary<T>(T obj, out Map buf) where T : IDictionary
	{
		bool condition = obj.GetType().GetInterface(nameof(IDictionary)) != null;

		if (!condition) {
			buf = null;
			return false;
		}

		var enumerator = obj.GetEnumerator();

		buf = new Map();

		while (enumerator.MoveNext()) {
			buf.Add(enumerator.Key, enumerator.Value);

		}

		return true;
	}

	#endregion

	public static List<object> CopyToList(this IEnumerable value)
	{
		var e  = value.GetEnumerator();
		var rg = new List<object>();

		while (e.MoveNext()) {
			var item = e.Current;
			rg.Add(item);
		}

		return rg;
	}
}