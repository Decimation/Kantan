global using MA = System.Runtime.InteropServices.MarshalAsAttribute;
global using UT = System.Runtime.InteropServices.UnmanagedType;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Flurl.Http;
using Kantan.Net.Content;
using Kantan.Net.Content.Resolvers;
using Kantan.Net.Media.Filters;

// ReSharper disable UnusedVariable

// ReSharper disable NonReadonlyMemberInGetHashCode

// ReSharper disable InconsistentNaming
#pragma warning disable IDE0059,IDE0060
namespace Kantan.Net.Media;

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

#if TST
	
	/// <summary>
	///     Scans for binary resources within a webpage.
	/// </summary>
	public static async Task<List<MediaResource>> ScanAsync(string url, ISet<HttpTypes> types, int count = 10,
	                                                        long? timeoutMS = null,
	                                                        CancellationToken? token = null,
	                                                        IMediaResourceFilter b = null)
	{
		var images = new List<HttpResource>();
		timeoutMS ??= HttpUtilities.Timeout;

		IHtmlDocument document = null;
		b     ??= new MediaImageFilter();
		token ??= CancellationToken.None;

		try {
			var result = await HttpResource.GetAsync(url);

			var s = await result.Response.GetStringAsync();

			var parser = new HtmlParser();

			document = parser.ParseDocument(s);
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

		/*urls = urls.Where(url => url != null).Select(u =>
		{
			if (UriUtilities.IsUri(u, out Uri u2)) {

				return UriUtilities.NormalizeUrl(u2);
			}

			return u;
		}).Distinct().Where(x => b.UrlFilter(x)).ToList();*/

		for (int i = urls.Count - 1; i >= 0; i--) {

			if (urls[i] is { }) {

				if (UriUtilities.IsUri(urls[i], out var u2)) {

					urls[i] = UriUtilities.NormalizeUrl(u2);
				}

				if (!b.UrlFilter( /*u2*/ urls[i])) {

					Debug.WriteLine($"removing {u2}");
					urls.RemoveAt(i);
				}
			}
		}

		/*
		* Filter urls if the host is known
		*/

		var imagesCopy = images;

		var plr = Parallel.For(0, urls.Count, async (i, pls) =>
		{
			string s = urls[i];

			var h = await HttpResource.GetAsync(s);

			// var mr1 = h != null ? h

			if (h is { } && count > 0) {
				mr1.Url = new Uri(mr1.Url.ToString().Split(' ')[0].Trim());
				imagesCopy.Add(mr1);
				count--;
				pls.Break();

			}
			else {
				mr1?.Dispose();
			}

			if (MediaResource.FromUrl(s, b, out var mr1, (int) timeoutMS, token)) { }
			else {
				mr1?.Dispose();
			}
		});

		_Return:
		document?.Dispose();
		images = images.DistinctBy(x => x.Url).ToList();
		return images;
	}
#endif

	/// <summary>
	///     Scans for binary resources within a webpage.
	/// </summary>
	public static List<MediaResource> Scan(string url, MediaResourceFilter b, int count = 10,
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

		/*urls = urls.Where(url => url != null).Select(u =>
		{
			if (UriUtilities.IsUri(u, out Uri u2)) {

				return UriUtilities.NormalizeUrl(u2);
			}

			return u;
		}).Distinct().Where(x => b.UrlFilter(x)).ToList();*/

		urls = b.Refine(urls);

		/*
		* Filter urls if the host is known
		*/

		List<MediaResource> imagesCopy = images;

		var plr = Parallel.For(0, urls.Count, (i, pls) =>
		{
			string s = urls[i];

			if (MediaResource.FromUrl(s, b, out var mr1, (int) timeoutMS, token)) {
				if (mr1 is { } && count > 0) {
					mr1.Url = new Uri(mr1.Url.ToString().Split(' ')[0].Trim());
					imagesCopy.Add(mr1);
					count--;
					pls.Break();

				}
				else {
					mr1?.Dispose();
				}
			}
			else {
				mr1?.Dispose();
			}
		});

		_Return:
		document?.Dispose();
		images = images.DistinctBy(x => x.Url).ToList();
		return images;
	}

	public static string Resolve(string url) => Resolve(url, IHttpTypeResolver.Default);

	public static string Resolve(this HttpContent content) => Resolve(content, IHttpTypeResolver.Default);

	public static string Resolve(string url, IHttpTypeResolver resolver)
	{
		var task = url.GetStreamAsync();
		task.Wait();
		return resolver.Resolve(task.Result);
	}

	public static string Resolve(this HttpContent content, IHttpTypeResolver resolver)
	{
		var stream = content.ReadAsStream();
		return resolver.Resolve(stream);
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

/// <see cref="MediaResourceFilter.DiscreteType"/>
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