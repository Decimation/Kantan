using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Flurl.Http;
using JetBrains.Annotations;
using Kantan.Model;
using Kantan.Net.Properties;
using Newtonsoft.Json;

#pragma warning disable CS0168

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local

#pragma warning disable 8600
#pragma warning disable 8604
#pragma warning disable IDE0055,IDE0059,CS0219


// ReSharper disable CognitiveComplexity

// ReSharper disable InconsistentNaming

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

// ReSharper disable UnusedMember.Global
#nullable disable

namespace Kantan.Net;

/// <summary>
/// Network, HTTP, web utilities
/// </summary>
/// <seealso cref="WebUtilities"/>
/// <seealso cref="Dns"/>
/// <seealso cref="IPAddress"/>
/// <seealso cref="HttpUtility"/>
public static class HttpUtilities
{
	internal const int TIMEOUT_MS = 2000;

	private const int MAX_AUTO_REDIRECTS = 50;

	public static string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
	                                               "(KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36";

#if DISABLED
	public static bool ResetStatus(this HttpRequestMessage request)
	{
		const int MessageNotYetSent = 0;
		const int MessageAlreadySent = 1;
		const int MessageIsRedirect = 2;

		const string sendStatusFieldName = "_sendStatus";

		// internal bool MarkAsSent() => Interlocked.CompareExchange(ref _sendStatus, MessageAlreadySent, MessageNotYetSent) == MessageNotYetSent;

		var type = request.GetType().GetTypeInfo();

		var field = type.GetField(sendStatusFieldName,
		                          BindingFlags.Instance | BindingFlags.NonPublic);

		if (field != null) {
			field.SetValue(request, MessageNotYetSent);
			return true;
		}

		return false;
	}


	public static bool ResetStatus(this HttpClient request)
	{
		const string sendStatusFieldName = "_operationStarted";

		var type = request.GetType().GetTypeInfo();

		var field = type.GetField(sendStatusFieldName,
		                          BindingFlags.Instance | BindingFlags.NonPublic);

		if (field != null) {
			field.SetValue(request, false);
			return true;
		}

		return false;
	}

#endif

	[CanBeNull]
	[MustUseReturnValue]
	public static HttpResponseMessage GetHttpResponse(string url, int ms = TIMEOUT_MS,
	                                                  [CanBeNull] HttpMethod method = null,
	                                                  bool allowAutoRedirect = true,
	                                                  int maxAutoRedirects = MAX_AUTO_REDIRECTS)
	{

		using var request = new HttpRequestMessage
		{
			RequestUri = new Uri(url),
			Method     = method ?? HttpMethod.Get
		};

		using var clientHandler = new HttpClientHandler
		{
			AllowAutoRedirect        = allowAutoRedirect,
			MaxAutomaticRedirections = maxAutoRedirects
		};

		using var client = new HttpClient(clientHandler);

		client.Timeout = TimeSpan.FromMilliseconds(ms);

		try {
			var response = client.Send(request);

			return response;
		}
		catch (Exception x) {
			return null;
		}

	}

	public static PingReply Ping(Uri u, int ms = TIMEOUT_MS) => Ping(IPUtilities.GetAddress(u.ToString()), ms);

	public static PingReply Ping(string hostOrIP, int ms = TIMEOUT_MS)
	{
		var ping = new Ping();

		var task = ping.SendPingAsync(hostOrIP, ms);
		task.Wait();

		PingReply r = task.Result;

		return r;
	}

	[CanBeNull]
	public static string GetFinalRedirect(string url)
	{
		// https://stackoverflow.com/questions/704956/getting-the-redirected-url-from-the-original-url

		if (String.IsNullOrWhiteSpace(url))
			return url;

		const int MAX_REDIR = 8;

		int maxRedirCount = MAX_REDIR; // prevent infinite loops

		string newUrl = url;

		do {

			try {

				var resp = GetHttpResponse(url, TIMEOUT_MS, HttpMethod.Head);

				switch (resp.StatusCode) {
					case HttpStatusCode.OK:
						return newUrl;

					case HttpStatusCode.Redirect:
					case HttpStatusCode.MovedPermanently:
					case HttpStatusCode.RedirectKeepVerb:
					case HttpStatusCode.RedirectMethod:
						newUrl = (string) resp.Content.Headers.ContentLocation?.ToString();

						if (newUrl == null)
							return url;

						if (newUrl.Contains("://", StringComparison.Ordinal)) {
							// Doesn't have a URL Schema, meaning it's a relative or absolute URL
							Uri u = new(new Uri(url), newUrl);
							newUrl = u.ToString();
						}

						break;

					default:
						return newUrl;
				}

				url = newUrl;
			}
			catch (WebException) {
				// Return the last known good URL
				return newUrl;
			}
			catch (Exception) {
				return null;
			}
		} while (maxRedirCount-- > 0);

		return newUrl;
	}

	public static string ToQueryString(this NameValueCollection nvc)
	{
		var array = (from key in nvc.AllKeys
		             from value in nvc.GetValues(key)
		             select $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(value)}"
		            ).ToArray();
		return "?" + String.Join("&", array);
	}

	public static Uri AddQuery(this Uri uri, string name, string value)
	{
		var collection = HttpUtility.ParseQueryString(uri.Query);

		collection.Remove(name);
		collection.Add(name, value);

		var ub = new UriBuilder(uri);

		// this code block is taken from httpValueCollection.ToString() method
		// and modified so it encodes strings with HttpUtility.UrlEncode
		if (collection.Count == 0)
		{
			ub.Query = String.Empty;
		}
		else {
			var sb = new StringBuilder();

			for (int i = 0; i < collection.Count; i++) {
				string text = collection.GetKey(i);

				{
					text = HttpUtility.UrlEncode(text);

					string   val    = (text != null) ? (text + "=") : string.Empty;
					string[] values = collection.GetValues(i);

					if (sb.Length > 0)
						sb.Append('&');

					if (values == null || values.Length == 0)
						sb.Append(val);
					else {
						if (values.Length == 1) {
							sb.Append(val);
							sb.Append(HttpUtility.UrlEncode(values[0]));
						}
						else {
							for (int j = 0; j < values.Length; j++) {
								if (j > 0)
									sb.Append('&');

								sb.Append(val);
								sb.Append(HttpUtility.UrlEncode(values[j]));
							}
						}
					}
				}
			}

			ub.Query = sb.ToString();
		}

		return ub.Uri;
	}

	public static IHtmlDocument GetHtmlDocument(string url)
	{
		string html   = GetString(url);
		var    parser = new HtmlParser();

		var document = parser.ParseDocument(html);
		return document;
	}

	public static string GetFile(string url, string folder)
	{
		string    fileName = Path.GetFileName(url);
		using var client   = new HttpClient();

		client.DefaultRequestHeaders.Add("User-Agent", "Other");
		string dir = Path.Combine(folder, fileName);
		client.DownloadFile(url, dir);

		return dir;

	}


	public static Stream GetStream(string url)
	{
		// using var wc = new WebClient();
		// return wc.OpenRead(url);

		var stream = url.GetStreamAsync();
		var r      = stream.GetAwaiter().GetResult();
		return r;
	}

	public static byte[] GetData(string url)
	{
		using var h = new HttpClient();
		return h.DownloadData(url);
	}

	public static string GetString(string url)
	{
		using var h = new HttpClient();
		return h.DownloadString(url);


	}

	public static string DownloadString(this HttpClient client, string url)
	{
		var task = client.GetStringAsync(url);
		task.Wait();
		return task.Result;
	}

	public static Stream GetStream(this HttpClient client, string url)
	{
		var task = client.GetStreamAsync(url);
		task.Wait();
		return task.Result;
	}

	public static byte[] DownloadData(this HttpClient client, string url)
	{
		var task = client.GetByteArrayAsync(url);
		task.Wait();
		return task.Result;
	}

	public static string DownloadFile(this HttpClient client, string url)
	{
		var fname = Path.GetFileName(url);
		var tmp   = Path.Combine(Path.GetTempPath(), fname);

		return client.DownloadFile(url, tmp);
	}

	public static string DownloadFile(this HttpClient client, string url, string output)
	{
		var bytes = client.DownloadData(url);

		File.WriteAllBytes(output, bytes);

		return output;
	}
}