﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo

namespace Kantan.Net.Utilities;

public static class UriUtilities
{
	/*
	 * Adapted from
	 * https://stackoverflow.com/questions/1188096/truncating-query-string-returning-clean-url-c-sharp-asp-net
	 * https://stackoverflow.com/questions/1222610/check-if-2-urls-are-equal
	 */


	public static Uri Normalize(this Uri u) => new(NormalizeUrl(u));

	/// <summary>
	/// Determines whether <paramref name="url1"/> is equal to <paramref name="url2"/>
	/// when both are normalized.
	/// </summary>
	public static bool UrlEqual(string url1, string url2)
	{
		url1 = NormalizeUrl(url1);
		url2 = NormalizeUrl(url2);
		return url1.Equals(url2);
	}

	/// <summary>
	/// Compares <paramref name="url1"/> to <paramref name="url2"/>
	/// when both are normalized.
	/// </summary>
	public static int UrlCompare(string url1, string url2)
	{
		url1 = NormalizeUrl(url1);
		url2 = NormalizeUrl(url2);
		return String.Compare(url1, url2, StringComparison.Ordinal);
	}

	/// <summary>
	/// Compares <paramref name="url1"/> to <paramref name="url2"/>
	/// when both are normalized.
	/// </summary>
	public static int UrlCompare(Uri url1, Uri url2)
	{
		return UrlCompare(url1.ToString(), url2.ToString());
	}


	/// <summary>
	/// Determines whether <paramref name="uri1"/> is equal to <paramref name="uri2"/>
	/// when both are normalized.
	/// </summary>
	public static bool UrlEqual(Uri uri1, Uri uri2)
	{
		var url1 = NormalizeUrl(uri1);
		var url2 = NormalizeUrl(uri2);
		return url1.Equals(url2);
	}

	public static readonly List<string> DefaultDirectoryIndexes =new()
	{
		"default.asp",
		"default.aspx",
		"index.htm",
		"index.html",
		"index.php"
	};

	public static string RemoveUrlQueries(string url)
	{
		var uri = new Uri(url);
		return uri.GetLeftPart(UriPartial.Path);
	}

	public static string NormalizeUrl(Uri uri)
	{
		var url = UrlToLower(uri);
		url = LimitProtocols(url);
		url = RemoveDefaultDirectoryIndexes(url);
		url = RemoveFragment(url);
		url = RemoveDuplicateSlashes(url);
		url = AddWWW(url);
		url = RemoveFeedburnerPart(url);
		url = RemoveUrlQueries(url);
		url = RemoveTrailingSlashAndEmptyQuery(url);
		return url;
	}

	public static string NormalizeUrl(string url) => NormalizeUrl(new Uri(url));

	public static string RemoveFeedburnerPart(string url)
	{
		var idx = url.IndexOf("utm_source=", StringComparison.Ordinal);
		return idx == -1 ? url : url[..(idx - 1)];
	}

	public static string AddWWW(string url)
	{
		if (new Uri(url).Host.Split('.').Length == 2 && !url.Contains("://www.")) {
			return url.Replace("://", "://www.");
		}

		return url;
	}

	public static string RemoveDuplicateSlashes(string url)
	{
		var path = new Uri(url).AbsolutePath;
		return path.Contains("//") ? url.Replace(path, path.Replace("//", "/")) : url;
	}

	public static string LimitProtocols(string url)
	{
		return new Uri(url).Scheme == Uri.UriSchemeHttps ? url.Replace("https://", "http://") : url;
	}

	public static string RemoveFragment(string url)
	{
		var fragment = new Uri(url).Fragment;
		return String.IsNullOrWhiteSpace(fragment) ? url : url.Replace(fragment, String.Empty);
	}

	public static string UrlToLower(Uri uri) => HttpUtility.UrlDecode(uri.AbsoluteUri.ToLowerInvariant());

	public static string RemoveTrailingSlashAndEmptyQuery(string url)
	{
		return url.TrimEnd(new[] { '?' }).TrimEnd(new[] { '/' });
	}

	public static string RemoveDefaultDirectoryIndexes(string url)
	{
		foreach (var index in DefaultDirectoryIndexes) {
			if (url.EndsWith(index)) {
				url = url.TrimEnd(index.ToCharArray());
				break;
			}
		}

		return url;
	}

	public static string NormalizeFilename(Uri src)
	{
		string filename = Path.GetFileName(src.AbsolutePath);

		if (!Path.HasExtension(filename)) {

			// If no format is specified/found, just append a jpg extension
			string ext = ".jpg";

			// For Pixiv (?)
			var kv = HttpUtility.ParseQueryString(src.Query);

			var t = kv["format"];

			if (t != null) {
				ext = $".{t}";
			}

			filename += ext;

		}

		// Stupid URI parameter Twitter appends to filenames

		var i = filename.IndexOf(":large", StringComparison.Ordinal);

		if (i != -1) {
			filename = filename[..i];
		}

		return filename;
	}

	public static Uri GetHostUri(this Uri u)
	{
		return new UriBuilder(u.Host).Uri;
	}

	public static string GetHostComponent(this Uri u)
	{
		return u.GetComponents(UriComponents.NormalizedHost, UriFormat.Unescaped);
	}

	public static string StripScheme(this Uri uri)
	{
		return uri.Host + uri.PathAndQuery + uri.Fragment;
	}

	public static bool IsUri(string uriName, out Uri uriResult)
	{
		bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
		              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

		return result;
	}
}