#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Flurl.Http;
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

	public static IHttpResourceFilter Default = new HttpMediaResourceFilter();

	public bool Filter(string s);

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

				if (!Filter( /*u2*/ urls[i])) {

					Debug.WriteLine($"removing {u2}");
					urls.RemoveAt(i);
				}
			}
		}

		return urls;
	}

	public List<string> GetUrls(IHtmlDocument document);

	public async Task<List<string>> ExtractUrls(string s)
	{
		// filter = new MediaImageFilter();

		string       r;
		List<string> urls = null;

		try {
			r = await s.AllowAnyHttpStatus().GetStringAsync();

		}
		catch (Exception e) {
			return Enumerable.Empty<string>() as List<string>;
		}

		urls = GetUrls(new HtmlParser().ParseDocument(r));

		urls = Refine(urls)
		       .Where(x => x != null)
		       // .DistinctBy(x => UriUtilities.GetHostUri(new Uri(x)))
		       .Distinct()
		       .ToList();

		return urls;
	}
}