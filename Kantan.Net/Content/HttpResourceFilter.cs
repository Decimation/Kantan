#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Flurl.Http;
using JetBrains.Annotations;
using Kantan.Net.Utilities;

#endregion

namespace Kantan.Net.Content;

public class HttpResourceFilter
{
	public int? MinimumSize { get; init; } = -1;

	[VP(nameof(FileType))]
	public string DiscreteType { get; init; }

	[VP(nameof(FileType))]
	public List<string> TypeBlacklist { get; init; } = new();

	public Dictionary<string[], (string, bool)> DomainFilterMap { get; init; } = new();

	public Dictionary<string, string> SelectorAttributeMap { get; init; } = new();

	public static readonly HttpResourceFilter Media = new()
	{
		SelectorAttributeMap = new Dictionary<string, string>()
		{
			["img"] = "src",
			["a"]   = "href"
		},
		MinimumSize = 50_000,
		TypeBlacklist = new List<string>()
		{
			"image/svg+xml"
		},
		DomainFilterMap = new()
		{
			[new string[] { "www.deviantart.com" }]              = ("images-wixmp", true),
			[new[] { "twitter.com", "pbs.twimg.com" }]           = ("profile_banners", false),
			[new[] { "https://pbs.twimg.com/profile_banners/" }] = (null, false),
			[new[] { ".svg" }]                                   = (null, false)
		},
		DiscreteType = FileType.MT_IMAGE
	};

	public static HttpResourceFilter Default { get; internal set; } = Media;

	protected bool Filter(string s)
	{
		foreach (var (k, v) in DomainFilterMap) {
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

	protected List<string> GetAttributeValues(IHtmlDocument document)
	{
		return SelectorAttributeMap.SelectMany(k => document.QuerySelectorAttributes(k.Key, k.Value))
		               .ToList();
	}

	protected List<string> RefineUrls(List<string> urls)
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

	public async Task<List<string>> ExtractUrls(string s)
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
		urls = GetAttributeValues(doc);

		urls = RefineUrls(urls)
		       .Where(x => x != null)
		       // .DistinctBy(x => UriUtilities.GetHostUri(new Uri(x)))
		       .Distinct()
		       .ToList();

		return urls;
	}


	public async Task<HttpResource[]> ScanAsync(string url)
	{
		var urls = await ExtractUrls(url);

		var hr = await Task.WhenAll(urls.Select(async Task<HttpResource>(s1) =>
		{
			var rsrc = await HttpResource.GetAsync(s1);
			rsrc?.Resolve();

			return rsrc;
		}));

		hr = hr.Where(x => x is { IsBinaryType: true }
			       /*&& x.Stream.Length >= filter.MinimumSize
			       && filter.TypeBlacklist.Contains(x.ComputedType)*/)
		       .ToArray();

		return hr;
	}
}