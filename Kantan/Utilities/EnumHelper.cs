using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Reflection;
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
	public static List<TEnum> GetSetFlags<TEnum>(TEnum value, bool excludeZero = true) where TEnum : Enum
	{
		var flags = Enum.GetValues(typeof(TEnum))
		                .Cast<TEnum>()
		                .Where(f => value.HasFlag(f))
		                .ToList();

		if (excludeZero) {
			flags.RemoveAll(e => Convert.ToInt32(e) == 0);

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
	public static TEnum Or<TEnum>(this TEnum t, TEnum t2) where TEnum : struct, Enum
	{
		unsafe {
			Trace.Assert(Unsafe.SizeOf<TEnum>() == sizeof(int));
			var ptr  = (int*) Unsafe.AsPointer(ref t);
			var ptr2 = (int*) Unsafe.AsPointer(ref t2);

			*ptr |= *ptr2;
			return Unsafe.As<int, TEnum>(ref *ptr);
		}
	}

	public static TEnum And<TEnum>(this TEnum t, TEnum t2) where TEnum : struct, Enum
	{
		unsafe {
			Trace.Assert(Unsafe.SizeOf<TEnum>() == sizeof(int));
			var ptr  = (int*) Unsafe.AsPointer(ref t);
			var ptr2 = (int*) Unsafe.AsPointer(ref t2);

			*ptr &= *ptr2;
			return Unsafe.As<int, TEnum>(ref *ptr);
		}
	}

	public static TEnum Xor<TEnum>(this TEnum t, TEnum t2) where TEnum : struct, Enum
	{
		unsafe {
			Trace.Assert(Unsafe.SizeOf<TEnum>() == sizeof(int));
			var ptr  = (int*) Unsafe.AsPointer(ref t);
			var ptr2 = (int*) Unsafe.AsPointer(ref t2);

			*ptr ^= *ptr2;
			return Unsafe.As<int, TEnum>(ref *ptr);
		}
	}

	public static TEnum Not<TEnum>(this TEnum t) where TEnum : struct, Enum
	{
		unsafe {
			Trace.Assert(Unsafe.SizeOf<TEnum>() == sizeof(int));
			var ptr = (int*) Unsafe.AsPointer(ref t);
			*ptr = ~*ptr;
			return Unsafe.As<int, TEnum>(ref *ptr);
		}
	}

	#endregion
}