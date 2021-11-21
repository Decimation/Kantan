using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Web;
using JetBrains.Annotations;
using Kantan.Model;
using RestSharp;

#pragma warning disable 8600
#pragma warning disable 8604

// ReSharper disable CognitiveComplexity

// ReSharper disable InconsistentNaming

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

// ReSharper disable UnusedMember.Global
#nullable enable

namespace Kantan.Net;

/// <summary>
/// Network, HTTP, web utilities
/// </summary>
/// <seealso cref="WebUtilities"/>
/// <seealso cref="Dns"/>
/// <seealso cref="IPAddress"/>
/// <seealso cref="HttpUtility"/>
public static class Network
{
	internal const int TimeoutMS = 2000;

	private const int MAX_AUTO_REDIRECTS = 50;

	#region IP

	public static IPAddress GetExternalIP()
	{
		using var client = new HttpClient();

		var s = client.DownloadString("https://icanhazip.com/").Trim();

		return IPAddress.Parse(s);
	}

	public static IPGeolocation GetAddressLocation(IPAddress ip) => GetAddressLocation(ip.ToString());

	public static IPGeolocation GetAddressLocation(string hostOrIP)
	{
		var rc = new RestClient("https://freegeoip.app/{format}/{host}");
		var r  = new RestRequest();

		r.AddUrlSegment("format", "json");
		r.AddUrlSegment("host", hostOrIP);

		var res = rc.Execute<IPGeolocation>(r, Method.GET);

		return res.Data;
	}

	public static IPAddress GetHostAddress(string hostOrIP) => Dns.GetHostAddresses(hostOrIP)[0];

	public static string GetAddress(string hostOrIP)
	{
		string s = null;

		if (IPAddress.TryParse(hostOrIP, out var ip)) {
			s = ip.ToString();
		}

		if (UriUtilities.IsUri(hostOrIP, out var ux)) {
			s = UriUtilities.GetHostComponent(ux);
		}

		return GetHostAddress(s).ToString();
	}

	public static PingReply Ping(Uri u, int ms = TimeoutMS) => Ping(GetAddress(u.ToString()), ms);

	public static PingReply Ping(string hostOrIP, int ms = TimeoutMS)
	{
		var ping = new Ping();

		var task = ping.SendPingAsync(hostOrIP, ms);
		task.Wait();

		PingReply r = task.Result;

		return r;
	}

	#endregion

	//public static bool IsAlive(Uri u, int ms = TimeoutMS) => GetResponse(u.ToString(), ms).IsSuccessful;

	public static string? GetFinalRedirect(string url)
	{
		// https://stackoverflow.com/questions/704956/getting-the-redirected-url-from-the-original-url

		if (String.IsNullOrWhiteSpace(url))
			return url;

		const int MAX_REDIR = 8;

		int maxRedirCount = MAX_REDIR; // prevent infinite loops

		string? newUrl = url;

		do {

			try {

				IRestResponse resp = GetResponse(url, TimeoutMS, Method.HEAD);

				switch (resp.StatusCode) {
					case HttpStatusCode.OK:
						return newUrl;

					case HttpStatusCode.Redirect:
					case HttpStatusCode.MovedPermanently:
					case HttpStatusCode.RedirectKeepVerb:
					case HttpStatusCode.RedirectMethod:
						newUrl = (string) resp.Headers.First(x => x.Name == "Location").Value;

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

	#region Response

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

	[MustUseReturnValue]
	public static HttpResponseMessage GetHttpResponse(string url, int ms = TimeoutMS, HttpMethod? method = null,
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

		var response = client.Send(request);

		return response;
	}
#endif


	public static IRestResponse GetResponse(string url, int ms = TimeoutMS, Method method = Method.GET,
	                                        bool redirect = true, int redirects = MAX_AUTO_REDIRECTS)
	{
		var client = new RestClient
		{
			FollowRedirects = redirect,
			Proxy           = null,
			MaxRedirects    = redirects,

		};

		var request = new RestRequest(url)
		{
			Method  = method,
			Timeout = ms
		};

		var response = client.Execute(request);

		return response;
	}

	public static void DumpResponse(IRestResponse response)
	{
		var ct = new ConsoleTable("-", "Value");

		ct.AddRow("Uri", response.ResponseUri);
		ct.AddRow("Successful", response.IsSuccessful);
		ct.AddRow("Status code", response.StatusCode);
		ct.AddRow("Error message", response.ErrorMessage);

		var str = ct.ToString();

		Trace.WriteLine(str);
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

	#endregion
}