using System.Collections.Generic;
using System.Json;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace Kantan.Net.Utilities;

public static class DomUtilities
{
	public static string GetExclusiveText(this INode node)
	{
		return node.ChildNodes.OfType<IText>().Select(m => m.Text).FirstOrDefault();
	}

	public static IEnumerable<string> QuerySelectorAttributes(this IHtmlDocument document, string s, string a)
	{
		return document.QuerySelectorAll(s).Select(element => element.GetAttribute(a));
	}

	public static string TryGetAttribute(this INode n, string s)
	{
		// return n.GetAttribute(s);
		// return n is IHtmlElement he ? he.GetAttribute(s) : null;
		return ((IHtmlElement) n).GetAttribute(s);
	}

	public static JsonValue TryGetKeyValue(this JsonValue value, string k)
	{
		return value.ContainsKey(k) ? value[k] : null;
	}
}