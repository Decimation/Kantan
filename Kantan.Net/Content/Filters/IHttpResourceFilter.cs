#region

using System.Collections.Generic;
using System.Diagnostics;
using AngleSharp.Html.Dom;
using Kantan.Net.Media;

#endregion

namespace Kantan.Net.Content.Filters;

public interface IHttpResourceFilter
{
	public int? MinimumSize { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public string DiscreteType { get; }

	[VP(nameof(DiscreteMediaTypes))]
	public List<string> TypeBlacklist { get; }

	public bool UrlFilter(string s);

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