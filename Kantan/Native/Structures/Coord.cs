using System.Diagnostics;

// ReSharper disable InconsistentNaming

namespace Kantan.Native.Structures
{
	[DebuggerDisplay("{X}, {Y}")]
	internal struct Coord
	{
		public ushort X;
		public ushort Y;

		

		public Coord(ushort x, ushort y)
		{
			X = x;
			Y = y;
		}
	}
}