using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Kantan.Diagnostics;

namespace Kantan.Net;

public static class BinarySniffer
{
	/*
	 * type/subtype
	 * type/subtype;parameter=value
	 */

	/*
	 * https://github.com/khellang/MimeTypes/blob/master/src/MimeTypes/MimeTypeFunctions.ttinclude
	 */


	/// <summary>
	///     Scans for direct images within a webpage.
	/// </summary>
	/// <param name="url">Url to search</param>
	/// <param name="count">Number of direct images to return</param>
	/// <param name="timeoutMS"></param>
	/// <param name="token"></param>
	public static List<BinaryResource> Scan(string url, IBinaryResourceFilter b, int count = 10,
	                                   long? timeoutMS = null,
	                                   CancellationToken? token = null)
	{
		var images = new List<BinaryResource>();
		timeoutMS ??= HttpUtilities.Timeout;

		IHtmlDocument document = null;

		var c = token ?? CancellationToken.None;

		try {
			var client = new HttpClient();
			var task   = client.GetStringAsync(url);
			task.Wait();
			string result = task.Result;

			var parser = new HtmlParser();
			document = parser.ParseDocument(result);
			client.Dispose();
		}
		catch (Exception e) {
			goto _Return;
		}


		var urls = new List<string>();

		urls.AddRange(document.QuerySelectorAttributes("a", "href"));
		urls.AddRange(document.QuerySelectorAttributes("img", "src"));

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

		string hostComponent = UriUtilities.GetHostComponent(new Uri(url));

		switch (hostComponent) {
			case "www.deviantart.com":
				//https://images-wixmp-
				urls = urls.Where(x => x.Contains("images-wixmp"))
				           .ToList();
				break;
			case "twitter.com":
				urls = urls.Where(x => !x.Contains("profile_banners"))
				           .ToList();
				break;
		}


		var pr = Parallel.For(0, urls.Count, (i, pls) =>
		{
			string s = urls[i];

			const int timeout = -1;

			if (IsBinaryResource(s, b, out var di, timeout: timeout, c)) {
				if (di is { } && count > 0) {
					images.Add(di);
					count--;
					pls.Break();

				}
				else {
					di?.Dispose();
				}
			}
			else {
				di?.Dispose();
			}
		});

		// Debug.WriteLine($"{nameof(ScanForImages)}: {pr}");

		_Return:
		document?.Dispose();
		return images;
	}

	public static bool IsBinaryResource(string url, IBinaryResourceFilter filter, out BinaryResource bu,
	                               int timeout = -1,
	                               CancellationToken? token = null)
	{
		/*const string svg_xml    = "image/svg+xml";
		const string image      = "image";
		const int    min_size_b = 50_000;*/

		bu = new();

		if (!UriUtilities.IsUri(url, out Uri u)) {
			return false;
		}

		using var client = new HttpClient() { };

		var t = HttpUtilities.GetHttpResponse(url, ms: timeout, token: token,
		                                      method: HttpMethod.Get);

		if (t is not { }) {
			return false;
		}

		if (!t.IsSuccessStatusCode) {
			t.Dispose();
			return false;
		}


		bu.Url      = new Uri(url);
		bu.Response = t;

		/* Check content-type */

		// The content-type returned from the response may not be the actual content-type, so
		// we'll resolve it using binary data instead to be sure

		var length = t.Content.Headers.ContentLength;
		bu.Response = t;

		string mediaType;

		try {
			// var task = t.Content.ReadAsByteArrayAsync();
			// task.Wait();
			// mediaType = ResolveMimeType(task.Result);
			mediaType = ResolveMimeType(t);

		}
		catch (Exception) {
			

			mediaType = t.Content.Headers is { ContentType.MediaType: { } }
				            ? t.Content.Headers.ContentType.MediaType
				            : null;
		}


		bool size, type;

		const int UNLIMITED_SIZE = -1;

		size = length is UNLIMITED_SIZE;

		if (filter.MinimumSize.HasValue) {
			size = size || length >= filter.MinimumSize.Value;
		}

		/*else {
		size = length is i or >= min_size_b;
		}*/


		if (filter.RootType != null) {
			if (mediaType != null) {
				type = mediaType.StartsWith(filter.RootType) && !filter.BlacklistedTypes.Contains(mediaType);

			}
			else {
				type = false; //?
			}
		}

		else {
			type = true;
		}

		Debug.WriteLine($">>>  {url}");

		return type && size;
	}

	[DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
	private static extern int FindMimeFromData(IntPtr pBC, [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
	                                           [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1,
	                                                      SizeParamIndex = 3)]
	                                           byte[] pBuffer, int cbSize,
	                                           [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
	                                           MimeFlags dwMimeFlags, out IntPtr ppwzMimeOut, int dwReserved);

	public static string ResolveMimeType(HttpResponseMessage message, string url = null, string mimeProposed = null)
	{
		var task = message.Content.ReadAsByteArrayAsync();
		task.Wait();
		var rg = task.Result;
		return ResolveMimeType(rg, url, mimeProposed);
	}

	public static string ResolveMimeType(byte[] dataBytes, string url = null, string mimeProposed = null)
	{
		//https://stackoverflow.com/questions/2826808/how-to-identify-the-extension-type-of-the-file-using-c/2826884#2826884
		//https://stackoverflow.com/questions/18358548/urlmon-dll-findmimefromdata-works-perfectly-on-64bit-desktop-console-but-gener
		//https://stackoverflow.com/questions/11547654/determine-the-file-type-using-c-sharp
		//https://github.com/GetoXs/MimeDetect/blob/master/src/Winista.MimeDetect/URLMONMimeDetect/urlmonMimeDetect.cs

		Guard.AssertArgumentNotNull(dataBytes, nameof(dataBytes));

		string mimeRet = String.Empty;

		if (!String.IsNullOrEmpty(mimeProposed)) {
			//suggestPtr = Marshal.StringToCoTaskMemUni(mimeProposed); // for your experiments ;-)
			mimeRet = mimeProposed;
		}


		const MimeFlags flags = MimeFlags.ENABLE_MIME_SNIFFING | MimeFlags.RETURN_UPDATED_IMG_MIMES |
		                        MimeFlags.IGNORE_MIME_TEXT_PLAIN;

		int ret = FindMimeFromData(IntPtr.Zero, url, dataBytes, dataBytes.Length,
		                           mimeProposed, flags, out IntPtr outPtr, 0);

		if (ret == 0 && outPtr != IntPtr.Zero) {
			string str = Marshal.PtrToStringUni(outPtr);

			Marshal.FreeCoTaskMem(outPtr);

			return str;
		}

		return mimeRet;
	}
}

public sealed class BinaryImageFilter : IBinaryResourceFilter
{
	public int? MinimumSize => 50_000;

	public string RootType => "image";

	public List<string> BlacklistedTypes => new() { "image/svg+xml" };
}

public interface IBinaryResourceFilter
{
	public int? MinimumSize { get; }

	public string RootType { get; }

	public List<string> BlacklistedTypes { get; }
}

public class BinaryResource : IDisposable
{
	public Uri Url { get; internal set; }

	public HttpResponseMessage Response { get; internal set; }

	public bool Equals(BinaryResource other)
	{
		return Url == other?.Url && Equals(Response, other?.Response);
	}

	public override bool Equals(object obj)
	{
		return obj is BinaryResource other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Response, Url);
	}

	public static bool operator ==(BinaryResource left, BinaryResource right)
	{
		return left is not null && left.Equals(right);
	}

	public static bool operator !=(BinaryResource left, BinaryResource right)
	{
		return left is not null && !left.Equals(right);
	}

	public void Dispose()
	{
		Response?.Dispose();
		GC.SuppressFinalize(this);

	}

	public override string ToString()
	{
		return $"{Url}";
	}
}

[Flags]
public enum MimeFlags : int
{
	DEFAULT = 0x00000000,

	URL_AS_FILENAME = 0x00000001,

	ENABLE_MIME_SNIFFING = 0x00000002,

	IGNORE_MIME_TEXT_PLAIN = 0x00000004,

	SERVER_MIME = 0x00000008,

	RESPECT_TEXT_PLAIN = 0x00000010,

	RETURN_UPDATED_IMG_MIMES = 0x00000020,
}