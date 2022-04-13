using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kantan.Cli;
using Kantan.Cli.Controls;

// ReSharper disable UnusedMember.Global
using System.Linq;

namespace Kantan.Utilities;

public static class StreamHelper
{
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