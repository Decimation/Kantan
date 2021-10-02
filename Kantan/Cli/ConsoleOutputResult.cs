#nullable enable
using System;
using System.Collections.Generic;
using Kantan.Text;

namespace Kantan.Cli
{
	public sealed class ConsoleOutputResult
	{
		public HashSet<object> Output { get; internal set; }

		public bool SelectMultiple { get; internal set; }

		public string? DragAndDrop { get; internal set; }

		public ConsoleKeyInfo? Key { get; internal set; }

		public ConsoleOutputResult()
		{
			Output = new HashSet<object>();
		}

		public override string ToString()
		{
			return $"{Key} | {SelectMultiple} | {DragAndDrop} | {Output.QuickJoin()}";
		}
	}
}