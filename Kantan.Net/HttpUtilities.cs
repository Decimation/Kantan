global using CBN = JetBrains.Annotations.CanBeNullAttribute;
global using MURV = JetBrains.Annotations.MustUseReturnValueAttribute;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Flurl.Http;
using Flurl.Http.Configuration;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Kantan.Model;
using Kantan.Net.Properties;
using Newtonsoft.Json;
#pragma warning disable CS0168, IDE0051
#pragma warning disable IDE0060

// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable CognitiveComplexity
// ReSharper disable InconsistentNaming
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming

// ReSharper disable IdentifierTypo

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
	public static int Timeout { get; set; } = 2000;

	public static int MaxAutoRedirects { get; set; } = 50;

	public static string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
	                                               "(KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36";

	static HttpUtilities()
	{
		ServicePointManager.SecurityProtocol =
			SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;


		const string fieldName = "_sendStatus";

		RequestStatusField = typeof(HttpRequestMessage)
		                     .GetTypeInfo()
		                     .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

	}

	[CanBeNull]
	public static string GetFinalRedirect(string url)
	{
		// https://stackoverflow.com/questions/704956/getting-the-redirected-url-from-the-original-url

		if (String.IsNullOrWhiteSpace(url)) {
			return url;
		}

		const int MAX_REDIR = 8;

		int maxRedirCount = MAX_REDIR; // prevent infinite loops

		string newUrl = url;

		do {

			try {

				var resp1 = GetHttpResponse(url, method: HttpMethod.Head);
				var resp  = resp1.ResponseMessage;
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
		return '?' + String.Join('&', array);
	}

	public static Uri AddQuery(this Uri uri, string name, string value)
	{
		var collection = HttpUtility.ParseQueryString(uri.Query);

		collection.Remove(name);
		collection.Add(name, value);

		var ub = new UriBuilder(uri);

		// this code block is taken from httpValueCollection.ToString() method
		// and modified so it encodes strings with HttpUtility.UrlEncode
		if (collection.Count == 0) {
			ub.Query = String.Empty;
		}
		else {
			var sb = new StringBuilder();

			for (int i = 0; i < collection.Count; i++) {
				string text = collection.GetKey(i);

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

			ub.Query = sb.ToString();
		}

		return ub.Uri;
	}

	public static IHtmlDocument GetHtmlDocument(this HttpResponseMessage r)
	{
		Task<string> task = r.Content.ReadAsStringAsync();
		task.Wait();
		string html = task.Result;

		var parser = new HtmlParser();

		var document = parser.ParseDocument(html);
		return document;
	}

	public static string GetFile(string url, string folder)
	{
		string    fileName = Path.GetFileName(url);
		using var client   = new HttpClient();

		client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
		string dir = Path.Combine(folder, fileName);
		client.DownloadFile(url, dir);

		return dir;
	}


	public static Stream GetStream(this HttpClient client, string url)
	{
		var task = client.GetStreamAsync(url);
		task.Wait();
		return task.Result;
	}

	public static string DownloadString(this HttpClient client, string url)
	{
		var task = client.GetStringAsync(url);
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

	[CBN]
	[MURV]
	public static async Task<IFlurlResponse> GetHttpResponseAsync(string url, int? ms = null,
	                                                                   [CBN] HttpMethod method = null,
	                                                                   bool allowAutoRedirect = true,
	                                                                   int? maxAutoRedirects = null,
	                                                                   CancellationToken? token = null)
	{

		method ??= HttpMethod.Get;
		token  ??= CancellationToken.None;
		ms     ??= Timeout;

		var c = new FlurlClient()
		{
			Settings =
			{
				Timeout = TimeSpan.FromMilliseconds(ms.Value),
				Redirects =
				{
					MaxAutoRedirects = maxAutoRedirects ?? MaxAutoRedirects,
					Enabled          = allowAutoRedirect,
				}
			}
		};

		var f = new FlurlRequest(url)
		{
			Client = c,
			Settings =
				{ },
			Verb = method
		};
		
		f = f.AllowAnyHttpStatus();

		/*using var request = new HttpRequestMessage
		{
			RequestUri = new Uri(url),
			Method     = method ?? HttpMethod.Get
		};

		using var clientHandler = new HttpClientHandler
		{
			AllowAutoRedirect        = allowAutoRedirect,
			MaxAutomaticRedirections = maxAutoRedirects ?? MaxAutoRedirects
		};

		using var client = new HttpClient(clientHandler)
		{
			Timeout = TimeSpan.FromMilliseconds(ms ?? Timeout)
		};*/

		try {
			// var response = await client.SendAsync(request, c);
			var response = await f.SendAsync(method, cancellationToken: token.Value);
			return response;
		}
		catch (Exception x) {
			Debug.WriteLine($"{nameof(GetHttpResponseAsync)}: {x}", LogCategories.C_VERBOSE);

			return null;
		}


	}

	[CBN]
	[MURV]
	public static IFlurlResponse GetHttpResponse(string url, int? ms = null,
	                                                  [CBN] HttpMethod method = null,
	                                                  bool allowAutoRedirect = true,
	                                                  int? maxAutoRedirects = null,
	                                                  CancellationToken? token = null)
	{

		token ??= CancellationToken.None;
		var v = GetHttpResponseAsync(url, ms, method, allowAutoRedirect, maxAutoRedirects, token);
		// v.Wait(c.Token);

		if (v is { }) {
			v.Wait(cancellationToken: token.Value);
			return v.Result;

		}

		return null;
	}

	public static PingReply Ping(Uri u, int? ms = null) => Ping(IPUtilities.GetAddress(u.ToString()), ms);

	public static PingReply Ping(string hostOrIP, int? ms = null)
	{
		var ping = new Ping();

		var task = ping.SendPingAsync(hostOrIP, ms ?? Timeout);
		task.Wait();

		PingReply r = task.Result;

		return r;
	}

	public static void OpenUrl(string url)
	{
		// https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
		// url must start with a protocol i.e. http://

		try {
			Process.Start(url);
		}
		catch {
			// hack because of this: https://github.com/dotnet/corefx/issues/10361
			if (OperatingSystem.IsWindows()) {
				url = url.Replace("&", "^&");

				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
				{
					CreateNoWindow = true
				});
			}
			else {
				throw;
			}
		}
	}

	#region Message

	public static bool IsSent(this HttpRequestMessage m)
	{
		return m.GetStatus() == ALREADY_SENT;
	}

	public static int GetStatus(this HttpRequestMessage m)
	{
		var value = RequestStatusField.GetValue(m);
		Require.NotNull(value);
		return (int) value;
	}

	public static void ResetStatus(this HttpRequestMessage m)
	{
		RequestStatusField.SetValue(m, NOT_YET_SENT);
	}

	private static FieldInfo RequestStatusField { get; }

	private const int NOT_YET_SENT = 0;
	private const int ALREADY_SENT = 1;
	private const int IS_REDIRECT  = 2;

	#endregion
}