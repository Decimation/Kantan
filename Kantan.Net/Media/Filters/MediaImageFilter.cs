using System.Collections.Generic;
using System.Linq;
using AngleSharp.Html.Dom;
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Kantan.Net.Media.Filters;

public sealed class MediaImageFilter : MediaResourceFilter
{
	public static readonly MediaImageFilter Default = new();

	public override int? MinimumSize => 50_000;

	public override bool UrlFilter(string url)
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

	public override string DiscreteType => DiscreteMediaTypes.Image;

	public override List<string> GetUrls(IHtmlDocument document)
	{
		var rg = document.QuerySelectorAttributes("img", "src")
		                 .Union(document.QuerySelectorAttributes("a", "href"))
		                 .ToList();

		return rg;
	}

	public override List<string> TypeBlacklist => new() { "image/svg+xml" };
}