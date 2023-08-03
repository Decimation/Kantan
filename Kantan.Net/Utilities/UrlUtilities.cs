// Read S Kantan.Net UrlUtilities.cs
// 2023-07-23 @ 11:56 PM

using System.Web;
using Flurl;

namespace Kantan.Net.Utilities;

/// <summary>
/// <seealso cref="UriUtilities"/>
/// </summary>
public static class UrlUtilities
{
	public static string GetFileName(this Url v)
	{
		string path;

		if (v.PathSegments is { Count: >= 1 })
		{
			path = v.PathSegments[^1];
		}
		else
			path = v.Path;

		// path = HttpUtility.HtmlDecode(path);
		// path = WebUtility.UrlDecode(path);
		path = HttpUtility.UrlDecode(path);

		// path = FileSystem.SanitizeFilename(path);

		return path;
	}
}