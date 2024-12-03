// Read S Kantan.Net IPGeolocation.cs
// 2023-08-05 @ 1:51 AM

using System.Text.Json.Serialization;

// ReSharper disable InconsistentNaming

namespace Kantan.Net;

public struct IPGeolocation
{

	[JsonPropertyName("ip")]
	public string IP { get; internal set; }

	[JsonPropertyName("ip_decimal")]
	public long IPDecimal { get; internal set; }

	[JsonPropertyName("country")]
	public string Country { get; internal set; }

	[JsonPropertyName("country_iso")]
	public string CountryIso { get; internal set; }

	[JsonPropertyName("country_eu")]
	public bool CountryEu { get; internal set; }

	[JsonPropertyName("region_name")]
	public string RegionName { get; internal set; }

	[JsonPropertyName("region_code")]
	public string RegionCode { get; internal set; }

	[JsonPropertyName("metro_code")]
	public int MetroCode { get; internal set; }

	[JsonPropertyName("zip_code")]
	public string ZipCode { get; internal set; }

	[JsonPropertyName("city")]
	public string City { get; internal set; }

	[JsonPropertyName("latitude")]
	public double Latitude { get; internal set; }

	[JsonPropertyName("longitude")]
	public double Longitude { get; internal set; }

	[JsonPropertyName("time_zone")]
	public string TimeZone { get; internal set; }

	[JsonPropertyName("asn")]
	public string Asn { get; internal set; }

	[JsonPropertyName("asn_org")]
	public string AsnOrg { get; internal set; }

	[JsonPropertyName("hostname")]
	public string Hostname { get; internal set; }

	// [JsonPropertyName("user_agent")]
	// public UserAgent UserAgent { get; internal set; }

	public override string ToString()
	{
		// @formatter:off
		
		return $"{nameof(IP)}: {IP}, {nameof(IPDecimal)}: {IPDecimal}, {nameof(Country)}: {Country}, " +
		       $"{nameof(CountryIso)}: {CountryIso}, {nameof(CountryEu)}: {CountryEu}, {nameof(RegionName)}: {RegionName}, " +
		       $"{nameof(RegionCode)}: {RegionCode}, {nameof(MetroCode)}: {MetroCode}, {nameof(ZipCode)}: {ZipCode}, " +
		       $"{nameof(City)}: {City}, {nameof(Latitude)}: {Latitude}, {nameof(Longitude)}: {Longitude}, "           +
		       $"{nameof(TimeZone)}: {TimeZone}, {nameof(Asn)}: {Asn}, {nameof(AsnOrg)}: {AsnOrg}, {nameof(Hostname)}: {Hostname}";

		// @formatter:on
	}

}