using System;
using System.Collections.Generic;
using AngleSharp.Html.Dom;

namespace Kantan.Net.Media.Filters;

public interface IMediaResourceFilter
{
	public int? MinimumSize { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public string DiscreteType { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public List<string> TypeBlacklist { get; }

	public bool UrlFilter(string s);

	public List<string> GetUrls(IHtmlDocument document);
}