using System;
using System.IO;
using System.Threading.Tasks;
using Flurl.Http;

namespace Kantan.Net.Content;

/// <remarks><a href="https://mimesniff.spec.whatwg.org/#handling-a-resource">5</a></remarks>
public sealed class MBinaryResource : IDisposable
{
	public string SuppliedType { get; private set; }

	public bool CheckBugFlag { get; private set; }

	public bool NoSniffFlag { get; private set; }

	public string ComputedType { get; private set; }

	public Stream Stream { get; private set; }

	public IFlurlResponse Response { get; private set; }

	public byte[] Header { get; private set; }

	public static async Task<MBinaryResource> GetResourceAsync(string u)
	{
		//todo: error handling
		var response = await u.AllowHttpStatus("400-404,6xx").AllowAnyHttpStatus().GetAsync();

		if (response is not { ResponseMessage: { IsSuccessStatusCode: true } }) {
			return null;
		}

		var stream = await response.GetStreamAsync();

		var resource = new MBinaryResource()
		{
			Response     = response,
			Stream       = stream,
			SuppliedType = GetSuppliedType(response, out var b),
			CheckBugFlag = b,
			Header       = await MScanner.ReadResourceHeader(stream)
		};

		return resource;
	}

	/// <remarks><a href="https://mimesniff.spec.whatwg.org/#supplied-mime-type-detection-algorithm">5.1</a></remarks>
	private static string GetSuppliedType(IFlurlResponse r, out bool c)
	{

		c = false;

		if (r.Headers.TryGetFirst("Content-Type", out string st)) {

			c = st is "text/plain"
				    or "text/plain; charset=ISO-8859-1"
				    or "text/plain; charset=iso-8859-1"
				    or "text/plain; charset=UTF-8";

		}

		// Skip 3
		// Skip 4
		// todo 5

		return st;
	}

	public void Dispose()
	{
		Stream?.Dispose();
		Response?.Dispose();
	}
}