#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
using Kantan.Utilities;

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

	public string Url { get; init; }

	public bool IsBinaryType => ComputedType == HttpType.MT_APPLICATION_OCTET_STREAM;

	public bool IsFile { get; init; }

	[MemberNotNullWhen(true, nameof(Response))]
	[MemberNotNullWhen(true, nameof(SuppliedType))]
	public bool IsUri { get; init; }

	public bool IsBinary => (IsFile || IsUri) && IsBinaryType;

	public HttpResource() { }

	public static async Task<HttpResource> GetAsync(string u)
	{
		//todo: error handling

		IFlurlResponse response = null;

		Stream stream;
		string suppliedType = null;

		bool checkBug = false, noSniff = false;

		bool isFile = File.Exists(u),
		     isUri  = UriUtilities.IsUri(u, out var uu);


		if (isFile) {
			stream = File.OpenRead(u);
		}
		else if (isUri) {
			try {
				response = await u.WithClient(HttpUtilities.Client)
				                  .AllowAnyHttpStatus()
				                  .WithTimeout(HttpUtilities.Timeout)
				                  .GetAsync();

			}
			catch (Exception e) {
				Debug.WriteLine($"{e.Message}");

			}

			if (response == null) {
				return null;
			}

			suppliedType = GetSuppliedType(response, out checkBug);
			stream       = await response.GetStreamAsync();

			noSniff = response.Headers.TryGetFirst("X-Content-Type-Options", out var x) && x == "nosniff";
		}
		else {
			return null;
		}

		var header = await stream.ReadHeaderAsync(HttpType.RSRC_HEADER_LEN);

		var resource = new HttpResource()
		{
			Response      = response,
			Stream        = stream,
			SuppliedType  = suppliedType,
			CheckBugFlag  = checkBug,
			NoSniffFlag   = noSniff,
			ResolvedTypes = new List<HttpType>(),
			Header        = header,
			ComputedType  = HttpType.IsBinaryResource(header),

			IsFile = isFile,
			IsUri  = isUri,

			Url = u,
		};

		// resource.Resolve(true);
		if (resource.ComputedType == HttpType.MT_APPLICATION_OCTET_STREAM) {
			// resource.Resolve(true);
			//todo...
		}

		return resource;
	}

	public List<HttpType> Resolve(bool runExtra = false, IHttpTypeResolver extraResolver = null)
	{
		if (IsBinaryType) {
			// todo ...
		}

		var rg = HttpType.All.Where(t => HttpType.CheckPattern(Header, t))
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

		ResolvedTypes = rg.DistinctBy(x => x.Type).ToList();

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
		return
			$"{Url} ({IsBinaryType}) :: supplied: {SuppliedType} | computed: {ComputedType} | resolved: {ResolvedTypes.QuickJoin()}";
	}

	public void Dispose()
	{
		Response?.Dispose();
		Stream?.Dispose();
	}
}