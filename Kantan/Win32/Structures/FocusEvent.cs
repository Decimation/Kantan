using System.Runtime.InteropServices;

namespace Kantan.Win32.Structures;

/// <summary>
/// Reports focus events in a console input record.
/// Use of this event type is not documented.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct FocusEvent
{
	[FieldOffset(0)]
	public uint bSetFocus;
}