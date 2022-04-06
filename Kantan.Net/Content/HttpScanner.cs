using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kantan.Diagnostics;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Local

namespace Kantan.Net.Content;

/// <summary>
/// <a href="https://mimesniff.spec.whatwg.org/">See</a>
/// </summary>
public static class HttpScanner
{
	private const int RSRC_HEADER_LEN = 1445;

	/// <remarks><a href="https://mimesniff.spec.whatwg.org/#sniffing-a-mislabeled-binary-resource">7.2</a></remarks>
	public static string IsBinaryResource(byte[] input)
	{
		var l = input.Length;

		var seq1a = new byte[] { 0xFE, 0xFF };
		var seq1b = new byte[] { 0xFF, 0xFE };
		var seq2  = new byte[] { 0xEF, 0xBB, 0xBF };

		switch (l) {
			case >= 2 when (input.SequenceEqual(seq1a) || input.SequenceEqual(seq1b)):
				return HttpTypes.MT_TEXT_PLAIN;
			case >= 3 when (input.SequenceEqual(seq2)):
				return HttpTypes.MT_TEXT_PLAIN;

		}

		if (!input.Any(IsBinaryDataByte)) {
			return HttpTypes.MT_TEXT_PLAIN;
		}

		return HttpTypes.MT_APPLICATION_OCTET_STREAM;
	}

	public static bool CheckPattern(byte[] input, HttpTypes s, ISet<byte> ignored = null)
		=> CheckPattern(input, s.Pattern, s.Mask, ignored);

	/// <remarks><a href="https://mimesniff.spec.whatwg.org/#matching-a-mime-type-pattern">6</a></remarks>
	public static bool CheckPattern(byte[] input, byte[] pattern, byte[] mask, ISet<byte> ignored = null)
	{
		Require.Assert(pattern.Length == mask.Length);
		ignored ??= Enumerable.Empty<byte>().ToHashSet();

		if (input.Length < pattern.Length) {
			return false;
		}

		int s = 0;

		while (s < input.Length) {
			if (!ignored.Contains(input[s])) {
				break;
			}

			s++;
		}

		int p = 0;

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

	/// <remarks><a href="https://mimesniff.spec.whatwg.org/#terminology">3</a></remarks>
	public static bool IsBinaryDataByte(byte b)
	{
		return b is >= 0x00 and <= 0x08 or 0x0B or >= 0x0E and <= 0x1A or >= 0x1C and <= 0x1F;
	}

	/// <remarks><a href="https://mimesniff.spec.whatwg.org/#reading-the-resource-header">5.2</a></remarks>
	public static async Task<byte[]> ReadResourceHeader(Stream m)
	{
		int d = checked((int) m.Length);
		int l = d >= RSRC_HEADER_LEN ? RSRC_HEADER_LEN : d;

		byte[] data = new byte[l];

		var l2 = await m.ReadAsync(data, 0, l);

		return data;
	}
}