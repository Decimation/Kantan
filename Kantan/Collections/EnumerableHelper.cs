using System;
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Monad;
using Kantan.Numeric;
using Kantan.Text;
using Kantan.Threading;
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
	/// Invokes <see cref="IEnumerator.MoveNext"/> then returns <see cref="IEnumerator.Current"/> if <c>non-null</c>; otherwise <c>default</c>
	/// </summary>
	public static T MoveAndGet<T>(this IEnumerator<T> t) => t.MoveNext() ? t.Current : default;

	/// <summary>
	/// Invokes <see cref="IEnumerator.MoveNext"/> then returns <see cref="IEnumerator.Current"/> if <c>non-null</c>; otherwise <c>default</c>
	/// </summary>
	public static object MoveAndGet(this IEnumerator t) => t.MoveNext() ? t.Current : default;

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

	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
	{
		// return source.OrderBy(x => Guid.NewGuid());
		return source.OrderBy(x => Random.Shared.Next());
	}

	public static IList<T> Shuffle<T>(this IList<T> list)
	{
		var cpy = new List<T>(list);
		int n   = cpy.Count;

		while (n > 1) {
			n--;
			int k = Random.Shared.Next(n + 1);

			(cpy[k], cpy[n]) = (cpy[n], cpy[k]);
		}

		return cpy;
	}

	/// <summary>
	/// Retrieves a random element from <paramref name="list"/>.
	/// </summary>
	/// <typeparam name="T"><see cref="List{T}"/> type</typeparam>
	/// <param name="list"><see cref="List{T}"/> from which to retrieve a random element</param>
	/// <returns>A random element</returns>
	public static T TakeRandom<T>(this IList<T> list)
	{
		int i = Random.Shared.Next(list.Count);

		return list[i];
	}

	public static bool IsIndexWithinBounds<T>(this IList<T> r, int index)
		=> MathHelper.IsInRange(index, r.Count);

	public static IEnumerable<T> TakeRandom<T>(this IList<T> list, int cnt) => list.Shuffle().Take(cnt);

	/*public static object[] CastObjectArray(this Array r)
	{
		/*var rg = new object[r.Length];
		r.CopyTo(rg, 0);
		return rg;#1#
		return r.Cast<object>().ToArray();
	}*/

	public delegate int IndexOfCallback<in T>(T search, int start);

	public static IEnumerable<int> IndexOfAll<T>(IndexOfCallback<T> callback, int inc, T search)
	{
		int minIndex = callback(search, 0);

		while (minIndex != -1) {
			yield return minIndex;

			minIndex = callback(search, minIndex + inc);
		}
	}

	public static IEnumerable<int> IndexOfAll<T>(this List<T> list, T search)
		=> IndexOfAll(list.IndexOf, 1, search);

	/// <summary>
	/// Replaces all occurrences of sequence <paramref name="sequence"/> within <paramref name="rg"/> with <paramref name="replace"/>.
	/// </summary>
	/// <param name="rg">Original <see cref="List{T}"/></param>
	/// <param name="sequence">Sequence to search for</param>
	/// <param name="replace">Replacement sequence</param>
	public static IList<T> ReplaceAllSequences<T>(this List<T> rg, IList<T> sequence, IList<T> replace)
		where T : IEquatable<T>
	{
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

	/*public static IEnumerator<T> Cast<T>(this IEnumerator iterator)
	{
		while (iterator.MoveNext()) {
			yield return (T) iterator.Current;
		}
	}*/

	public static IEnumerable<T> Difference<T>(this IEnumerable<T> a, IEnumerable<T> b) => b.Where(c => !a.Contains(c));

	public static void Replace<T>(this List<T> list, Predicate<T> oldItemSelector, T newItem)
	{
		//check for different situations here and throw exception
		//if list contains multiple items that match the predicate
		//or check for nullability of list and etc ...
		int oldItemIndex = list.FindIndex(oldItemSelector);
		list[oldItemIndex] = newItem;
	}

	// public static List<object> CastToList(this IEnumerable value) => CastToList<object>(value);

	// public static List<T> CastToList<T>(this IEnumerable value) => value.Cast<T>().ToList();

	/*private static bool TryIndex<T>(Index i, int len, out T value)
	{
		value = default;

		if (i.Value >= len) {
			return false;
		}

		return true;
	}

	public static bool TryIndex<T>(this IList<T> list, Index i, [CBN] out T value)
	{
		var b = TryIndex(i, list.Count, out value);

		if (b) {
			value = list[i];
		}

		return b;
	}

	public static bool TryIndex<T>(this Span<T> span, Index i, [CBN] out T value)
	{
		var b = TryIndex(i, span.Length, out value);

		if (b) {
			value = span[i];
		}

		return b;

	}*/

	#region Dictionary

	public static Dictionary<string, string> ReadDelim(string s, char delim = ',')
	{
		var dic = new Dictionary<string, string>();

		foreach (string s1 in s.Split(Environment.NewLine)) {
			string[] split = s1.Split(delim);

			dic.Add(split[0].RemoveNewLines(), split[1].RemoveNewLines());
		}

		return dic;
	}

	public static Dictionary<TKey, TValue> CastDictionary<TKey, TValue>(this IDictionary dic)
		=> dic.CastDictionary(k => (TKey) k, v => (TValue) v);

	public static Dictionary<TKey, TValue> CastDictionary<TKey, TValue>(
		this IDictionary dic, Func<object, TKey> keySelector, Func<object, TValue> valueSelector)
	{
		// return dic.Keys.Cast<TKey>().ToDictionary(k => keySelector(k), v => valueSelector(dic[v]));
		// var keys = dic.Keys.Cast<object>().Select(keySelector);
		// var keys = dic.Keys.Cast<TKey>();
		var keys = dic.Keys.Cast<object>();

		return keys.ToDictionary(keySelector, v => valueSelector(dic[v]));

	}

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

	public static IEnumerable<T> Cons<T>(this IEnumerable<T> source, T item) => new[] { item }.Concat(source);

	public static IResult<IEnumerable<T>> AllOrNothing<T>
		(this IEnumerable<IResult<T>> source)
		=> source.Aggregate
		(
			Enumerable.Empty<T>()
				.ToResult(),
			(left, right) =>
				left is IFailureResult<IEnumerable<T>> failure
					? $"{failure.Error}\n{right.Match(_ => string.Empty, error => error)}"
						.ToFailureResult<IEnumerable<T>>()
					: left is ISuccessResult<IEnumerable<T>> success
						? right is ISuccessResult<T> s ? success.Result
							  .Cons(s.Result)
							  .ToResult() :
						  right is IFailureResult<T> f ? f.Error
							                                 .ToFailureResult<IEnumerable<T>>() :
						                                 throw new ArgumentNullException(nameof(right))
						: throw new ArgumentNullException(nameof(left))
		);

	static IResult<IEnumerable<T>> AnyOrNothing<T>
		(this IEnumerable<IResult<T>> source)
		=> source.Aggregate
		(
			string.Empty
				.ToFailureResult<IEnumerable<T>>(),
			(left, right) =>
				right is ISuccessResult<T> success ? left is ISuccessResult<IEnumerable<T>> s
					                                     ? s.Result.Cons(success.Result).ToResult()
					                                     : left is IFailureResult<IEnumerable<T>> _
						                                     ? new[] { success.Result }.ToResult()
						                                     : throw new ArgumentNullException(nameof(left)) :
				right is IFailureResult<T> failure ? left is ISuccessResult<IEnumerable<T>>
					                                     ? left
					                                     : left is IFailureResult<IEnumerable<T>> f
						                                     ? $"{failure.Error}\n{f.Error}"
							                                     .ToFailureResult<IEnumerable<T>>()
						                                     : throw new ArgumentNullException(nameof(left)) :
				                                     throw new ArgumentNullException(nameof(right))
		);

	// This call is ambiguous with SelectMany<IEnumerable<T>, T>
	// the query syntax will prefer silently the other. This
	// needs to be into another namespace.
	public static IResult<IEnumerable<TResult>> SelectMany<T, TResult>
	(
		this IResult<IEnumerable<T>> source,
		Func<T, IResult<TResult>> func
	)
		=> source.BindMany(func);

	// We enumerate the enumerable two times, one for the BindMany
	// and one for the Zip, should really be an IReadOnlyCollection<T>
	// or we should ToList it.
	public static IResult<IEnumerable<TOutput>> SelectMany<T, TResult, TOutput>
	(
		this IResult<IEnumerable<T>> source,
		Func<T, IResult<TResult>> func,
		Func<T, TResult, TOutput> projection
	)
		=> source.BindMany(func)
			.Bind(result =>
				      ((ISuccessResult<IEnumerable<T>>) source)
				      .Result
				      .Zip(result, projection)
				      .ToResult());

	public static IResult<IEnumerable<TResult>> BindMany<T, TResult>
	(
		this IResult<IEnumerable<T>> source,
		Func<T, IResult<TResult>> func
	)
		=> source.Match(success => success.Select(func).AllOrNothing(),
		                failure => failure.ToFailureResult<TResult[]>());

	public static Task<IResult<IEnumerable<TResult>>> SelectMany<T, TResult>
	(
		this IResult<IEnumerable<T>> source,
		Func<T, Task<IResult<TResult>>> func
	)
		=> source.BindManyAsync(func);

	public static Task<IResult<IEnumerable<TOutput>>> SelectMany<T, TResult, TOutput>
	(
		this IResult<IEnumerable<T>> source,
		Func<T, Task<IResult<TResult>>> func,
		Func<T, TResult, TOutput> projection
	)
		=> source.BindManyAsync(func)
			.BindAsync(result =>
				           result.Zip(((ISuccessResult<IEnumerable<T>>) source).Result,
				                      (l, r) => projection(r, l))
					           .ToResult());

	public static Task<IResult<IEnumerable<TResult>>> BindManyAsync<T, TResult>
	(
		this IResult<IEnumerable<T>> source,
		Func<T, Task<IResult<TResult>>> func
	)
		=> source.Match(result => result.Select(func)
			                .WhenAll()
			                .Map(t => t.AllOrNothing()),
		                error => error.ToFailureResult<IEnumerable<TResult>>().ToTask());

	// First error message is lost when second fails
	public static Task<IResult<IEnumerable<TResult>>> BindManyAsync<T, TResult>
	(
		this IResult<IEnumerable<T>> source,
		Func<T, Task<IResult<TResult>>> func,
		Func<T, Task<IResult<TResult>>> fallbackFunc
	)
		=> source.BindManyAsync(func)
			.MatchAsync(result => result.ToResult().ToTask(),
			            _ => source.BindManyAsync(fallbackFunc)).Unwrap();

	public static IResult<IEnumerable<TResult>> BindMultipleAll<T, TResult>
	(
		this IResult<T> source,
		params Func<T, IResult<TResult>>[] func
	)
		=> source.Match(result => func.Select(fun => fun(result)).AllOrNothing(),
		                error => error.ToFailureResult<IEnumerable<TResult>>());

	public static IResult<IEnumerable<TResult>> BindMultipleAny<T, TResult>
	(
		this IResult<T> source,
		params Func<T, IResult<TResult>>[] func
	)
		=> source.Match(result => func.Select(fun => fun(result)).AnyOrNothing(),
		                error => error.ToFailureResult<IEnumerable<TResult>>());

}