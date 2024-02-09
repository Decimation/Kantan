using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Kantan.Text;

// ReSharper disable UnusedMember.Global
#pragma warning disable CA2248
namespace Kantan.Utilities;

/// <summary>
/// Utilities for enums (<see cref="Enum"/>).
/// </summary>
public static class EnumHelper
{

	public static List<TEnum> GetSetFlags<TEnum>(this TEnum value, bool excludeZero = true, bool excludeValue = false)
		where TEnum : Enum
	{
		var flags = Enum.GetValues(typeof(TEnum))
			.Cast<TEnum>()
			.Where(f =>
			{
				var flag = value.HasFlag(f);
				return flag;
			})
			.ToList();

		if (excludeZero) {
			flags.RemoveAll(e => Convert.ToInt32(e) == 0);

		}

		if (excludeValue) {
			flags.RemoveAll(e => value.Equals(e));
		}

		//flags.RemoveAll(e => e.Equals(value));

		return flags;
	}

	public static TEnum SafeParse<TEnum>(string s) where TEnum : Enum
	{
		if (String.IsNullOrWhiteSpace(s)) {
			return default;
		}

		Enum.TryParse(typeof(TEnum), s, out object e);
		return (TEnum) e;
	}

	public static TEnum ReadFromSet<TEnum>(ISet<object> set) where TEnum : Enum
	{
		var t = typeof(TEnum);

		if (t.GetCustomAttribute<FlagsAttribute>() != null) {
			string sz = set.QuickJoin();
			Enum.TryParse(typeof(TEnum), sz, out object e);

			if (e == null) {
				return default;
			}

			return (TEnum) e;
		}

		return default;
	}

	#region Generic bitwise operations

	// TODO: .NET 7

	public static TEnum Or<TEnum>(TEnum t, TEnum t2) where TEnum : struct, Enum
	{
		if (Enum.GetUnderlyingType(typeof(TEnum)) != typeof(int)) {
			throw new NotImplementedException();
		}

		var v = Unsafe.As<TEnum, int>(ref t) | Unsafe.As<TEnum, int>(ref t2);
		return Unsafe.As<int, TEnum>(ref v);
	}

	public static TEnum And<TEnum>(TEnum t, TEnum t2) where TEnum : struct, Enum
	{
		if (Enum.GetUnderlyingType(typeof(TEnum)) != typeof(int)) {
			throw new NotImplementedException();
		}

		var v = Unsafe.As<TEnum, int>(ref t) & Unsafe.As<TEnum, int>(ref t2);
		return Unsafe.As<int, TEnum>(ref v);
	}

	public static TEnum Xor<TEnum>(TEnum t, TEnum t2) where TEnum : struct, Enum
	{
		if (Enum.GetUnderlyingType(typeof(TEnum)) != typeof(int)) {
			throw new NotImplementedException();
		}

		var v = Unsafe.As<TEnum, int>(ref t) ^ Unsafe.As<TEnum, int>(ref t2);
		return Unsafe.As<int, TEnum>(ref v);

	}

	public static TEnum Not<TEnum>(TEnum t) where TEnum : struct, Enum
	{
		if (Enum.GetUnderlyingType(typeof(TEnum)) != typeof(int)) {
			throw new NotImplementedException();
		}

		var v = ~Unsafe.As<TEnum, int>(ref t);
		return Unsafe.As<int, TEnum>(ref v);
	}

	#endregion

}