using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Kantan.Win32.Structures;

/// <summary>
/// Contains information for a console selection.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ConsoleSelectionInfo
{
	public int dwFlags;

	public Coord dwSelectionAnchor;

	[MarshalAs(UnmanagedType.Struct)]
	public SmallRect srSelection;
}