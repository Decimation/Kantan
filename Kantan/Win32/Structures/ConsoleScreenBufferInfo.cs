using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Kantan.Win32.Structures;

/// <summary>
/// Contains information about a console screen buffer.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ConsoleScreenBufferInfo
{
	public Coord dwSize;
	public Coord dwCursorPosition;
	public short wAttributes;

	[MarshalAs(UnmanagedType.Struct)]
	public SmallRect srWindow;

	public Coord dwMaximumWindowSize;
}