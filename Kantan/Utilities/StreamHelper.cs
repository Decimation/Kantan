using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
		// NOTE: throws when stream is not seekable
		stream.Position = 0;
		// using var ms = new MemoryStream();
		// stream.CopyTo(ms);
		// var rg = ms.ToArray();

		int length = checked((int) stream.Length);

		return stream.ReadBlock(length);
	}

	public static byte[] ReadBlock(this Stream stream, int i = 256)
	{
		if (stream.CanSeek) {
			stream.Position = 0;
		}

		var buffer = new byte[i];

		int read = stream.Read(buffer);

		return buffer;
	}

	public static async Task<byte[]> ReadBlockAsync(this Stream m, int l1 = 256)
	{
		int l = l1;

		if (m.CanSeek) {
			m.Position = 0;
			int d = checked((int) m.Length);
			l = d >= l1 ? l1 : d;
		}

		// int l=Math.Clamp(d, d, HttpType.RSRC_HEADER_LEN);
		var data = new byte[l];

		int l2 = await m.ReadAsync(data, 0, l);

		return data;
	}
}