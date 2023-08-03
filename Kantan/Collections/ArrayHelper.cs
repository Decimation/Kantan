using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kantan.Collections;

public static class ArrayHelper
{
	public static T[] AddRange<T>(T[] rg, T[] val)
	{
		Array.Resize(ref rg, rg.Length + val.Length);
		val.CopyTo(rg, rg.Length - 1);
		return rg;
	}

	/*public static T[] Add<T>(T[] rg, T val)
	{
		/*var m = new List<T>(args);
		m.Add(o);
		return m.ToArray();#1#
		/*Array.Resize(ref args, args.Length + 1);
		args[^1] = o;
		return args;#1#
		return AddRange(rg, new[] { val });
	}

	public static T[] Insert<T>(T[] rg, int i, T val)
	{
		var m = new List<T>(rg);
		m.Insert(i, val);
		return m.ToArray();
	}*/

	public static T[][] TransposeRowsAndColumns<T>(this T[][] arr)
	{
		int   rowCount    = arr.Length;
		int   columnCount = arr[0].Length;
		T[][] transposed  = new T[columnCount][];

		if (rowCount == columnCount) {
			transposed = (T[][]) arr.Clone();

			for (int i = 1; i < rowCount; i++) {
				for (int j = 0; j < i; j++) {
					(transposed[i][j], transposed[j][i]) = (transposed[j][i], transposed[i][j]);
				}
			}
		}
		else {
			for (int column = 0; column < columnCount; column++) {
				transposed[column] = new T[rowCount];

				for (int row = 0; row < rowCount; row++) {
					transposed[column][row] = arr[row][column];
				}
			}
		}

		return transposed;
	}

	public static T[][] MatrixCopy<T>(T[] rg, int dim)
	{
		int rdim = rg.Length / dim;
		var cpy  = new T[rdim][];

		for (int i = 0; i < rdim; i++) {
			cpy[i] = rg.Skip(i * dim).Take(dim).ToArray();
		}

		return cpy;
	}
}