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

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
/*public class UserAgent
{
	[JsonProperty("product")]
	public string Product { get; set; }

	[JsonProperty("version")]
	public string Version { get; set; }

	[JsonProperty("raw_value")]
	public string RawValue { get; set; }
}*/

public struct IPGeolocation
{
	[JsonProperty("ip")]
	public string Ip { get; set; }

	[JsonProperty("ip_decimal")]
	public long IpDecimal { get; set; }

	[JsonProperty("country")]
	public string Country { get; set; }

	[JsonProperty("country_iso")]
	public string CountryIso { get; set; }

	[JsonProperty("country_eu")]
	public bool CountryEu { get; set; }

	[JsonProperty("region_name")]
	public string RegionName { get; set; }

	[JsonProperty("region_code")]
	public string RegionCode { get; set; }

	[JsonProperty("metro_code")]
	public int MetroCode { get; set; }

	[JsonProperty("zip_code")]
	public string ZipCode { get; set; }

	[JsonProperty("city")]
	public string City { get; set; }

	[JsonProperty("latitude")]
	public double Latitude { get; set; }

	[JsonProperty("longitude")]
	public double Longitude { get; set; }

	[JsonProperty("time_zone")]
	public string TimeZone { get; set; }

	[JsonProperty("asn")]
	public string Asn { get; set; }

	[JsonProperty("asn_org")]
	public string AsnOrg { get; set; }

	[JsonProperty("hostname")]
	public string Hostname { get; set; }

	// [JsonProperty("user_agent")]
	// public UserAgent UserAgent { get; set; }


	public override string ToString()
	{
		return $"{nameof(Ip)}: {Ip}, {nameof(IpDecimal)}: {IpDecimal}, {nameof(Country)}: {Country}, " +
			   $"{nameof(CountryIso)}: {CountryIso}, {nameof(CountryEu)}: {CountryEu}, {nameof(RegionName)}: {RegionName}, " +
			   $"{nameof(RegionCode)}: {RegionCode}, {nameof(MetroCode)}: {MetroCode}, {nameof(ZipCode)}: {ZipCode}, " +
			   $"{nameof(City)}: {City}, {nameof(Latitude)}: {Latitude}, {nameof(Longitude)}: {Longitude}, {nameof(TimeZone)}: {TimeZone}, " +
			   $"{nameof(Asn)}: {Asn}, {nameof(AsnOrg)}: {AsnOrg}, {nameof(Hostname)}: {Hostname}";
	}
}