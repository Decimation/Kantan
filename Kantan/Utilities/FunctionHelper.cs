using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Utilities;

public static class FunctionHelper
{
	public static T[] Generate<T>(Func<T, T> f, int n)
	{
		var rg = new T[n];

		for (int i = 0; i < n; i++)
		{
			T t = i == 0 ? default : rg[i - 1];
			rg[i] = f(t);
		}

		return rg;
	}

	public static T[] Generate<T>(Func<T> f, int n) => Generate<T>(x => f(), n);

	public static List<T> RunUntil<T>(this Func<T> f, Predicate<T> until, Predicate<T> stop)
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

	public static bool ActionThrows<TException>(Action f) where TException : Exception
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
}