using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo

namespace Kantan.Net
{
	public static class UriUtilities
	{
		/*
		 * Adapted from
		 * https://stackoverflow.com/questions/1188096/truncating-query-string-returning-clean-url-c-sharp-asp-net
		 * https://stackoverflow.com/questions/1222610/check-if-2-urls-are-equal
		 */

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

		public static string RemoveUrlQueries(string url)
		{
			Uri uri = new Uri(url);
			return uri.GetLeftPart(UriPartial.Path);
		}

		public static string[] DefaultDirectoryIndexes =
		{
			"default.asp",
			"default.aspx",
			"index.htm",
			"index.html",
			"index.php"
		};

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

		public static Uri GetHostUri(Uri u)
		{
			return new UriBuilder(u.Host).Uri;
		}

		public static string GetHostComponent(Uri u)
		{
			return u.GetComponents(UriComponents.NormalizedHost, UriFormat.Unescaped);
		}

		public static string StripScheme(Uri uri)
		{
			return uri.Host + uri.PathAndQuery + uri.Fragment;
		}

		public static bool IsUri(string uriName, out Uri uriResult)
		{
			bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
			              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

			// if (!result) {
			// 	var b = new UriBuilder(StripScheme(new Uri(uriName)))
			// 	{
			// 		Scheme = Uri.UriSchemeHttps, 
			// 		Port = -1
			// 	};
			// 	uriResult = b.Uri;
			//
			// 	//uriResult = new Uri(uriResult.ToString().Replace("file:", "http:"));
			// }

			return result;
		}
	}
}