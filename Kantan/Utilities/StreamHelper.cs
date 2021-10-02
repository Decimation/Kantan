using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kantan.Cli;
using Kantan.Cli.Controls;

// ReSharper disable UnusedMember.Global
using System.Linq;

namespace Kantan.Utilities
{
	public static class StreamHelper
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

		public static string[] ReadAllLines(this StreamReader stream)
		{
			var list = new List<string>();

			while (!stream.EndOfStream) {
				string line = stream.ReadLine();

				if (line != null) {
					list.Add(line);
				}
			}

			return list.ToArray();
		}


		public static byte[] ToByteArray(this Stream stream)
		{
			stream.Position = 0;
			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			var rg = ms.ToArray();

			return rg;
		}
	}
}