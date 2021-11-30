using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
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
	
	public static bool ResetStatus(this HttpRequestMessage request)
	{
		const int MessageNotYetSent  = 0;
		const int MessageAlreadySent = 1;
		const int MessageIsRedirect  = 2;

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

	public static string ToQueryString(this NameValueCollection nvc)
	{
		var array = (
			            from key in nvc.AllKeys
			            from value in nvc.GetValues(key)
			            select string.Format(
				            "{0}={1}",
				            HttpUtility.UrlEncode(key),
				            HttpUtility.UrlEncode(value))
		            ).ToArray();
		return "?" + string.Join("&", array);
	}

	public static Uri AddQuery(this Uri uri, string name, string value)
	{
		var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

		httpValueCollection.Remove(name);
		httpValueCollection.Add(name, value);

		var ub = new UriBuilder(uri);

		// this code block is taken from httpValueCollection.ToString() method
		// and modified so it encodes strings with HttpUtility.UrlEncode
		if (httpValueCollection.Count == 0)
			ub.Query = String.Empty;
		else {
			var sb = new StringBuilder();

			for (int i = 0; i < httpValueCollection.Count; i++) {
				string text = httpValueCollection.GetKey(i);

				{
					text = HttpUtility.UrlEncode(text);

					string   val    = (text != null) ? (text + "=") : string.Empty;
					string[] values = httpValueCollection.GetValues(i);

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
}