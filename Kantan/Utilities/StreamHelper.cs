using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
		// using var ms = new MemoryStream();
		// stream.CopyTo(ms);
		// var rg = ms.ToArray();
		var rg = new byte[stream.Length];

		int i = stream.Read(rg, 0, checked((int) stream.Length));

		return rg;
	}

	public static byte[] ReadHeader(this Stream stream, int i = 256)
	{
		var ms = (MemoryStream) stream;
		ms.Position = 0;

		var buffer = new byte[i];

		int read   = ms.Read(buffer);

		return buffer;
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#reading-the-resource-header">5.2</a>
	/// </remarks>
	public static async Task<byte[]> ReadHeaderAsync(this Stream m, int l1 = 256)
	{
		int d = checked((int) m.Length);
		int l = d >= l1 ? l1 : d;

		// int l=Math.Clamp(d, d, HttpType.RSRC_HEADER_LEN);
		byte[] data = new byte[l];

		int l2 = await m.ReadAsync(data, 0, l);

		return data;
	}
}