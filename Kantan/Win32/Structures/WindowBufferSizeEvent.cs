using System.Runtime.InteropServices;

namespace Kantan.Win32.Structures;

/// <summary>
/// Reports window buffer sizing events in a console input record.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct WindowBufferSizeEvent
{
	[FieldOffset(0)]
	public Coord dwSize;
}