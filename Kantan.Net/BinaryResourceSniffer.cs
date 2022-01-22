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
/// Binary data and Mime type sniffing
/// </summary>
public static class BinaryResourceSniffer
{
	/*
	 * type/subtype
	 * type/subtype;parameter=value
	 */

	/*
	 * https://github.com/khellang/MimeTypes/blob/master/src/MimeTypes/MimeTypeFunctions.ttinclude
	 */

	static BinaryResourceSniffer() { }

	/// <summary>
	///     Scans for binary resources within a webpage.
	/// </summary>
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

			const int timeout = -1;

			if (BinaryResource.FromUrl(s, b, out var bi, timeout: timeout, c)) {
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

	/// <see cref="BinaryResourceSniffer.FindMimeFromData"/>
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
	private static extern int FindMimeFromData(IntPtr pBC, [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
	                                           [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1,
	                                                      SizeParamIndex = 3)]
	                                           byte[] pBuffer, int cbSize,
	                                           [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
	                                           MimeFromDataFlags dwMimeFlags, out IntPtr ppwzMimeOut, int dwReserved);


	public static string ResolveMediaType(byte[] dataBytes, string mimeProposed = null)
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

	public static string GetMediaTypeFromData(this HttpContent message)
	{
		var stream = message.ReadAsStream();
		var ms     = (MemoryStream) stream;

		ms.Position = 0;

		const int i = 256;

		var buffer = new byte[i];
		int read   = ms.Read(buffer);

		string mediaType = ResolveMediaType(buffer);

		ms.Position = 0;

		return mediaType;
	}

	public static (string Type, string Subtype) ParseMediaType(string s)
	{
		var split = s.Split('/');

		var s1 = split[0];
		var s2 = split[^1];

		if (s2.Contains(';') /*||h.Parameters.Any()*/) {
			s2 = s2.Split(';')[0];
		}

		return (s1, s2);
	}

	public static (string Type, string Subtype) ParseMediaType(this MediaTypeHeaderValue header)
	{
		var s = header.MediaType;

		if (s == null) {
			return (null, null);
		}


		return ParseMediaType(s);

	}
}

public class BinaryResource : IDisposable
{
	public Uri Url { get; set; }

	public HttpResponseMessage Response { get; set; }

	public IBinaryResourceFilter Filter { get; set; }

	public BinaryResource() { }

	public bool Equals(BinaryResource other)
	{
		return Url == other?.Url && Equals(Response, other?.Response);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Url, Response, Filter);
	}

	public void Dispose()
	{
		Response?.Dispose();
		GC.SuppressFinalize(this);

	}

	public static bool FromUrl(string url, IBinaryResourceFilter filter, out BinaryResource br,
	                           int timeout = -1, CancellationToken? token = null)
	{
		br = new();

		if (!UriUtilities.IsUri(url, out Uri u)) {
			return false;
		}

		using var client = new HttpClient();

		var message = HttpUtilities.GetHttpResponse(url, timeout, token: token,
		                                            method: HttpMethod.Get);

		/*if (message is not { }) {
			return false;
		}*/
		
		if ( message is not { IsSuccessStatusCode: true }) {
			message?.Dispose();
			return false;
		}

		br.Url      = new Uri(url);
		br.Response = message;

		/* Check content-type */

		// The content-type returned from the response may not be the actual content-type, so
		// we'll resolve it using binary data instead to be sure

		var length = message.Content.Headers.ContentLength;
		br.Response = message;
		
		string mediaType;

		try {
			/*
			 * #1
			 */
			mediaType = message.Content.GetMediaTypeFromData();

		}
		catch (Exception) {
			/*
			 * #2
			 */
			mediaType = message.Content.Headers is { ContentType.MediaType: { } }
				            ? message.Content.Headers.ContentType.MediaType
				            : null;
		}

		const int UNLIMITED_SIZE = -1;

		bool type, size = length is UNLIMITED_SIZE;

		if (filter.MinimumSize.HasValue) {
			size = size || length >= filter.MinimumSize.Value;
		}


		if (filter.DiscreteType != default /*|| filter.DiscreteType!= DiscreteMimeType.Other*/) {
			if (mediaType != null) {
				type = mediaType.StartsWith(filter.DiscreteType, StringComparison.InvariantCultureIgnoreCase)
				       && !filter.TypeBlacklist.Contains(mediaType);
			}
			else {
				type = false; //?
			}
		}

		else {
			type = true;
		}


		return type && size;
	}

	
}

/// <see cref="IBinaryResourceFilter.DiscreteType"/>
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

public sealed class BinaryImageFilter : IBinaryResourceFilter
{
	public static readonly BinaryImageFilter Default = new();

	public int? MinimumSize => 50_000;

	public string DiscreteType => DiscreteMediaTypes.Image;

	public List<string> GetUrls(IHtmlDocument document)
	{
		var rg = document.QuerySelectorAttributes("img", "src")
		                 .Union(document.QuerySelectorAttributes("a", "href"))
		                 .ToList();

		return rg;
	}

	public Dictionary<string, Func<string, string>> HostUrlFilter
		=> new()
		{
			/*string hostComponent = UriUtilities.GetHostComponent(new Uri(url));
	
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
			}*/
		};

	public List<string> TypeBlacklist => new() { "image/svg+xml" };
}

public interface IBinaryResourceFilter
{
	public int? MinimumSize { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public string DiscreteType { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public List<string> TypeBlacklist { get; }

	public Dictionary<string, Func<string, string>> HostUrlFilter { get; }

	public List<string> GetUrls(IHtmlDocument document);
}