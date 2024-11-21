using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

	public static readonly string UserAgent
		= Resources.UserAgent;

	static HttpUtilities()
	{
		/*Client = new()
		{
			Settings =
				{ }
		};*/

		ServicePointManager.DefaultConnectionLimit = int.MaxValue;

		ServicePointManager.SecurityProtocol =
			SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

		const string fieldName = "_sendStatus";

		RequestStatusField = typeof(HttpRequestMessage)
			.GetTypeInfo()
			.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

	}

	#region

	public static async Task<bool> WriteResponseDataAsync(this HttpListenerResponse response, byte[] responseBytes,
	                                                      CancellationToken ct = default)
	{
			response.ContentType     = MediaTypeNames.Text.Plain;
			response.StatusCode      = (int) HttpStatusCode.OK;
			response.ContentLength64 = responseBytes.Length;
			await response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length, ct);

 response.OutputStream.Close();

			// state.Dispose();
			response.Close();
			return true;
		


	}

	public static Task<bool> WriteResponseStringAsync(this HttpListenerResponse req, string s,
	                                                  CancellationToken ct = default)
	{
		var encoding = req.ContentEncoding ?? DefaultEncoding;
		var bytes    = encoding.GetBytes(s);
		return req.WriteResponseDataAsync(bytes, ct);
	}

	public static Task<bool> WriteResponseJsonAsync<T>(this HttpListenerResponse req, T t,
	                                                   CancellationToken ct = default)
	{
		var bytes = JsonSerializer.Serialize(t, Options);
		return req.WriteResponseStringAsync(bytes, ct);
	}

	public static async Task<byte[]> ReadRequestDataAsync(this HttpListenerRequest req, CancellationToken ct = default)
	{
		var len = req.ContentLength64;

		if (len == KantanInit.INVALID) {
			if (!req.InputStream.CanSeek) {
				return [];
			}

			len = req.InputStream.Length;
		}

		var responseData = new byte[len];
		var cb           = await req.InputStream.ReadAsync(responseData, 0, (int) len, ct);

		if (cb < len) {
			Debugger.Break();
		}

		return responseData;
	}

	public static async Task<string> ReadRequestStringAsync(this HttpListenerRequest req,
	                                                        CancellationToken ct = default)
	{
		var data = await req.ReadRequestDataAsync(ct);
		var s    = req.ContentEncoding.GetString(data);

		return s;
	}

	public static async Task<T> ReadRequestJsonAsync<T>(this HttpListenerRequest req, CancellationToken ct = default)
	{
		var data = await req.ReadRequestDataAsync(ct);
		var obj  = JsonSerializer.Deserialize<T>(data, Options);

		return obj;
	}

	#endregion

	public static async Task<Url> GetFinalRedirectAsync(Url url)
	{
		// https://stackoverflow.com/questions/704956/getting-the-redirected-url-from-the-original-url

		Require.NotNullOrWhiteSpace(url);

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
							Enabled               = true,
							MaxAutoRedirects      = MAX_REDIR,
							AllowSecureToInsecure = true,
						},
						Timeout = TimeSpan.FromSeconds(5),
					},
					Verb = HttpMethod.Head,
				};

				var resp1 = await req.AllowAnyHttpStatus()
					            .SendAsync(HttpMethod.Head);

				var resp = resp1.ResponseMessage;

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
				return newUrl;
			}
		} while (maxRedirCount-- > 0);

		return newUrl;
	}

	/*[CBN]
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
		};#1#

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
	*/

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

	public static bool TryOpenUrl([CBN] string u, out Process p)
	{
		bool b;
		p = null;

		try {
			b = (!string.IsNullOrWhiteSpace(u)) && Url.IsValid(u);

			if (b) {
				OpenUrl(u, out p);
			}
		}
		catch (Exception e) {
			b = false;
		}

		return b;
	}

	public static void OpenUrl(string url, out Process p)
	{
		//todo
		// https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
		// url must start with a protocol i.e. http://

		try {
			p = Process.Start(url);
		}
		catch {
			// hack because of this: https://github.com/dotnet/corefx/issues/10361
			if (OperatingSystem.IsWindows()) {
				url = url.Replace("&", "^&");

				p = Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
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

	public static readonly JsonSerializerOptions Options = new(JsonSerializerOptions.Default)
	{
		IncludeFields          = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.Never,
		Converters =
		{
			new UrlTypeConverter()
		}
	};

	public static readonly Encoding DefaultEncoding = Encoding.UTF8;

}