// Read Stanton SmartImage.Lib NodeHelper.cs
// 2023-01-13 @ 11:37 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using AngleSharp.Dom;
using JetBrains.Annotations;

// ReSharper disable AnnotateNotNullParameter

namespace Kantan.Net.Utilities;

public static class NodeHelper
{

	public static JsonNode TryGetKeyValue(this JsonObject v, string k)
		=> v.ContainsKey(k) ? v[k] : null;

	[CBN]
	public static INode FirstOrDefaultElement(this INodeList nodes, Func<IElement, bool> predicate)
		=> ApplyFunctorInnerPredicate<IElement, INode>(nodes.FirstOrDefault, predicate);

	/*[CBN]
	internal static INode ElemFunctor(Func<Func<INode, bool>, INode> functor, Predicate<IElement> elemPredicate)
		=> ApplyFunctorInnerPredicate(functor, elemPredicate);*/


	[CBN]
	public static INode FirstOrDefaultElementByClassName(this INodeList nodes, string className)
		=> ApplyFunctorInnerPredicate<IElement, INode>(nodes.FirstOrDefault, e => e.ClassName == className);

	[CBN]
	[LinqTunnel]
	public static T2 ApplyFunctorInnerPredicate<T, T2>(Func<Func<T2, bool>, T2> functor,
	                                                     Func<T, bool> predicate)
		=> functor(f => f is T e && predicate(e));


	/*
	 * IEnumerable<T>	bound
	 *
	 */


	// Why am I recreating LINQ


	[return: NN]
	public static IEnumerable<T> TryFindElementsByClassName<T>(Func<Func<T, bool>, IEnumerable<T>> where,
	                                                             Func<T, bool> predicate)
		=> where(predicate);


	public static IEnumerable<string> QueryAllAttribute(this IParentNode doc, string sel, string attr)
		=> doc.QuerySelectorAll(sel)
			.Select(e => e.GetAttribute(attr));

	public static INode RecurseChildren(this INode node, int childNodeIndex, int level)
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

}