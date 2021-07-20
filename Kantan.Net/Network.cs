using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Web;
using Kantan.Model;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions;
using Formatting = System.Xml.Formatting;

#pragma warning disable 8602

#pragma warning disable 8600
#pragma warning disable 8604

// ReSharper disable CognitiveComplexity

// ReSharper disable InconsistentNaming
#pragma warning disable 8618

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

// ReSharper disable UnusedMember.Global
#nullable enable

namespace Kantan.Net
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="WebUtilities"/>
	/// <seealso cref="Dns"/>
	/// <seealso cref="IPAddress"/>
	/// <seealso cref="HttpUtility"/>
	public static class Network
	{
		private const int TimeoutMS = 3000;

		#region URI

		public static Uri GetHostUri(Uri u)
		{
			return new UriBuilder(u.Host).Uri;
		}

		public static string GetHostComponent(Uri u)
		{
			return u.GetComponents(UriComponents.NormalizedHost, UriFormat.Unescaped);
		}

		public static string StripScheme(Uri uri)
		{
			return uri.Host + uri.PathAndQuery + uri.Fragment;
		}

		public static bool IsUri(string uriName, out Uri? uriResult)
		{
			bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
			              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);


			// if (!result) {
			// 	var b = new UriBuilder(StripScheme(new Uri(uriName)))
			// 	{
			// 		Scheme = Uri.UriSchemeHttps, 
			// 		Port = -1
			// 	};
			// 	uriResult = b.Uri;
			//
			// 	//uriResult = new Uri(uriResult.ToString().Replace("file:", "http:"));
			// }


			return result;
		}

		#endregion

		#region IP

		public static IPAddress GetExternalIP()
		{
			using var client = new WebClient();

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

			if (IsUri(hostOrIP, out var ux)) {
				s = GetHostComponent(ux);
			}

			return GetHostAddress(s).ToString();
		}

		public static PingReply Ping(Uri u, int ms = TimeoutMS) =>
			Ping(GetAddress(u.ToString()), ms);


		public static PingReply Ping(string hostOrIP, int ms = TimeoutMS)
		{
			var ping = new Ping();

			var task = ping.SendPingAsync(hostOrIP, (int) ms);
			task.Wait();

			PingReply r = task.Result;

			return r;
		}

		#endregion

		public static bool IsType(string url, string type, int ms = TimeoutMS)
		{
			if (!IsUri(url, out var u)) {
				return false;
			}

			var response = GetResponse(u.ToString(), ms, Method.HEAD);

			return response.IsSuccessful && response.ContentType.StartsWith(type);
		}

		public static bool IsAlive(Uri u, int ms = TimeoutMS) => GetResponse(u.ToString(), ms).IsSuccessful;

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
				finally {
					//....
				}
			} while (maxRedirCount-- > 0);

			return newUrl;
		}


		public static IRestResponse GetResponse(string url, int ms = TimeoutMS, Method method = Method.GET)
		{
			var client = new RestClient
			{
				FollowRedirects = false,
				Proxy           = null
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
	}
}