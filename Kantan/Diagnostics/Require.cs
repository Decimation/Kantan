// ReSharper disable RedundantUsingDirective.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UseNullableReferenceTypesAnnotationSyntax
// ReSharper disable IdentifierTypo

#pragma warning disable IDE0051, IDE0005

#region Aliases
global using VP = JetBrains.Annotations.ValueProviderAttribute;
global using CAE = System.Runtime.CompilerServices.CallerArgumentExpressionAttribute;
global using ACT = JetBrains.Annotations.AssertionConditionType;
global using AC = JetBrains.Annotations.AssertionConditionAttribute;
global using AM = JetBrains.Annotations.AssertionMethodAttribute;
global using CA = JetBrains.Annotations.ContractAnnotationAttribute;
global using SFM = JetBrains.Annotations.StringFormatMethodAttribute;
global using DH = System.Diagnostics.DebuggerHiddenAttribute;
global using DNR = System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute;
global using DNRI = System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute;
global using NNV = JetBrains.Annotations.NonNegativeValueAttribute;
global using NNINN = System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute;
global using NNW = System.Diagnostics.CodeAnalysis.NotNullWhenAttribute;
global using MNN = System.Diagnostics.CodeAnalysis.MemberNotNullAttribute;
global using MNNW = System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute;
global using MNW = System.Diagnostics.CodeAnalysis.MaybeNullWhenAttribute;
global using NN = JetBrains.Annotations.NotNullAttribute;
global using CBN = JetBrains.Annotations.CanBeNullAttribute;
global using ICBN = JetBrains.Annotations.ItemCanBeNullAttribute;
global using AN = System.Diagnostics.CodeAnalysis.AllowNullAttribute;
global using MN = System.Diagnostics.CodeAnalysis.MaybeNullAttribute;
global using MURV = JetBrains.Annotations.MustUseReturnValueAttribute;

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using Kantan.Collections;
using Kantan.Text;
using static Kantan.Internal.Common;

#nullable enable

namespace Kantan.Diagnostics;

/// <summary>
/// Diagnostic utilities, conditions, contracts
/// </summary>
/// <seealso cref="Debug"/>
/// <seealso cref="Trace"/>
/// <seealso cref="Debugger"/>
public static class Require
{
	/*
	 * https://www.jetbrains.com/help/resharper/Contract_Annotations.html
	 * https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis
	 */

	#region Contract annotations

	private const string VALUE_NULL_HALT    = "value:null => halt";
	private const string VALUE_NOTNULL_HALT = "value:notnull => halt";
	private const string COND_FALSE_HALT    = "condition:false => halt";
	private const string UNCONDITIONAL_HALT = "=> halt";

	private const ACT ACT_TRUE     = ACT.IS_TRUE;
	private const ACT ACT_NOT_NULL = ACT.IS_NOT_NULL;

	#endregion

	public static bool QWrite(object o, [CallerArgumentExpression("o")] string? call = null)
	{
		//todo
		string? s;

		if (o is IEnumerable xx) {
			List<object> rg = xx.CopyToList();

			s = rg.QuickJoin();
		}
		else {
			s = o switch
			{
				null => null,
				_    => o.ToString(),
			};
		}

		Console.WriteLine(s);

		return true;
	}


	[DNR, DH, AM]
	[CA(UNCONDITIONAL_HALT), SFM(STRING_FORMAT_ARG)]
	public static void Fail(string? msg = null, params object[] args) => Fail<Exception>(msg, args);

	/// <summary>
	/// Root fail function
	/// </summary>
	[DNR, DH, AM]
	[CA(UNCONDITIONAL_HALT), SFM(STRING_FORMAT_ARG)]
	public static void Fail<TException>(string? msg = null, params object[] args)
		where TException : Exception, new()
	{
		TException exception;

		if (msg != null) {
			string s = String.Format(msg, args);

			exception = (TException) Activator.CreateInstance(typeof(TException), s)!;
		}
		else {
			exception = new TException();
		}

		throw exception;
	}

	private static bool ActionThrows<TException>(Action f) where TException : Exception
	{
		bool throws = false;

		try {
			f();
		}
		catch (TException) {
			throws = true;
		}

		return throws;
	}

	[DH, AM]
	[CA(COND_FALSE_HALT)]
	public static void Assert([AC(ACT_TRUE), DNRI(false)] bool condition,
	                          string? msg = null, params object[] args)
		=> Assert<Exception>(condition, msg, args);

	/// <summary>
	/// Root assertion function
	/// </summary>
	[DH, AM]
	[CA(COND_FALSE_HALT), SFM(STRING_FORMAT_ARG)]
	public static void Assert<TException>([AC(ACT_TRUE), DNRI(false)] bool condition,
	                                      string? msg = null, params object[] args)
		where TException : Exception, new()
	{
		if (!condition) {
			Fail<TException>(msg, args);
		}
	}

	[DH, AM]
	[CA(COND_FALSE_HALT)]
	public static void Argument([AC(ACT_TRUE), DNRI(false)] bool condition,
	                                  string? name = null)
		=> Assert<ArgumentException>(condition, name);

	[DH, AM]
	[CA(VALUE_NULL_HALT)]
	public static void ArgumentNotNull([NN, AC(ACT_NOT_NULL)] object? value,
	                                         string? name = null)
		=> Assert<ArgumentNullException>(value != null, name);

	[DH, AM]
	[CA(VALUE_NULL_HALT)]
	public static void NotNull([NN, AC(ACT_NOT_NULL)] object? value, string? name = null)
		=> Assert<NullReferenceException>(value != null, name);

	[DH, AM]
	[CA(VALUE_NULL_HALT)]
	public static void NotNullOrWhiteSpace([NN, AC(ACT_NOT_NULL)] string? value, string? name = null)
		=> Assert<NullReferenceException>(!String.IsNullOrWhiteSpace(value), name);

	[DH, AM]
	public static void Equal(object a, object b) => Assert(a.Equals(b));

	[DH, AM]
	public static void Equal<T>(T a, T b) where T : IEquatable<T> => Assert(a.Equals(b));

	[DH, AM]
	public static void Contains<T>(IEnumerable<T> enumerable, T value) => Assert(enumerable.Contains(value));

	[DH, AM]
	public static void NonNegative([NNV] long value, string? name = null) => Assert(value is > 0 or 0, name);

	[DH, AM]
	public static void Positive([NNV] long value, string? name = null) => Assert(value > 0, name);

	[DH, AM]
	public static void FileExists(string value, string? name = null)
		=> Assert<FileNotFoundException>(File.Exists(value), name);

	[DH, AM]
	public static void Throws<TException>(Action f) where TException : Exception => Assert(ActionThrows<TException>(f));

	[DH, AM]
	public static void ForAll([AC(ACT_TRUE), DNRI(false)] params bool[] conditions)
	{
		foreach (bool condition in conditions) {
			Assert(condition);
		}
	}
}

public sealed class RequireException : Exception
{
	public RequireException() { }

	public RequireException(string? message) : base(message) { }
}