using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Utilities;

public static class ObjectExtensions
{
	private static readonly Dictionary<Type, MethodInfo> TypeCloneMap = new();

	public static T Clone<T>(this T t)
	{
		const string name = "MemberwiseClone";

		var type = t.GetType().GetTypeInfo();

		if (TypeCloneMap.TryGetValue(type, out var mi)) {
			mi = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public |
			                          BindingFlags.Instance);
		}
		else {
			TypeCloneMap[type] = mi;
		}

		Debug.Assert(mi != null);

		var value = mi.Invoke(t, null);
		return Unsafe.As<object, T>(ref value);
	}
}