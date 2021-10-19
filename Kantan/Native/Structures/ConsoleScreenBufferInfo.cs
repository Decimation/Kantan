using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace Kantan.Native.Structures
{
	/// <summary>
	/// Contains information about a console screen buffer.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct ConsoleScreenBufferInfo
	{
		public Coord dwSize;
		public Coord dwCursorPosition;
		public short wAttributes;

		[MarshalAs(UnmanagedType.Struct)]
		public SmallRect srWindow;

		public Coord dwMaximumWindowSize;
	}
}