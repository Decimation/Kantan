using System.Diagnostics;

// ReSharper disable InconsistentNaming

namespace Kantan.Native.Structures
{
	[DebuggerDisplay("{dwMousePosition.X}, {dwMousePosition.Y}")]
	internal struct MouseEventRecord
	{
		public Coord           dwMousePosition;
		public ButtonState     dwButtonState;
		public ControlKeyState dwControlKeyState;
		public EventFlags      dwEventFlags;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{nameof(dwMousePosition)}: {dwMousePosition}, \n" +
			       $"{nameof(dwButtonState)}: {dwButtonState}, \n" +
			       $"{nameof(dwControlKeyState)}: {dwControlKeyState}, \n" +
			       $"{nameof(dwEventFlags)}: {dwEventFlags}";
		}
	}
}