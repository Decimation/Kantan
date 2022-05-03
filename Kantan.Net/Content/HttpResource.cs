#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Net.Content.Resolvers;
using Kantan.Net.Utilities;
using Kantan.Text;

#endregion

namespace Kantan.Net.Content;

/// <remarks>
///     <a href="https://mimesniff.spec.whatwg.org/#handling-a-resource">5</a>
/// </remarks>
public sealed class HttpResource : IDisposable
{
	public bool CheckBugFlag { get; init; }

	public bool NoSniffFlag { get; init; }

	public Stream Stream { get; init; }

	public IFlurlResponse Response { get; init; }

	public byte[] Header { get; init; }

	public string SuppliedType { get; init; }

	public string ComputedType { get; private set; }

	public List<HttpType> ResolvedTypes { get; private set; }

	// public string Type => ComputedType ?? SuppliedType;

	public string Url { get; init; }

	public bool IsBinary => ComputedType == HttpType.MT_APPLICATION_OCTET_STREAM;

	public HttpResource() { }

	public static async Task<HttpResource> GetAsync(string u)
	{
		//todo: error handling

		IFlurlResponse response = null;

		/*if (!UriUtilities.IsUri(u, out var u2)) {
			return null;
		}*/

		try {
			response = await u.WithClient(HttpUtilities.Client)
			                  .AllowAnyHttpStatus()
			                  .WithTimeout(HttpUtilities.Timeout)
			                  .GetAsync();

			// response = new HttpClient().GetAsync(u);
		}
		catch (Exception e) {
			Debug.WriteLine($"{e.Message}");

		}

		if (response == null) {
			return null;

		}

		/*if (response is not { ResponseMessage: { IsSuccessStatusCode: true } }) {
			return null;
		}*/

		var stream = await response.GetStreamAsync();

		var header = await HttpScanner.ReadResourceHeader(stream);

		var resource = new HttpResource()
		{
			Response      = response,
			Stream        = stream,
			SuppliedType  = GetSuppliedType(response, out bool b),
			CheckBugFlag  = b,
			ResolvedTypes = new List<HttpType>(),
			Header        = header,
			ComputedType  = HttpScanner.IsBinaryResource(header),
			Url           = u
		};
		
		// resource.Resolve(true);

		return resource;
	}

	public List<HttpType> Resolve(bool runExtra = false, IHttpTypeResolver extraResolver = null)
	{
		if (IsBinary) {
			// todo ...
		}

		List<HttpType> rg = HttpType.All.Where(t => HttpScanner.CheckPattern(Header, t))
		                            .ToList();

		if (runExtra) {

			extraResolver ??= IHttpTypeResolver.Default;

			string rx = extraResolver.Resolve(Stream);

			var type = new HttpType()
			{
				Type = rx
			};

			rg.Add(type);
		}

		ResolvedTypes = rg.DistinctBy(x=>x.Type).ToList();

		return rg;
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#supplied-mime-type-detection-algorithm">5.1</a>
	/// </remarks>
	private static string GetSuppliedType(IFlurlResponse r, out bool c)
	{
		c = false;

		const string CONTENT_TYPE_HEADER = "Content-Type";

		if (r.Headers.TryGetFirst(CONTENT_TYPE_HEADER, out string st))
			c = st is HttpType.MT_TEXT_PLAIN
				    or $"{HttpType.MT_TEXT_PLAIN} charset=ISO-8859-1"
				    or $"{HttpType.MT_TEXT_PLAIN} charset=iso-8859-1"
				    or $"{HttpType.MT_TEXT_PLAIN} charset=UTF-8";

		// Skip 3
		// Skip 4
		// todo 5

		return st;
	}

	public override string ToString()
	{
		return $"{Url}:: supplied: {SuppliedType} | computed: {ComputedType} | resolved: {ResolvedTypes.QuickJoin()}";
	}

	public void Dispose()
	{
		Response?.Dispose();
		Stream?.Dispose();
	}
}