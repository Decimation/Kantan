// ReSharper disable InconsistentNaming

using Newtonsoft.Json;

namespace Kantan.Net;

public struct IPGeolocation
{
	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
	[JsonProperty("status")]
	public string Status { get; set; }

	[JsonProperty("country")]
	public string Country { get; set; }

	[JsonProperty("countryCode")]
	public string CountryCode { get; set; }

	[JsonProperty("region")]
	public string Region { get; set; }

	[JsonProperty("regionName")]
	public string RegionName { get; set; }

	[JsonProperty("city")]
	public string City { get; set; }

	[JsonProperty("zip")]
	public string Zip { get; set; }

	[JsonProperty("lat")]
	public double Lat { get; set; }

	[JsonProperty("lon")]
	public double Lon { get; set; }

	[JsonProperty("timezone")]
	public string Timezone { get; set; }

	[JsonProperty("isp")]
	public string Isp { get; set; }

	[JsonProperty("org")]
	public string Org { get; set; }

	[JsonProperty("as")]
	public string As { get; set; }

	[JsonProperty("query")]
	public string Query { get; set; }

	public override string ToString()
	{
		return $"{nameof(Status)}: {Status}, {nameof(Country)}: {Country}, " +
		       $"{nameof(CountryCode)}: {CountryCode}, {nameof(Region)}: {Region}, " +
		       $"{nameof(RegionName)}: {RegionName}, " + $"{nameof(City)}: {City}, {nameof(Zip)}: {Zip}, " +
		       $"{nameof(Lat)}: {Lat}, {nameof(Lon)}: {Lon}, " +
		       $"{nameof(Timezone)}: {Timezone}, {nameof(Isp)}: {Isp}, {nameof(Org)}: {Org}, " +
		       $"{nameof(As)}: {As}, {nameof(Query)}: {Query}";
	}
}