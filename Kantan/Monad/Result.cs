// Read Stanton Kantan Result.cs
// 2023-01-16 @ 5:42 PM

using System.Diagnostics;
using System.Threading.Tasks;
using System;
using Kantan.Threading;

namespace Kantan.Monad;

//https://github.com/UnoSD/ResultMonad/blob/master/Result/Result.cs

public interface IResult<out T> { }

public interface ISuccessResult<out T> : IResult<T>
{
	T Result { get; }
}

[DebuggerStepThrough]
[DebuggerDisplay("Success: {" + nameof(Result) + "}")]
public class SuccessResult<T> : ISuccessResult<T>
{
	public SuccessResult(T result) => Result = result;

	public T Result { get; }
}

public interface IFailureResult<out T> : IResult<T>
{
	string Error { get; }
}

[DebuggerStepThrough]
[DebuggerDisplay("Failure: {" + nameof(Error) + "}")]
public class FailureResult<T> : IFailureResult<T>
{
	public FailureResult(string error) => Error = error;

	public string Error { get; }
}

[DebuggerStepThrough]
public static class ResultExtensions
{
	public static IResult<T> ToResult<T>(this T result) => new SuccessResult<T>(result);

	public static IResult<T> ToFailureResult<T>(this string error) => new FailureResult<T>(error);
}

[DebuggerStepThrough]
public static class SingleResultExtensions
{
	public static IResult<TResult> SelectMany<T, TResult>
	(
		this IResult<T> source,
		Func<T, IResult<TResult>> func
	)
		=> source.Bind(func);

	public static TResult Match<T, TResult>
	(
		this IResult<T> source,
		Func<T, TResult> onSuccess,
		Func<string, TResult> onError
	)
		=> source is IFailureResult<T> failure ? onError(failure.Error) :
		   source is ISuccessResult<T> success ? onSuccess(success.Result) :
		                                         throw new ArgumentNullException(nameof(source));

	public static IResult<TOutput> SelectMany<T, TResult, TOutput>
	(
		this IResult<T> source,
		Func<T, IResult<TResult>> func,
		Func<T, TResult, TOutput> projection
	)
		=> source.Bind(func)
			.Bind(result => projection(((ISuccessResult<T>) source).Result, result)
				      .ToResult());

	public static IResult<TResult> Bind<T, TResult>
	(
		this IResult<T> source,
		Func<T, IResult<TResult>> func
	)
		=> source.Match(func,
		                error => error.ToFailureResult<TResult>());

	public static IResult<TResult> Select<T, TResult>
	(
		this IResult<T> source,
		Func<T, TResult> func
	)
		=> source.Map(func);

	public static IResult<TResult> Map<T, TResult>
	(
		this IResult<T> source,
		Func<T, TResult> func
	)
		=> source.Match(result => func(result).ToResult(),
		                error => error.ToFailureResult<TResult>());

	public static Task<IResult<TResult>> Select<T, TResult>
	(
		this IResult<T> source,
		Func<T, Task<TResult>> func
	)
		=> source.MapAsync(func);

	public static Task<IResult<TResult>> MapAsync<T, TResult>
	(
		this IResult<T> source,
		Func<T, Task<TResult>> func
	)
		=> source.Match(result => func(result).Map(o => o.ToResult()),
		                error => error.ToFailureResult<TResult>()
			                .ToTask());

	public static Task<IResult<TResult>> SelectMany<T, TResult>
	(
		this Task<IResult<T>> source,
		Func<T, IResult<TResult>> func
	)
		=> source.BindAsync(func);

	// The method with projection theoretically is not needed
	// when you don't need T and TResult to get to TOutput but
	// TResult IS TOutput like in the version without projection.
	// Unfortunately the compiler complains it can't use the
	// query syntax if you don't declare this as well.
	public static Task<IResult<TOutput>> SelectMany<T, TResult, TOutput>
	(
		this Task<IResult<T>> source,
		Func<T, IResult<TResult>> func,
		Func<T, TResult, TOutput> projection
	)
		=> source.BindAsync(func)
			.BindAsync(result => projection(((ISuccessResult<T>) source.Result).Result,
			                                result)
				           .ToResult());

	public static Task<IResult<TResult>> BindAsync<T, TResult>
	(
		this Task<IResult<T>> source,
		Func<T, IResult<TResult>> func
	)
		=> source.Map(result => result.Bind(func));

	public static Task<IResult<TResult>> SelectMany<T, TResult>
	(
		this IResult<T> source,
		Func<T, Task<IResult<TResult>>> func
	)
		=> source.BindAsync(func);

	public static Task<IResult<TOutput>> SelectMany<T, TResult, TOutput>
	(
		this IResult<T> source,
		Func<T, Task<IResult<TResult>>> func,
		Func<T, TResult, TOutput> projection
	)
		=> source.BindAsync(func)
			.BindAsync(result => projection(((ISuccessResult<T>) source).Result,
			                                result)
				           .ToResult());

	public static Task<IResult<TResult>> BindAsync<T, TResult>
	(
		this IResult<T> source,
		Func<T, Task<IResult<TResult>>> func
	)
		=> source.Match(func,
		                error => error.ToFailureResult<TResult>()
			                .ToTask());

	public static Task<TResult> MatchAsync<T, TResult>
	(
		this Task<IResult<T>> source,
		Func<T, Task<TResult>> onSuccess,
		Func<string, TResult> onError
	)
		=> source.Bind(result => result.Match(onSuccess,
		                                      error => onError(error).ToTask()));

	public static Task<TResult> MatchAsync<T, TResult>
	(
		this Task<IResult<T>> source,
		Func<T, TResult> onSuccess,
		Func<string, TResult> onError
	)
		=> source.Map(result => result.Match(onSuccess, onError));
}