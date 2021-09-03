using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Kantan.Native.Structures
{
	[DebuggerDisplay("KeyCode: {wVirtualKeyCode}")]
	[StructLayout(LayoutKind.Explicit)]
	internal struct KeyEventRecord
	{
		[FieldOffset(0)]
		public BOOL bKeyDown;

		[FieldOffset(4)]
		public ushort wRepeatCount;

		[FieldOffset(6)]
		public ushort wVirtualKeyCode;

		[FieldOffset(8)]
		public ushort wVirtualScanCode;

		[FieldOffset(10)]
		public char UnicodeChar;

		[FieldOffset(10)]
		public byte AsciiChar;

		[FieldOffset(12)]
		public ControlKeyState dwControlKeyState;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{nameof(bKeyDown)}: {bKeyDown}, \n" +
			       $"{nameof(wRepeatCount)}: {wRepeatCount}, \n" +
			       $"{nameof(wVirtualKeyCode)}: {wVirtualKeyCode}, \n" +
			       $"{nameof(wVirtualScanCode)}: {wVirtualScanCode}, \n" +
			       $"{nameof(UnicodeChar)}: {UnicodeChar}, \n" +
			       $"{nameof(AsciiChar)}: {AsciiChar}, \n" +
			       $"{nameof(dwControlKeyState)}: {dwControlKeyState}";
		}
	};
}