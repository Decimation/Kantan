// ReSharper disable InconsistentNaming
namespace Kantan.Net;

public sealed class IPGeolocation
{
	public string IP { get; internal set; }

	public string CountryCode { get; internal set; }

	public string CountryName { get; internal set; }

	public string RegionCode { get; internal set; }

	public string RegionName { get; internal set; }

	public string City { get; internal set; }

	public string ZipCode { get; internal set; }

	public string TimeZone { get; internal set; }

	public double Latitude { get; internal set; }

	public double Longitude { get; internal set; }

	public int MetroCode { get; internal set; }

	public override string ToString()
	{
		return $"{nameof(IP)}: {IP}\n"                   +
		       $"{nameof(CountryCode)}: {CountryCode}\n" +
		       $"{nameof(CountryName)}: {CountryName}\n" +
		       $"{nameof(RegionCode)}: {RegionCode}\n"   +
		       $"{nameof(RegionName)}: {RegionName}\n"   +
		       $"{nameof(City)}: {City}\n"               +
		       $"{nameof(ZipCode)}: {ZipCode}\n"         +
		       $"{nameof(TimeZone)}: {TimeZone}\n"       +
		       $"{nameof(Latitude)}: {Latitude}\n"       +
		       $"{nameof(Longitude)}: {Longitude}\n"     +
		       $"{nameof(MetroCode)}: {MetroCode}";
	}
}