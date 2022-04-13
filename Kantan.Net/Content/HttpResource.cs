#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Net.Content.Resolvers;
using Kantan.Net.Media;
using Kantan.Text;

#endregion

namespace Kantan.Net.Content;

/// <remarks>
///     <a href="https://mimesniff.spec.whatwg.org/#handling-a-resource">5</a>
/// </remarks>
public sealed class HttpResource : IDisposable
{
	public string SuppliedType { get; private set; }

	public bool CheckBugFlag { get; private set; }

	public bool NoSniffFlag { get; private set; }

	public string ComputedType { get; private set; }

	public Stream Stream { get; private set; }

	public IFlurlResponse Response { get; private set; }

	public byte[] Header { get; private set; }

	public List<HttpTypes> ResolvedTypes { get; private set; }

	private HttpResource() { }

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#supplied-mime-type-detection-algorithm">5.1</a>
	/// </remarks>
	private static string GetSuppliedType(IFlurlResponse r, out bool c)
	{
		c = false;

		const string CONTENT_TYPE_HEADER = "Content-Type";

		if (r.Headers.TryGetFirst(CONTENT_TYPE_HEADER, out string st)) {

			c = st is HttpTypes.MT_TEXT_PLAIN
				    or $"{HttpTypes.MT_TEXT_PLAIN} charset=ISO-8859-1"
				    or $"{HttpTypes.MT_TEXT_PLAIN} charset=iso-8859-1"
				    or $"{HttpTypes.MT_TEXT_PLAIN} charset=UTF-8";

		}

		// Skip 3
		// Skip 4
		// todo 5

		return st;
	}

	public static async Task<HttpResource> GetAsync(string u)
	{
		//todo: error handling

		IFlurlResponse response = null;

		/*if (!UriUtilities.IsUri(u, out var u2)) {
			return null;
		}*/

		try {
			response = await u.AllowAnyHttpStatus()
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

		var resource = new HttpResource()
		{
			Response      = response,
			Stream        = stream,
			SuppliedType  = GetSuppliedType(response, out var b),
			CheckBugFlag  = b,
			ResolvedTypes = new List<HttpTypes>(),
			Header        = await HttpResourceScanner.ReadResourceHeader(stream)
		};

		return resource;
	}

	public override string ToString()
	{
		return $"{nameof(SuppliedType)}: {SuppliedType}, " +
		       $"{nameof(ComputedType)}: {ComputedType}, " +
		       $"{nameof(Stream)}: {Stream}, " +
		       $"{nameof(ResolvedTypes)}: {ResolvedTypes.QuickJoin()}";
	}

	public List<HttpTypes> Resolve(bool runExtra = false, IHttpTypeResolver extraResolver = null)
	{
		if (ResolvedTypes is { }) {
			// todo ...
		}

		var rg = HttpTypes.All
		                  .Where(t => HttpResourceScanner.CheckPattern(Header, t))
		                  .ToList();

		if (runExtra) {

			extraResolver ??= IHttpTypeResolver.Default;
			var rx = extraResolver.Resolve(Stream);

			var type = new HttpTypes()
			{
				Type = rx
			};

			rg.Add(type);
		}

		ResolvedTypes = rg;

		return rg;
	}

	public void Dispose()
	{
		Response?.Dispose();
		Stream?.Dispose();
	}
}