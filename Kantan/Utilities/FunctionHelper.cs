using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Utilities
{
	internal class FunctionHelper
	{
		public static List<T> ReadUntil<T>(Func<T> f, Predicate<T> a, Predicate<T> b)
		{
			var rg = new List<T>();

			T t;

			do {
				t = f();

				if (b(t)) {
					return null;
				}

				rg.Add(t);

				/*if (a(t)) {
				break;
			}*/

			} while (!a(t));

			return rg;
		}
	}
}
