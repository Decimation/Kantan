using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Utilities;

public static class FunctionHelper
{
	public static List<T> AddUntil<T>(this Func<T> f, Predicate<T> until, Predicate<T> stop)
	{
		var rg = new List<T>();

		T t;

		do {
			t = f();

			if (stop(t)) {
				return null;
			}

			rg.Add(t);

			/*if (until(t)) {
			break;
		}*/

		} while (!until(t));

		return rg;
	}
}