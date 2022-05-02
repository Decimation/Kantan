#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Flurl.Http;
using Kantan.Diagnostics;
using Kantan.Net.Properties;
using Kantan.Utilities;
using Microsoft.Extensions.Logging;

#endregion

namespace Kantan.Net.Content.Filters;

public class HttpResourceFilter
{
	public int? MinimumSize { get; init; } = -1;

	[VP(nameof(HttpType))]
	public string DiscreteType { get; init; }

	[VP(nameof(HttpType))]
	public List<string> TypeBlacklist { get; init; } = new();

	public Dictionary<string[], (string, bool)> FilterMap { get; init; } = new();

	public Dictionary<string, string> ParseMap { get; init; } = new();

	public static readonly HttpResourceFilter Media = new()
	{
		ParseMap = new Dictionary<string, string>()
		{
			["img"] = "src",
			["a"]   = "href"
		},
		MinimumSize = 50_000,
		TypeBlacklist = new List<string>()
		{
			"image/svg+xml"
		},
		FilterMap = new()
		{
			[new string[] { "www.deviantart.com" }]              = ("images-wixmp", true),
			[new[] { "twitter.com", "pbs.twimg.com" }]           = ("profile_banners", false),
			[new[] { "https://pbs.twimg.com/profile_banners/" }] = (null, false),
			[new[] { ".svg" }]                                   = (null, false)
		},
		DiscreteType = HttpType.MT_IMAGE
	};

	public static HttpResourceFilter Default { get; internal set; } = Media;

	public bool Filter(string s)
	{
		foreach (var (k, v) in FilterMap) {
			foreach (string s1 in k) {
				if (s.Contains(s1)) {
					if (v.Item1 == null) {
						return v.Item2;
					}

					var b = s.Contains(v.Item1);
					return v.Item2 ? b : !b;
				}

			}
		}

		return true;
	}

	public List<string> Refine(List<string> urls)
	{

		for (int i = urls.Count - 1; i >= 0; i--) {
			if (urls[i] is null) {
				urls.RemoveAt(i);
				continue;
			}

			if (UriUtilities.IsUri(urls[i], out var u2)) {
				urls[i] = UriUtilities.NormalizeUrl(u2);
			}
			else {
				urls.RemoveAt(i);
				continue;
			}

			if (!Filter(urls[i])) {
				/*KantanNetInit.LoggerFactory.CreateLogger<HttpScanner>()
					  .LogDebug(message: $"removing {u2}");*/

				// Debug.WriteLine($"removing {u2}",LogCategories.C_VERBOSE);
				urls.RemoveAt(i);
			}

		}

		return urls;
	}

	public List<string> Parse(IHtmlDocument document)
	{
		return ParseMap.SelectMany(k => document.QuerySelectorAttributes(k.Key, k.Value))
		               .ToList();
	}

	public async Task<List<string>> Extract(string s)
	{
		// filter = new MediaImageFilter();

		string       r;
		List<string> urls = null;

		try {
			r = await s.WithClient(HttpUtilities.Client)
			           .AllowAnyHttpStatus()
			           .GetStringAsync();
		}
		catch (Exception e) {
			Debug.WriteLine($"{nameof(HttpResourceFilter)}: {e.Message}");
			return Enumerable.Empty<string>() as List<string>;
		}

		var parser = new HtmlParser();
		var doc    = parser.ParseDocument(r);
		urls = Parse(doc);

		urls = Refine(urls)
		       .Where(x => x != null)
		       // .DistinctBy(x => UriUtilities.GetHostUri(new Uri(x)))
		       .Distinct()
		       .ToList();

		return urls;
	}
}