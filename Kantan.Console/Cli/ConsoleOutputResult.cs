#nullable enable
using Kantan.Text;

namespace Kantan.Console.Cli;

[Obsolete]
public sealed class ConsoleOutputResult
{
	public HashSet<object> Output { get; internal set; }

	public bool SelectMultiple { get; internal set; }

	public string? DragAndDrop { get; internal set; }

	public ConsoleKeyInfo? Key { get; internal set; }

	public bool Cancelled { get; internal set; }

	public ConsoleOutputResult()
	{
		Output = new HashSet<object>();
		Status = new ConsoleInputStatus();
	}

	public override string ToString()
	{
		return $"{Key} | {SelectMultiple} | {DragAndDrop} | {Output.QuickJoin()}";
	}

	internal ConsoleInputStatus Status { get; set; }
}

internal sealed class ConsoleInputStatus
{
	internal bool SkipNext { get; set; }
}