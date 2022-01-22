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
	public static Task<IPGeolocation> GetIPInformationAsync()
	{
		var task = (Resources.IFConfigUrl).WithHeaders(new
		{
			Accept     = "application/json",
			User_Agent = HttpUtilities.UserAgent
		}).GetJsonAsync<IPGeolocation>();

		return task;
	}


	public static IPAddress GetHostAddress(string hostOrIP) => Dns.GetHostAddresses(hostOrIP)[0];

	public static string GetAddress(string hostOrIP)
	{
		string s = null;

		if (IPAddress.TryParse(hostOrIP, out IPAddress ip)) {
			s = ip.ToString();
		}

		if (UriUtilities.IsUri(hostOrIP, out Uri ux)) {
			s = ux.GetHostComponent();
		}

		return GetHostAddress(s).ToString();
	}
}