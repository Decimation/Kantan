using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local

namespace Kantan.Net;

public static class QScanner
{
	private const int RSRC_HEADER_LEN = 1445;

	private const string MIME_TYPE_TEXT_PLAIN = "text/plain";

	public static string Resolve(byte[] r)
	{
		var l = r.Length;

		switch (l) {
			case >= 2 when (r.SequenceEqual(new byte[] { 0xFE, 0xFF }) || r.SequenceEqual(new byte[] { 0xFF, 0xFE })):
				return MIME_TYPE_TEXT_PLAIN;
			case >= 3 when (r.SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF })):
				return MIME_TYPE_TEXT_PLAIN;

		}

		if (!r.Any(IsBinary)) {
			return MIME_TYPE_TEXT_PLAIN;
		}

		return "application/octet-stream";
	}

	public static bool Pattern(byte[] input, byte[] pattern, byte[] mask, ISet<byte> ignored)
	{
		Trace.Assert(pattern.Length == mask.Length);

		if (input.Length < pattern.Length) {
			return false;
		}

		var s = 0;

		while (s < input.Length) {
			if (!ignored.Contains(input[s])) {
				break;
			}

			s++;
		}

		var p = 0;

		while (p < pattern.Length) {
			var md = input[s] & mask[p];

			if (md != pattern[p]) {
				return false;
			}

			s++;
			p++;
		}

		return true;
	}

	public static bool IsBinary(byte b)
	{
		return b is >= 0x00 and <= 0x08 or 0x0B or >= 0x0E and <= 0x1A or >= 0x1C and <= 0x1F;
	}

	public static async Task<byte[]> ReadResourceHeader(Stream m)
	{

		// int l =m.Read(data, 0, data.Length);

		int j = 0;

		int    d    = checked((int) m.Length);
		int    l    = d >= RSRC_HEADER_LEN ? RSRC_HEADER_LEN : d;
		byte[] data = new byte[l];
		var    l2   = await m.ReadAsync(data, 0, l);

		/*do {
			int b = m.ReadByte();
			j++;
			if (j >= i) {
				break;
			}

			if (b == -1) {
				break;
			}

			data[j] = (byte) b;
		} while (m.CanRead);*/

		return data;
	}

	private static readonly byte[] PNG_P = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, };
	private static readonly byte[] PNG_M = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, };
	private static readonly byte[] JPG_M = { 0xFF, 0xFF, 0xFF };
	private static readonly byte[] JPG_P = { 0xFF, 0xD8, 0xFF };

	public static readonly HashSet<byte> EmptyIgnored = Enumerable.Empty<byte>().ToHashSet();
}