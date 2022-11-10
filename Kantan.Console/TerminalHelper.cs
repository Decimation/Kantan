using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Kantan.Utilities;
using Terminal.Gui;

namespace Kantan.Console;

public static class TerminalHelper
{
	/*public static IEnumerable GetItems(this ListView lv)
	{
		return lv.Source.ToList();
	}*/

	public static IEnumerable<T> ToList<T>(this IListDataSource lv)
	{
		var ls = lv.ToList().Cast<T>();
		return ls;
	}

	public static IEnumerable<T> GetMarkedItems<T>(this IListDataSource lv)
	{
		return lv.GetItems<T>()
		         .Where(kv => kv.IsMarked)
		         .Select(kv => kv.Value);
	}

	public static IEnumerable<(T Value, bool IsMarked)> GetItems<T>(this IListDataSource lv)
	{
		var ls = lv.ToList<T>().ToArray();

		for (int i = 0; i < lv.Count; i++) {
			yield return (ls[i], lv.IsMarked(i));
		}
	}

	/*public static void FromEnum<TEnum>(this ListView lv, TEnum t) where TEnum : struct, Enum
	{
		Debug.Assert(Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<int>());
		var e   = Enum.GetValues<TEnum>();
		var it  = Unsafe.As<TEnum, int>(ref t);
		
		var lvs = lv.Source.GetItems<TEnum>();
		foreach ((TEnum Value, bool IsMarked) valueTuple in lvs) {
			for (int i = 0; i < e.Length; i++) {
				unsafe {
					var val = e[i];
					int iv  = Unsafe.As<TEnum, int>(ref val);

					var b = (it & iv) != 0;

					if (valueTuple.Value.Equals(val)) {
						
					}
				}
			}

		}
	}*/

	public static void FromEnum<TEnum>(this ListView lv, TEnum e) where TEnum : struct, Enum
	{
		var list = lv.Source.ToList<TEnum>().ToArray();

		for (var i = 0; i < list.Length; i++) {
			// var flag = Enum.Parse<TEnum>(list[i].ToString());
			// var mark = e.HasFlag(flag);
			var mark = e.HasFlag(list[i]);
			lv.Source.SetMark(i, mark);
		}
	}
	
	public static TEnum GetEnum<TEnum>(this IListDataSource lv, TEnum t = default) where TEnum : struct, Enum
	{
		var m = lv.GetItems<TEnum>();

		TEnum t2 = t;

		Debug.Assert(Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<int>());

		unsafe {
			var ptr = (int*) Unsafe.AsPointer(ref t2);

			foreach (var t1 in m) {
				TEnum tv  = t1.Value;
				var   val = (int*) Unsafe.AsPointer(ref tv);

				if (t1.IsMarked) {
					*ptr |= *val;

				}
				else {
					*ptr &= ~*val;
				}
			}

		}

		return t2;
	}
}