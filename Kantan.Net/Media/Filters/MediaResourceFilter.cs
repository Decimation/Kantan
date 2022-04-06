using System;
using System.Collections.Generic;
using System.Diagnostics;
using AngleSharp.Html.Dom;

namespace Kantan.Net.Media.Filters;

public abstract class MediaResourceFilter
{
	public abstract int? MinimumSize { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public abstract string DiscreteType { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public abstract List<string> TypeBlacklist { get; }

	public abstract bool UrlFilter(string s);

	public virtual List<string> Refine(List<string> urls)
	{
		for (int i = urls.Count - 1; i >= 0; i--) {

			if (urls[i] is { }) {

				if (UriUtilities.IsUri(urls[i], out var u2)) {

					urls[i] = UriUtilities.NormalizeUrl(u2);
				}
				else {
					urls.RemoveAt(i);
					continue;
				}

				if (!UrlFilter( /*u2*/ urls[i])) {

					Debug.WriteLine($"removing {u2}");
					urls.RemoveAt(i);
				}
			}
		}

		return urls;
	}

	public abstract List<string> GetUrls(IHtmlDocument document);
}