using System;
using System.Collections.Generic;
using AngleSharp.Html.Dom;

namespace Kantan.Net;

public interface IMediaResourceFilter
{
	public int? MinimumSize { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public string DiscreteType { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public List<string> TypeBlacklist { get; }

	public Dictionary<string, Func<string, string>> HostUrlFilter { get; }

	public List<string> GetUrls(IHtmlDocument document);
}