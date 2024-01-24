using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Net.Properties;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Kantan.Net.Utilities;

public static class HostUtilities
{

	private const int TIMEOUT_DEFAULT = -1;

	public static Task<IPGeolocation> GetIPInformationAsync()
	{
		var task = (Resources.IFConfigUrl).WithHeaders(new
		{
			Accept     = "application/json",
			User_Agent = Resources.UserAgent,
		}).GetJsonAsync<IPGeolocation>();

		return task;
	}

	public static IPAddress GetHostAddress(string hostOrIP)
		=> Dns.GetHostAddresses(hostOrIP)
			.FirstOrDefault();

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

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
/*public class UserAgent
{
	[JsonPropertyName("product")]
	public string Product { get; internal set; }

	[JsonPropertyName("version")]
	public string Version { get; internal set; }

	[JsonPropertyName("raw_value")]
	public string RawValue { get; internal set; }
}*/