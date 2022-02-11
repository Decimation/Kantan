using System;
using System.Net.Http;
using System.Threading;

namespace Kantan.Net;

public class MediaResource : IDisposable
{
	public Uri Url { get; set; }

	public HttpResponseMessage Response { get; set; }

	public IMediaResourceFilter Filter { get; set; }

	public MediaResource() { }

	public bool Equals(MediaResource other)
	{
		return Url == other?.Url && Equals(Response, other?.Response);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Url, Response, Filter);
	}

	public void Dispose()
	{
		Response?.Dispose();
		GC.SuppressFinalize(this);

	}

	public static bool FromUrl(string url, IMediaResourceFilter filter, out MediaResource br, int? timeout = null,
	                           CancellationToken? token = null)
		=> FromUrl(url, filter, out br, out _, timeout, token);

	public static bool FromUrl(string url, IMediaResourceFilter filter, out MediaResource br,
	                           out HttpResponseMessage message, int? timeout = null, CancellationToken? token = null)
	{

		br = new();

		if (!UriUtilities.IsUri(url, out Uri u)) {
			message = null;
			return false;
		}

		using var client = new HttpClient();

		message = HttpUtilities.GetHttpResponse(url, timeout, token: token,
		                                        method: HttpMethod.Get);

		/*if (message is not { }) {
			return false;
		}*/

		if (message is not { IsSuccessStatusCode: true }) {
			message?.Dispose();
			return false;
		}

		br.Url      = new Uri(url);
		br.Response = message;

		/* Check content-type */

		// The content-type returned from the response may not be the actual content-type, so
		// we'll resolve it using binary data instead to be sure

		var length = message.Content.Headers.ContentLength;
		br.Response = message;

		string mediaType;

		try {
			/*
			 * #1
			 */
			mediaType = message.Content.Resolve();

		}
		catch (Exception) {
			/*
			 * #2
			 */
			mediaType = message.Content.Headers is { ContentType.MediaType: { } }
				            ? message.Content.Headers.ContentType.MediaType
				            : null;
		}

		const int UNLIMITED_SIZE = -1;

		bool type, size = length is UNLIMITED_SIZE;

		if (filter.MinimumSize.HasValue) {
			size = size || length >= filter.MinimumSize.Value;
		}


		if (filter.DiscreteType != default /*|| filter.DiscreteType!= DiscreteMimeType.Other*/) {
			if (mediaType != null) {
				type = mediaType.StartsWith(filter.DiscreteType, StringComparison.InvariantCultureIgnoreCase)
				       && !filter.TypeBlacklist.Contains(mediaType);
			}
			else {
				type = false; //?
			}
		}

		else {
			type = true;
		}


		return type && size;
	}
}