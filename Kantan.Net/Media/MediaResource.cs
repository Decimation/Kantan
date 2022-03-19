using System;
using System.Net.Http;
using System.Threading;
using Flurl.Http;
using Kantan.Net.Media.Filters;

namespace Kantan.Net.Media;

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

	public static bool FromUrl(string url, IMediaResourceFilter filter, out MediaResource mr,
	                           int? timeout = null, CancellationToken? token = null)
	{

		mr = new();

		if (!UriUtilities.IsUri(url, out Uri u)) {
			mr.Response = null;
			return false;
		}

		// using var client = new HttpClient();

		var flurlResponse = HttpUtilities.GetHttpResponse(url, timeout, token: token, method: HttpMethod.Get);

		// var t = url.WithTimeout(TimeSpan.FromMilliseconds(timeout ?? HttpUtilities.Timeout)).GetAsync();
		// t.Wait();

		mr.Response = flurlResponse.ResponseMessage;

		/*if (message is not { }) {
			return false;
		}*/

		if (mr.Response is not { IsSuccessStatusCode: true }) {
			mr.Response?.Dispose();
			return false;
		}

		mr.Url = new Uri(url);

		/* Check content-type */

		// The content-type returned from the response may not be the actual content-type, so
		// we'll resolve it using binary data instead to be sure

		var length = mr.Response.Content.Headers.ContentLength;

		string mediaType;

		try {
			/*
			 * #1
			 */
			mediaType = mr.Response.Content.Resolve();

		}
		catch (Exception) {
			/*
			 * #2
			 */
			mediaType = mr.Response.Content.Headers is { ContentType.MediaType: { } }
				            ? mr.Response.Content.Headers.ContentType.MediaType
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