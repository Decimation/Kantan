using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using AngleSharp.Common;
using AngleSharp.Html.Dom;
using Kantan.Net.Media;
using Kantan.Net.Properties;
using Kantan.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace Kantan.Net.Content.Filters;

[Obsolete]
public sealed class HttpMediaResourceFilter : HttpResourceFilter
{
	public static readonly HttpMediaResourceFilter Default = new();

	public int? MinimumSize => 50_000;

	public string DiscreteType => HttpType.MT_IMAGE;

	public bool Filter(string url)
	{
		/*var map = ResourceHelper.ReadMap(Resources.KV_Media);

		foreach (var (k, v) in map) {

			if (k.Any(url.Contains)) {
				KantanNetInit.Logger.LogDebug($"{url}");

				return url.Contains(v);
			}
		}
		*/

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

	public List<string> Parse(IHtmlDocument document)
	{
		var rg = document.QuerySelectorAttributes("img", "src")
		                 .Union(document.QuerySelectorAttributes("a", "href"))
		                 .ToList();

		return rg;
	}

	public List<string> TypeBlacklist => new() { "image/svg+xml" };
}