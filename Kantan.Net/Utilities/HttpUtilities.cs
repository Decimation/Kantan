using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Flurl.Http;
using Kantan.Diagnostics;
using Kantan.Net.Properties;
using Url = Flurl.Url;

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

namespace Kantan.Net.Utilities;

/// <summary>
///     Network, HTTP, web utilities
/// </summary>
/// <seealso cref="Dns" />
/// <seealso cref="IPAddress" />
/// <seealso cref="HttpUtility" />
public static class HttpUtilities
{
	public static int Timeout { get; set; } = 2000;

	public static int MaxAutoRedirects { get; set; } = 50;

	public static string UserAgent { get; set; } = Resources.UserAgent;

	public static FlurlClient Client { get; internal set; } = new()
	{
		Settings =
		{
			Redirects =
			{
				AllowSecureToInsecure = true,
				Enabled               = true
			},

		},

	};

	static HttpUtilities()
	{
		/*Client = new()
		{
			Settings =
				{ }
		};*/

		ServicePointManager.DefaultConnectionLimit = 300;

		ServicePointManager.SecurityProtocol =
			SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

		const string fieldName = "_sendStatus";

		RequestStatusField = typeof(HttpRequestMessage)
		                     .GetTypeInfo()
		                     .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

	}

	[CBN]
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
				var req = new FlurlRequest(url)
				{
					Settings =
					{
						Redirects =
						{
							Enabled          = true,
							MaxAutoRedirects = MaxAutoRedirects
						}
					},
					Verb = HttpMethod.Head
				};

				var resp1 = req.WithClient(Client).AllowAnyHttpStatus().SendAsync(HttpMethod.Head);
				resp1.Wait();
				var resp = resp1?.Result.ResponseMessage;

				switch (resp.StatusCode) {
					case HttpStatusCode.OK:
						return newUrl;

					case HttpStatusCode.Redirect:
					case HttpStatusCode.MovedPermanently:
					case HttpStatusCode.RedirectKeepVerb:
					case HttpStatusCode.RedirectMethod:
						newUrl = resp.Content.Headers.ContentLocation?.ToString();

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

	public static string GetFile(string url, string folder)
	{
		string    fileName = Path.GetFileName(url);
		using var client   = new HttpClient();

		client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
		string dir = Path.Combine(folder, fileName);
		client.DownloadFile(url, dir);

		return dir;
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

		var f = new FlurlRequest(url)
		{
			Client = Client,
			Settings =
			{
				Timeout = TimeSpan.FromMilliseconds(ms.Value),
				Redirects =
				{
					MaxAutoRedirects = maxAutoRedirects ?? MaxAutoRedirects,
					Enabled          = allowAutoRedirect
				}
			},
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

		IFlurlResponse response = null;

		try {
			// var response = await client.SendAsync(request, c);
			response = await f.SendAsync(method, cancellationToken: token.Value);
		}
		catch (Exception x) {
			Debug.WriteLine($"{x}", nameof(GetHttpResponseAsync));

		}

		return response;

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

		v?.Wait(token.Value);

		return v?.Result;
	}

	public static async Task<IFlurlResponse> TryGetResponseAsync(string u)
	{
		//todo: wtf
		IFlurlResponse response = null;

		try {
			response = await u.WithClient(Client)
			                  .AllowAnyHttpStatus()
			                  .WithTimeout(Timeout)
			                  .GetAsync();
		}
		catch (Exception e) {
			Debug.WriteLine($"{e.Message}", nameof(HttpUtilities));
		}

		return response;
	}

	/*[CBN]
	public static string Download(Uri src, string path)
	{
		string    filename = UriUtilities.NormalizeFilename(src);
		string    combine  = Path.Combine(path, filename);
		using var wc       = new WebClient();

		Debug.WriteLine($"{nameof(HttpUtilities)}: Downloading {src} to {combine} ...");

		try {
			wc.DownloadFile(src.ToString(), combine);
			return combine;
		}
		catch (Exception e) {
			Debug.WriteLine($"{e.Message}", nameof(GetHttpResponseAsync));
			return null;
		}
	}*/

	public static bool TryOpenUrl([CBN] string u)
	{
		try {
			var b = (!string.IsNullOrWhiteSpace(u)) && Url.IsValid(u);

			if (b) {
				OpenUrl(u);
			}

			return b;
		}
		catch (Exception e) {
			return false;
		}
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

	private static readonly FieldInfo RequestStatusField;

	private const int NOT_YET_SENT = 0;
	private const int ALREADY_SENT = 1;
	private const int IS_REDIRECT  = 2;

	#endregion

	/*public static async Task<IEnumerable<FileType>> GetResolvedMediaType(this IFlurlResponse r, IFileTypeResolver resolver = null)
	{
		resolver ??= IFileTypeResolver.Default;

		var rg = await r.GetBytesAsync();
		var t  = resolver.Resolve(rg);

		return t;
	}*/
}