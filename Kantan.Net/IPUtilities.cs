using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Kantan.Net.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Kantan.Net;

public static class IPUtilities
{
	public static IPAddress GetExternalIP()
	{
		using var client = new HttpClient();

		var s = client.DownloadString(Resources.ExternalIPUrl).Trim();

		return IPAddress.Parse(s);
	}

	public static IPGeolocation GetAddressLocation(IPAddress ip) => GetAddressLocation(ip.ToString());

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

					string   val  = (text != null) ? (text + "=") : string.Empty;
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

	public static IPGeolocation GetAddressLocation(string hostOrIP)
	{
		using var rc = new HttpClient();

		var u = new Uri(Resources.GeoIPUrl+hostOrIP);

		using var request = new HttpRequestMessage(HttpMethod.Get, u);

		using var res     = rc.Send(request);

		var task = res.Content.ReadAsStringAsync();
		task.Wait();

		var json = task.Result;
		return JsonConvert.DeserializeObject<IPGeolocation>(json);
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
}