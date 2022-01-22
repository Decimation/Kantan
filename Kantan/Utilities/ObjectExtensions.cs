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
	public static T Clone<T>(this T t)
	{
		const string name = "MemberwiseClone";

		var method = t.GetType().GetTypeInfo().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public |
		                                                       BindingFlags.Instance);
		Debug.Assert(method != null);

		var value = method.Invoke(t, null);
		return Unsafe.As<object, T>(ref value);
	}
}