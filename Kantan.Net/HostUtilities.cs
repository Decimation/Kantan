using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Net.Properties;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Kantan.Net;

public static class HostUtilities
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

	public static IPAddress GetHostAddress(string hostOrIP) => Dns.GetHostAddresses(hostOrIP)
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

	public static PingReply Ping(Uri u, int? ms = null) => Ping(GetAddress(u.ToString()), ms);

	public static PingReply Ping(string hostOrIP, int? ms = null)
	{
		using var ping = new Ping();

		var task = ping.SendPingAsync(hostOrIP, ms ?? HttpUtilities.Timeout);
		task.Wait();

		PingReply r = task.Result;

		return r;
	}
}

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
/*public class UserAgent
{
	[JsonProperty("product")]
	public string Product { get; internal set; }

	[JsonProperty("version")]
	public string Version { get; internal set; }

	[JsonProperty("raw_value")]
	public string RawValue { get; internal set; }
}*/

public struct IPGeolocation
{
	[JsonProperty("ip")]
	public string IP { get; internal set; }

	[JsonProperty("ip_decimal")]
	public long IPDecimal { get; internal set; }

	[JsonProperty("country")]
	public string Country { get; internal set; }

	[JsonProperty("country_iso")]
	public string CountryIso { get; internal set; }

	[JsonProperty("country_eu")]
	public bool CountryEu { get; internal set; }

	[JsonProperty("region_name")]
	public string RegionName { get; internal set; }

	[JsonProperty("region_code")]
	public string RegionCode { get; internal set; }

	[JsonProperty("metro_code")]
	public int MetroCode { get; internal set; }

	[JsonProperty("zip_code")]
	public string ZipCode { get; internal set; }

	[JsonProperty("city")]
	public string City { get; internal set; }

	[JsonProperty("latitude")]
	public double Latitude { get; internal set; }

	[JsonProperty("longitude")]
	public double Longitude { get; internal set; }

	[JsonProperty("time_zone")]
	public string TimeZone { get; internal set; }

	[JsonProperty("asn")]
	public string Asn { get; internal set; }

	[JsonProperty("asn_org")]
	public string AsnOrg { get; internal set; }

	[JsonProperty("hostname")]
	public string Hostname { get; internal set; }

	// [JsonProperty("user_agent")]
	// public UserAgent UserAgent { get; internal set; }

	public override string ToString()
	{
		return $"{nameof(IP)}: {IP}, {nameof(IPDecimal)}: {IPDecimal}, {nameof(Country)}: {Country}, " +
		       $"{nameof(CountryIso)}: {CountryIso}, {nameof(CountryEu)}: {CountryEu}, {nameof(RegionName)}: {RegionName}, " +
		       $"{nameof(RegionCode)}: {RegionCode}, {nameof(MetroCode)}: {MetroCode}, {nameof(ZipCode)}: {ZipCode}, " +
		       $"{nameof(City)}: {City}, {nameof(Latitude)}: {Latitude}, {nameof(Longitude)}: {Longitude}, {nameof(TimeZone)}: {TimeZone}, " +
		       $"{nameof(Asn)}: {Asn}, {nameof(AsnOrg)}: {AsnOrg}, {nameof(Hostname)}: {Hostname}";
	}
}