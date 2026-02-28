using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Text.Json.Nodes;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Kantan.Utilities;

namespace Kantan.Net.Utilities;

public static class DomUtilities
{

	extension(INode node)
	{

		public INode RecurseChildren(int childNodeIndex, int level)
		{
			/*if (level <= 0) {
			return n;
		}

		return RecurseChildren(n.ChildNodes[childNodeIndex], childNodeIndex, --level);*/
			// return level <= 0 ? n : RecurseChildren(n.ChildNodes[childNodeIndex], childNodeIndex, --level);

			while (true) {
				if (level <= 0)
					return node;

				node  = node.ChildNodes[childNodeIndex];
				level = --level;
			}
		}

		[CBN]
		public string GetExclusiveText()
		{
			return node.ChildNodes.OfType<IText>().Select(m => m.Text).FirstOrDefault();
		}

		[CBN]
		public string TryGetAttribute(string s)
		{
			// return n.GetAttribute(s);
			// return n is IHtmlElement he ? he.GetAttribute(s) : null;
			if (node is IHtmlElement { } e) {
				return e.GetAttribute(s);
			}

			return null;
		}

	}

	extension(INodeList nodeList)
	{

		[CBN]
		public INode FirstOrDefaultElement(Func<IElement, bool> predicate)
			=> FunctionHelper.ApplyFunctorInnerPredicate<IElement, INode>(nodeList.FirstOrDefault, predicate);

		[CBN]
		public INode FirstOrDefaultElementByClassName(string className)
			=> FunctionHelper.ApplyFunctorInnerPredicate<IElement, INode>(nodeList.FirstOrDefault, e => e.ClassName == className);

	}

	public static IEnumerable<string> QuerySelectorAttributes(this IHtmlDocument document, string s, string a)
		=> document.QuerySelectorAll(s)
			.Select(element => element.GetAttribute(a));

	public static IEnumerable<string> QueryAllAttribute(this IParentNode doc, string sel, string attr)
		=> doc.QuerySelectorAll(sel)
			.Select(e => e.GetAttribute(attr));

	public static JsonNode TryGetKeyValue(this JsonObject v, string k)
		=> v.ContainsKey(k) ? v[k] : null;

}