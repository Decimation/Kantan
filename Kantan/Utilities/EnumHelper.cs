using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
}