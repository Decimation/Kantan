using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Text.Json.Nodes;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace Kantan.Net.Utilities;

public static class DomUtilities
{
	[CBN]
	public static string GetExclusiveText(this INode node)
	{
		return node.ChildNodes.OfType<IText>().Select(m => m.Text).FirstOrDefault();
	}

	public static IEnumerable<string> QuerySelectorAttributes(this IHtmlDocument document, string s, string a)
	{
		return document.QuerySelectorAll(s).Select(element => element.GetAttribute(a));
	}

	[CBN]
	public static string TryGetAttribute(this INode n, string s)
	{
		// return n.GetAttribute(s);
		// return n is IHtmlElement he ? he.GetAttribute(s) : null;
		if (n is IHtmlElement { } e) {
			return e.GetAttribute(s);
		}

		return null;
	}

}