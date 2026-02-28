using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Flurl;
using Jint.Parser.Ast;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo

namespace Kantan.Net.Utilities;

/// <summary>
/// <see cref="Uri"/>
/// <see cref="Url"/>
/// <seealso cref="UrlUtilities"/>
/// </summary>
public static class UriUtilities
{

	/*
	 * Adapted from
	 * https://stackoverflow.com/questions/1188096/truncating-query-string-returning-clean-url-c-sharp-asp-net
	 * https://stackoverflow.com/questions/1222610/check-if-2-urls-are-equal
	 */

	extension(Uri u)
	{

		public Uri NormalizeToUri() => new(u.Normalize());

		public string Normalize()
		{
			var url = u.ToLower();
			url = LimitProtocols(url);
			url = RemoveDefaultDirectoryIndexes(url);
			url = RemoveFragment(url);
			url = RemoveDuplicateSlashes(url);
			url = AddWWW(url);
			url = RemoveFeedburnerPart(url);
			url = u.RemoveUriQueries();
			url = RemoveTrailingSlashAndEmptyQuery(url);
			return url;
		}


		/// <summary>
		/// Determines whether <paramref name="u"/> is equal to <paramref name="url2"/>
		/// when both are normalized.
		/// </summary>
		public bool UriEqual(Uri url2)
		{
			var uri1 = u.Normalize();
			var uri2 = url2.Normalize();
			return uri1.Equals(uri2);
		}

		/// <summary>
		/// Compares <paramref name="u"/> to <paramref name="url2"/>
		/// when both are normalized.
		/// </summary>
		public int UriCompare(Uri url2)
		{
			var uri1 = u.Normalize();
			var uri2 = url2.Normalize();
			return String.Compare(uri1, uri2, StringComparison.Ordinal);
		}


		public string RemoveUriQueries()
		{
			return u.GetLeftPart(UriPartial.Path);
		}

		public string ToLower() => HttpUtility.UrlDecode(u.AbsoluteUri.ToLowerInvariant());

		public Uri GetHostUri()
		{
			return new UriBuilder(u.Host).Uri;
		}

		public string GetHostComponent() => u.GetComponents(UriComponents.NormalizedHost, UriFormat.Unescaped);

		public string StripScheme() => u.Host + u.PathAndQuery + u.Fragment;

		public Uri AddQuery(string name, string value)
		{
			var collection = HttpUtility.ParseQueryString(u.Query);

			collection.Remove(name);
			collection.Add(name, value);

			var ub = new UriBuilder(u);

			// this code block is taken from httpValueCollection.ToString() method
			// and modified so it encodes strings with HttpUtility.UrlEncode
			if (collection.Count == 0) {
				ub.Query = String.Empty;
			}
			else {
				var sb = new StringBuilder();

				for (int i = 0; i < collection.Count; i++) {
					string text = collection.GetKey(i);

					text = HttpUtility.UrlEncode(text);

					string   val    = (text != null) ? (text + "=") : string.Empty;
					string[] values = collection.GetValues(i);

					if (sb.Length > 0)
						sb.Append('&');

					if (values == null || values.Length == 0)
						sb.Append(val);
					else {
						if (values.Length == 1) {
							sb.Append(val);
							sb.Append(HttpUtility.UrlEncode(values[0]));
						}
						else {
							for (int j = 0; j < values.Length; j++) {
								if (j > 0)
									sb.Append('&');

								sb.Append(val);
								sb.Append(HttpUtility.UrlEncode(values[j]));
							}
						}
					}
				}

				ub.Query = sb.ToString();
			}

			return ub.Uri;
		}

	}

	public static string NormalizeUri(string url) => new Uri(url).Normalize();


	public static readonly List<string> DefaultDirectoryIndexes = new()
	{
		"default.asp",
		"default.aspx",
		"index.htm",
		"index.html",
		"index.php"
	};

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

	public static string NormalizeFilename(this Uri src)
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

	public static bool IsUri(string uriName, out Uri uriResult)
	{
		bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
		              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

		return result;
	}

	public static string ToQueryString(this NameValueCollection nvc)
	{
		var array = (
			            from key in nvc.AllKeys
			            from value in nvc.GetValues(key)
			            select $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(value)}"
		            ).ToArray();
		return '?' + String.Join('&', array);
	}

}