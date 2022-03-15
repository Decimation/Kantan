using System;
using System.Collections.Generic;

namespace Kantan.Collections;

public static class ArrayHelper
{
	public static T[] Add<T>(this T[] rg, T[] val)
	{
		Array.Resize(ref rg, rg.Length + val.Length);
		val.CopyTo(rg, rg.Length - 1);
		return rg;
	}

	public static T[] Add<T>(this T[] rg, T val)
	{
		/*var m = new List<T>(args);
		m.Add(o);
		return m.ToArray();*/
		/*Array.Resize(ref args, args.Length + 1);
		args[^1] = o;
		return args;*/
		return rg.Add(new[] { val });
	}

	public static T[] Insert<T>(this T[] rg, int i, T val)
	{
		var m = new List<T>(rg);
		m.Insert(i, val);
		return m.ToArray();
	}
}