using System.Collections.Generic;
using System.Linq;
using AngleSharp.Html.Dom;
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Kantan.Net.Media.Filters;

public sealed class MediaImageFilter : IMediaResourceFilter
{
	public static readonly MediaImageFilter Default = new();

	public int? MinimumSize => 50_000;

	public bool UrlFilter(string url)
	{
		if (url.Contains("www.deviantart.com")) {
			//https://images-wixmp-
			return url.Contains("images-wixmp");
		}

		if (url.Contains("pbs.twimg.com") || url.Contains("twitter.com")) {
			return !url.Contains("profile_banners");
		}

		if (url.StartsWith("https://pbs.twimg.com/profile_banners/")) {
			return false;
		}

		if (url.EndsWith(".svg")) {
			return false;
		}

		return true;
	}

	public string DiscreteType => DiscreteMediaTypes.Image;

	public List<string> GetUrls(IHtmlDocument document)
	{
		var rg = document.QuerySelectorAttributes("img", "src")
		                 .Union(document.QuerySelectorAttributes("a", "href"))
		                 .ToList();

		return rg;
	}

	public List<string> TypeBlacklist => new() { "image/svg+xml" };
}