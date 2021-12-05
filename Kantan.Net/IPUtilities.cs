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
using Flurl;
using Flurl.Http;
using Kantan.Net.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Kantan.Net;

public static class IPUtilities
{
	/*public static IPAddress GetExternalIP()
	{
		using var client = new HttpClient();

		var s = client.DownloadString(Resources.ExternalIPUrl).Trim();

		return IPAddress.Parse(s);
	}*/
	
	public static IPGeolocation GetIPInformation()
	{
		var task = (Resources.IFConfigUrl).WithHeaders(new
		{
			Accept     = "application/json",
			User_Agent = HttpUtilities.UserAgent
		}).GetJsonAsync<IPGeolocation>();

		task.Wait();
		return task.Result;
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