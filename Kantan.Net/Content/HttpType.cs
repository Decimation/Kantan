using System.Linq;
using System.Reflection;

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
		return $"{nameof(Type)}: {Type} | " +
		       $"{nameof(IsPartial)}: {IsPartial}";
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
}