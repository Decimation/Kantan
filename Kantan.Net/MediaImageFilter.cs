using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Html.Dom;

namespace Kantan.Net;

public sealed class MediaImageFilter : IMediaResourceFilter
{
	public static readonly MediaImageFilter Default = new();

	public int? MinimumSize => 50_000;

	public string DiscreteType => DiscreteMediaTypes.Image;

	public List<string> GetUrls(IHtmlDocument document)
	{
		var rg = document.QuerySelectorAttributes("img", "src")
		                 .Union(document.QuerySelectorAttributes("a", "href"))
		                 .ToList();

		return rg;
	}

	public Dictionary<string, Func<string, string>> HostUrlFilter
		=> new()
		{
			/*string hostComponent = UriUtilities.GetHostComponent(new Uri(url));
	
			switch (hostComponent) {
				case "www.deviantart.com":
					//https://images-wixmp-
					urls = urls.Where(x => x.Contains("images-wixmp"))
					           .ToList();
					break;
				case "twitter.com":
					urls = urls.Where(x => !x.Contains("profile_banners"))
					           .ToList();
					break;
			}*/
		};

	public List<string> TypeBlacklist => new() { "image/svg+xml" };
}