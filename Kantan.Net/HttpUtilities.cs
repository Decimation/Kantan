using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Web;
using JetBrains.Annotations;
using Kantan.Model;
using Kantan.Net.Properties;

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


#if USE_WC
	[MustUseReturnValue]
	public static HttpWebResponse GetWebResponse(string url, int ms = TimeoutMS, string method = "GET",
	                                             bool redirect = true, int redirects = MAX_AUTO_REDIRECTS)
	{
		var h = (HttpWebRequest) WebRequest.Create(url);
		h.AllowAutoRedirect = redirect;
		h.Method = method;
		h.MaximumAutomaticRedirections = redirects;
		h.Timeout = ms;

		var r = (HttpWebResponse) h.GetResponse();

		return r;
	}

#else

	public static bool ResetSendStatus(this HttpRequestMessage request)
	{
		const int MessageNotYetSent  = 0;
		const int MessageAlreadySent = 1;
		const int MessageIsRedirect  = 2;

		const string sendStatusFieldName = "_sendStatus";

		// internal bool MarkAsSent() => Interlocked.CompareExchange(ref _sendStatus, MessageAlreadySent, MessageNotYetSent) == MessageNotYetSent;

		var requestType = request.GetType().GetTypeInfo();


		var sendStatusField =
			requestType.GetField(sendStatusFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

		if (sendStatusField != null) {
			sendStatusField.SetValue(request, MessageNotYetSent);
			return true;
		}
		else
			return false;
	}

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
			Debug.WriteLine($"[{nameof(GetHttpResponse)}]: {x.Message}");
			return null;
		}

	}
#endif


	public static PingReply Ping(Uri u, int ms = TIMEOUT_MS) => Ping(IPUtilities.GetAddress(u.ToString()), ms);

	public static PingReply Ping(string hostOrIP, int ms = TIMEOUT_MS)
	{
		var ping = new Ping();

		var task = ping.SendPingAsync(hostOrIP, ms);
		task.Wait();

		PingReply r = task.Result;

		return r;
	}

	public static void DumpResponse(HttpWebResponse response)
	{
		var ct = new ConsoleTable("-", "Value");

		ct.AddRow("Uri", response.ResponseUri);
		ct.AddRow("Status code", response.StatusCode);
		ct.AddRow("Status message", response.StatusDescription);

		var str = ct.ToString();

		Trace.WriteLine(str);
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
}