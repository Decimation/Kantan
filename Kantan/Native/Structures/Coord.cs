using System.Diagnostics;

// ReSharper disable InconsistentNaming

namespace Kantan.Native.Structures;

[DebuggerDisplay("{X}, {Y}")]
public struct Coord
{
	public ushort X;
	public ushort Y;

		

	public Coord(ushort x, ushort y)
	{
		X = x;
		Y = y;
	}

	public Coord(short x, short y):this((ushort)x,(ushort)y)
	{
	}
}