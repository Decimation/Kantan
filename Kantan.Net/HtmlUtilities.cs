using System.Collections.Generic;
using System.Json;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

// ReSharper disable UnusedMember.Global

namespace Kantan.Net;

public static class HtmlUtilities
{
	public static JsonValue TryGetKeyValue(this JsonValue value, string k)
	{
		return value.ContainsKey(k) ? value[k] : null;
	}

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
		return ((IHtmlElement) n).GetAttribute(s);
	}
}