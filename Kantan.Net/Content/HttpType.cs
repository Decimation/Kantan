using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kantan.Diagnostics;

// ReSharper disable InconsistentNaming

namespace Kantan.Net.Content;

/// <remarks><a href="https://mimesniff.spec.whatwg.org/#matching-an-image-type-pattern">6.1</a></remarks>
public readonly struct HttpType
{
	public byte[] Mask { get; init; }

	public byte[] Pattern { get; init; }

	public string Type { get; init; }

	public bool IsPartial => Mask is null && Pattern is null && Type is not null;

	/*public HttpTypeSignature()
	{
		Mask    = null;
		Pattern = null;
		Type    = null;
	}*/

	// todo: move to Embedded Resources

	#region

	public static readonly HttpType gif = new()
	{
		Pattern = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61, },
		Mask    = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, },
		Type    = "image/gif"
	};

	public static readonly HttpType gif2 = new()
	{
		Pattern = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, },
		Mask    = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, },
		Type    = "image/gif"
	};

	public static readonly HttpType webp = new()
	{
		Pattern = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, },
		Mask    = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, },
		Type    = "image/webp"
	};

	public static readonly HttpType png = new()
	{
		Pattern = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, },
		Mask    = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, },
		Type    = "image/png"
	};

	public static readonly HttpType jpg = new()
	{
		Pattern = new byte[] { 0xFF, 0xD8, 0xFF },
		Mask    = new byte[] { 0xFF, 0xFF, 0xFF },
		Type    = "image/png"
	};

	public static readonly HttpType bmp = new()
	{
		Pattern = new byte[] { 0x42, 0x4D },
		Mask    = new byte[] { 0xFF, 0xFF },
		Type    = "image/bmp"
	};

	#endregion

	static HttpType()
	{
		All = typeof(HttpType)
		      .GetFields(BindingFlags.Static | BindingFlags.Public)
		      .Where(f => f.FieldType == typeof(HttpType))
		      .Select(x => (HttpType) x.GetValue(null)).ToArray();
	}

	public static HttpType[] All { get; }

	public override string ToString()
	{
		/*return $"{nameof(Type)}: {Type} | " +
		       $"{nameof(IsPartial)}: {IsPartial}";*/
		return Type;
	}

	#region 

	public const string MT_TEXT_PLAIN               = $"{MT_TEXT}/plain";
	public const string MT_APPLICATION_OCTET_STREAM = $"{MT_APPLICATION}/octet-stream";

	public const string MT_IMAGE       = "image";
	public const string MT_TEXT        = "text";
	public const string MT_APPLICATION = "application";
	public const string MT_VIDEO       = "video";
	public const string MT_AUDIO       = "audio";
	public const string MT_MODEL       = "model";

	#endregion

	public const int RSRC_HEADER_LEN = 1445;

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#sniffing-a-mislabeled-binary-resource">7.2</a>
	/// </remarks>
	public static string IsBinaryResource(byte[] input)
	{
		int l = input.Length;

		byte[] seq1a = { 0xFE, 0xFF };
		byte[] seq1b = { 0xFF, 0xFE };
		byte[] seq2  = { 0xEF, 0xBB, 0xBF };

		switch (l) {
			case >= 2 when input.SequenceEqual(seq1a) || input.SequenceEqual(seq1b):
				return MT_TEXT_PLAIN;
			case >= 3 when input.SequenceEqual(seq2):
				return MT_TEXT_PLAIN;

		}

		if (!input.Any(IsBinaryDataByte)) {
			return MT_TEXT_PLAIN;
		}

		return MT_APPLICATION_OCTET_STREAM;
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#terminology">3</a>
	/// </remarks>
	public static bool IsBinaryDataByte(byte b)
	{
		return b is >= 0x00 and <= 0x08 or 0x0B or >= 0x0E and <= 0x1A or >= 0x1C and <= 0x1F;
	}

	public static bool CheckPattern(byte[] input, HttpType s, ISet<byte> ignored = null)
		=> CheckPattern(input, s.Pattern, s.Mask, ignored);

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#matching-a-mime-type-pattern">6</a>
	/// </remarks>
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
			int md = input[s] & mask[p];

			if (md != pattern[p]) {
				return false;
			}

			s++;
			p++;
		}

		return true;
	}
}