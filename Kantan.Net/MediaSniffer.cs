global using MA = System.Runtime.InteropServices.MarshalAsAttribute;
global using UT = System.Runtime.InteropServices.UnmanagedType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Flurl.Http;
using JetBrains.Annotations;
using Kantan.Diagnostics;

// ReSharper disable UnusedVariable

// ReSharper disable NonReadonlyMemberInGetHashCode

// ReSharper disable InconsistentNaming
#pragma warning disable IDE0059,IDE0060
namespace Kantan.Net;

/// <summary>
/// Binary data and Mime (Media) type sniffing
/// </summary>
public static class MediaSniffer
{
	/*
	 * type/subtype
	 * type/subtype;parameter=value
	 */

	/*
	 * https://github.com/khellang/MimeTypes/blob/master/src/MimeTypes/MimeTypeFunctions.ttinclude
	 */

	static MediaSniffer() { }

	/// <summary>
	///     Scans for binary resources within a webpage.
	/// </summary>
	public static List<MediaResource> Scan(string url, IMediaResourceFilter b, int count = 10,
	                                       long? timeoutMS = null,
	                                       CancellationToken? token = null)
	{
		var images = new List<MediaResource>();
		timeoutMS ??= HttpUtilities.Timeout;

		IHtmlDocument document = null;

		token ??= CancellationToken.None;

		try {
			var client = new HttpClient();
			var task   = client.GetStringAsync(url);
			task.Wait();
			string result = task.Result;

			var parser = new HtmlParser();
			document = parser.ParseDocument(result);
			client.Dispose();
		}
		catch (Exception) {
			goto _Return;
		}


		// urls.AddRange(document.QuerySelectorAttributes("a", "href"));
		// urls.AddRange(document.QuerySelectorAttributes("img", "src"));

		List<string> urls = b.GetUrls(document);

		/*
		* Normalize urls
		*/

		urls = urls.Where(url => url != null).Select(u =>
		{
			if (UriUtilities.IsUri(u, out Uri u2)) {
				return UriUtilities.NormalizeUrl(u2);
			}

			return u;
		}).Distinct().ToList();

		/*
		* Filter urls if the host is known
		*/


		var plr = Parallel.For(0, urls.Count, (i, pls) =>
		{
			string s = urls[i];


			if (MediaResource.FromUrl(s, b, out var bi, timeout: (int) timeoutMS, token)) {
				if (bi is { } && count > 0) {
					bi.Url = new Uri(bi.Url.ToString().Split(' ')[0].Trim());
					images.Add(bi);
					count--;
					pls.Break();

				}
				else {
					bi?.Dispose();
				}
			}
			else {
				bi?.Dispose();
			}
		});


		_Return:
		document?.Dispose();
		return images;
	}


	public static IMediaTypeResolver DefaultResolver { get; set; } = new UrlmonResolver();

	public static string Resolve(string u) => Resolve(u, DefaultResolver);
	
	public static string Resolve(this HttpContent u) => Resolve(u, DefaultResolver);


	public static string Resolve(string u, IMediaTypeResolver z)
	{
		var task = u.GetStreamAsync();
		task.Wait();
		return z.Resolve(task.Result);
	}


	public static string Resolve(this HttpContent message, IMediaTypeResolver z)
	{
		var stream = message.ReadAsStream();
		return z.Resolve(stream);
	}

	internal static byte[] GetHeaderBlock(this Stream stream)
	{
		var ms = (MemoryStream) stream;

		ms.Position = 0;

		const int i = 256;

		var buffer = new byte[i];
		int read   = ms.Read(buffer);
		return buffer;
	}

	public static (string Type, string Subtype) GetMediaTypeTuple(string s)
	{
		string[] split = s.Split('/');

		string s1 = split[0];
		string s2 = split[^1];

		if (s2.Contains(';') /*||h.Parameters.Any()*/) {
			s2 = s2.Split(';')[0];
		}

		return (s1, s2);
	}
}

public sealed class UrlmonResolver : IMediaTypeResolver
{
	public string Resolve(Stream stream)
	{
		var buf = (stream).GetHeaderBlock();
		return ResolveFromData(buf);
	}

	public static UrlmonResolver Instance { get; } = new UrlmonResolver();

	private static string ResolveFromData(byte[] dataBytes, string mimeProposed = null)
	{
		//https://stackoverflow.com/questions/2826808/how-to-identify-the-extension-type-of-the-file-using-c/2826884#2826884
		//https://stackoverflow.com/questions/18358548/urlmon-dll-findmimefromdata-works-perfectly-on-64bit-desktop-console-but-gener
		//https://stackoverflow.com/questions/11547654/determine-the-file-type-using-c-sharp
		//https://github.com/GetoXs/MimeDetect/blob/master/src/Winista.MimeDetect/URLMONMimeDetect/urlmonMimeDetect.cs

		Require.ArgumentNotNull(dataBytes, nameof(dataBytes));

		string mimeRet = String.Empty;

		if (!String.IsNullOrEmpty(mimeProposed)) {
			//suggestPtr = Marshal.StringToCoTaskMemUni(mimeProposed); // for your experiments ;-)
			mimeRet = mimeProposed;
		}


		const MimeFromDataFlags flags = MimeFromDataFlags.ENABLE_MIME_SNIFFING |
		                                MimeFromDataFlags.RETURN_UPDATED_IMG_MIMES |
		                                MimeFromDataFlags.IGNORE_MIME_TEXT_PLAIN;

		int ret = FindMimeFromData(IntPtr.Zero, null, dataBytes, dataBytes.Length,
		                           mimeProposed, flags, out IntPtr outPtr, 0);

		if (ret == 0 && outPtr != IntPtr.Zero) {
			string str = Marshal.PtrToStringUni(outPtr);

			Marshal.FreeCoTaskMem(outPtr);

			return str;
		}

		return mimeRet;
	}

	/// <see cref="FindMimeFromData"/>
	[Flags]
	private enum MimeFromDataFlags
	{
		DEFAULT                  = 0x00000000,
		URL_AS_FILENAME          = 0x00000001,
		ENABLE_MIME_SNIFFING     = 0x00000002,
		IGNORE_MIME_TEXT_PLAIN   = 0x00000004,
		SERVER_MIME              = 0x00000008,
		RESPECT_TEXT_PLAIN       = 0x00000010,
		RETURN_UPDATED_IMG_MIMES = 0x00000020,
	}

	[DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
	private static extern int FindMimeFromData(IntPtr pBC, [MA(UT.LPWStr)] string pwzUrl,
	                                           [MA(UT.LPArray, ArraySubType = UT.I1, SizeParamIndex = 3)]
	                                           byte[] pBuffer, int cbSize,
	                                           [MA(UT.LPWStr)] string pwzMimeProposed,
	                                           MimeFromDataFlags dwMimeFlags, out IntPtr ppwzMimeOut,
	                                           int dwReserved);
}

public sealed class MagicResolver : IMediaTypeResolver
{
	public MagicResolver()
	{
		Magic = MagicNative.magic_open(MagicResolver.MagicMimeFlags);
		var rd = MagicNative.magic_load(Magic, @"C:\Library\magic.mgc");
	}

	public static MagicResolver Instance { get; } = new MagicResolver();

	public string Resolve(Stream stream)
	{
		var buf = (stream).GetHeaderBlock();
		var sz  = MagicNative.magic_buffer(Magic, buf, buf.Length);
		return Marshal.PtrToStringAnsi(sz);
	}

	public const MagicOpenFlags MagicMimeFlags =
		MagicOpenFlags.MAGIC_ERROR |
		MagicOpenFlags.MAGIC_MIME_TYPE |
		MagicOpenFlags.MAGIC_NO_CHECK_COMPRESS |
		MagicOpenFlags.MAGIC_NO_CHECK_ELF |
		MagicOpenFlags.MAGIC_NO_CHECK_APPTYPE;

	public IntPtr Magic { get; }
}

public interface IMediaTypeResolver
{
	string Resolve(Stream m);
}

/// <see cref="IMediaResourceFilter.DiscreteType"/>
public static class DiscreteMediaTypes
{
	public const string Image       = "image";
	public const string Text        = "text";
	public const string Application = "application";
	public const string Video       = "video";
	public const string Audio       = "audio";
	public const string Model       = "model";

	// todo
	// ...
}

/*
 * Adapted from https://github.com/hey-red/Mime
 */
public static class MagicNative
{
	private const string MAGIC_LIB_PATH = "libmagic-1";

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr magic_open(MagicOpenFlags flags);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern int magic_load(IntPtr magic_cookie, string filename);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern void magic_close(IntPtr magic_cookie);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr magic_file(IntPtr magic_cookie, string filename);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr magic_buffer(IntPtr magic_cookie, byte[] buffer, int length);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr magic_buffer(IntPtr magic_cookie, IntPtr buffer, int length);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern IntPtr magic_error(IntPtr magic_cookie);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern MagicOpenFlags magic_getflags(IntPtr magic_cookie);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern int magic_setflags(IntPtr magic_cookie, MagicOpenFlags flags);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern int magic_check(IntPtr magic_cookie, string dbPath);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern int magic_compile(IntPtr magic_cookie, string dbPath);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern int magic_getparam(IntPtr magic_cookie, MagicParams param, out int value);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern int magic_setparam(IntPtr magic_cookie, MagicParams param, ref int value);

	[DllImport(MAGIC_LIB_PATH, CallingConvention = CallingConvention.Cdecl)]
	public static extern int magic_version();
}

public enum MagicParams
{
	/// <summary>
	/// The parameter controls how many levels of recursion will be followed for indirect magic entries.
	/// </summary>
	MAGIC_PARAM_INDIR_MAX = 0,

	/// <summary>
	/// The parameter controls the maximum number of calls for name/use.
	/// </summary>
	MAGIC_PARAM_NAME_MAX,

	/// <summary>
	/// The parameter controls how many ELF program sections will be processed.
	/// </summary>
	MAGIC_PARAM_ELF_PHNUM_MAX,

	/// <summary>
	/// The parameter controls how many ELF sections will be processed.
	/// </summary>
	MAGIC_PARAM_ELF_SHNUM_MAX,

	/// <summary>
	/// The parameter controls how many ELF notes will be processed.
	/// </summary>
	MAGIC_PARAM_ELF_NOTES_MAX,

	/// <summary>
	/// Regex limit
	/// </summary>
	MAGIC_PARAM_REGEX_MAX,

	/// <summary>
	/// The parameter controls how many bytes read from file
	/// </summary>
	MAGIC_PARAM_BYTES_MAX
}

[Flags]
public enum MagicOpenFlags
{
	// From LIBMAGIC(3) man.

	/// <summary>
	/// No special handling.
	/// </summary>
	MAGIC_NONE = 0x000000,

	/// <summary>
	/// Print debugging messages to stderr.
	/// </summary>
	MAGIC_DEBUG = 0x000001,

	/// <summary>
	/// If the file queried is a symlink, follow it.
	/// </summary>
	MAGIC_SYMLINK = 0x000002,

	/// <summary>
	/// If the file is compressed, unpack it and look at the contents.
	/// </summary>
	MAGIC_COMPRESS = 0x000004,

	/// <summary>
	/// If the file is a block or character special device, then
	/// open the device and try to look in its contents.
	/// </summary>
	MAGIC_DEVICES = 0x000008,

	/// <summary>
	/// Return a MIME type string, instead of a textual description.
	/// </summary>
	MAGIC_MIME_TYPE = 0x000010,

	/// <summary>
	/// Return all matches, not just the first.
	/// </summary>
	MAGIC_CONTINUE = 0x000020,

	/// <summary>
	/// Check the magic database for consistency and print warnings to stderr.
	/// </summary>
	MAGIC_CHECK = 0x000040,

	/// <summary>
	/// On systems that support utime(3) or utimes(2), attempt to
	/// preserve the access time of files analysed.
	/// </summary>
	MAGIC_PRESERVE_ATIME = 0x000080,

	/// <summary>
	/// Don't translate unprintable characters to a \ooo octal representation.
	/// </summary>
	MAGIC_RAW = 0x000100,

	/// <summary>
	/// Treat operating system errors while trying to open files
	/// and follow symlinks as real errors, instead of printing
	/// them in the magic buffer.
	/// </summary>
	MAGIC_ERROR = 0x000200,

	/// <summary>
	/// Return a MIME encoding, instead of a textual description.
	/// </summary>
	MAGIC_MIME_ENCODING = 0x000400,

	/// <summary>
	/// A shorthand for MAGIC_MIME_TYPE | MAGIC_MIME_ENCODING.
	/// </summary>
	MAGIC_MIME = (MAGIC_MIME_TYPE | MAGIC_MIME_ENCODING),

	/// <summary>
	/// Return the Apple creator and type.
	/// </summary>
	MAGIC_APPLE = 0x000800,

	/// <summary>
	/// Return a slash-separated list of extensions for this file type.
	/// </summary>
	MAGIC_EXTENSION = 0x1000000,

	/// <summary>
	/// Don't report on compression, only report about the uncompressed data.
	/// </summary>
	MAGIC_COMPRESS_TRANSP = 0x200000,

	/// <summary>
	/// A shorthand for (MAGIC_EXTENSION|MAGIC_MIME|MAGIC_APPLE)
	/// </summary>
	MAGIC_NODESC = (MAGIC_EXTENSION | MAGIC_MIME | MAGIC_APPLE),

	/// <summary>
	/// Don't look inside compressed files.
	/// </summary>
	MAGIC_NO_CHECK_COMPRESS = 0x001000,

	/// <summary>
	/// Don't examine tar files.
	/// </summary>
	MAGIC_NO_CHECK_TAR = 0x002000,

	/// <summary>
	/// Don't consult magic files.
	/// </summary>
	MAGIC_NO_CHECK_SOFT = 0x004000,

	/// <summary>
	/// Don't check for EMX application type (only on EMX).
	/// </summary>
	MAGIC_NO_CHECK_APPTYPE = 0x008000,

	/// <summary>
	/// Don't print ELF details.
	/// </summary>
	MAGIC_NO_CHECK_ELF = 0x010000,

	/// <summary>
	/// Don't check for various types of text files.
	/// </summary>
	MAGIC_NO_CHECK_TEXT = 0x020000,

	/// <summary>
	/// Don't get extra information on MS Composite Document Files.
	/// </summary>
	MAGIC_NO_CHECK_CDF = 0x040000,

	/// <summary>
	/// Don't check for CSV files
	/// </summary>
	MAGIC_NO_CHECK_CSV = 0x080000,

	/// <summary>
	/// Don't look for known tokens inside ascii files.
	/// </summary>
	MAGIC_NO_CHECK_TOKENS = 0x100000,

	/// <summary>
	/// Don't check text encodings.
	/// </summary>
	MAGIC_NO_CHECK_ENCODING = 0x200000,

	/// <summary>
	///  Don't check for JSON files
	/// </summary>
	MAGIC_NO_CHECK_JSON = 0x400000,

	/// <summary>
	/// No built-in tests; only consult the magic file
	/// </summary>
	MAGIC_NO_CHECK_BUILTIN =
		MAGIC_NO_CHECK_COMPRESS |
		MAGIC_NO_CHECK_TAR |
		MAGIC_NO_CHECK_SOFT |
		MAGIC_NO_CHECK_APPTYPE |
		MAGIC_NO_CHECK_ELF |
		MAGIC_NO_CHECK_TEXT |
		MAGIC_NO_CHECK_CSV |
		MAGIC_NO_CHECK_CDF |
		MAGIC_NO_CHECK_TOKENS |
		MAGIC_NO_CHECK_ENCODING |
		MAGIC_NO_CHECK_JSON,
}