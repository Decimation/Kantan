using System.Runtime.InteropServices;

namespace Kantan.OS.Structures;

/// <summary>
/// Reports menu events in a console input record.
/// Use of this event type is not documented.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct MenuEvent
{
	[FieldOffset(0)]
	public int dwCommandId;
}