using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Kantan.Native.Structures
{
	[DebuggerDisplay("EventType: {EventType}")]
	[StructLayout(LayoutKind.Explicit)]
	internal struct InputRecord
	{
		[FieldOffset(0)]
		public ConsoleEventType EventType;

		[FieldOffset(4)]
		public KeyEventRecord KeyEvent;

		[FieldOffset(4)]
		public MouseEventRecord MouseEvent;

		/// <summary>
		/// Window buffer size information if this is a window buffer size event.
		/// </summary>
		[FieldOffset(4)]
		public WindowBufferSizeEvent WindowBufferSizeEvent;

		/// <summary>
		/// Menu event information if this is a menu event.
		/// </summary>
		[FieldOffset(4)]
		public MenuEvent MenuEvent;

		/// <summary>
		/// Focus event information if this is a focus event.
		/// </summary>
		[FieldOffset(4)]
		public FocusEvent FocusEvent;

		public override string ToString()
		{
			return $"{nameof(EventType)}: {EventType}";
		}

		public bool IsKeyDownEvent()
		{
			return EventType == ConsoleEventType.KEY_EVENT && KeyEvent.bKeyDown != BOOL.FALSE;
		}

		public bool IsMouseScrollEvent()
		{
			return !IsMouseMoveEvent() && MouseEvent.dwEventFlags is MouseEventFlags.MOUSE_WHEELED
				       or MouseEventFlags.MOUSE_HWHEELED;
		}

		public bool IsMouseButtonEvent()
		{
			return !IsMouseMoveEvent() || IsMouseScrollEvent();
		}

		public bool IsMouseMoveEvent()
		{
			return EventType == ConsoleEventType.MOUSE_EVENT &&
			       MouseEvent.dwEventFlags == MouseEventFlags.MOUSE_MOVED;
		}

		public bool IsModKey()
		{
			// We should also skip over Shift, Control, and Alt, as well as caps lock.
			// Apparently we don't need to check for 0xA0 through 0xA5, which are keys like
			// Left Control & Right Control. See the ConsoleKey enum for these values.
			var keyCode = (VirtualKey) KeyEvent.wVirtualKeyCode;

			return keyCode is >= VirtualKey.SHIFT and <= VirtualKey.MENU or VirtualKey.CAPITAL
				       or VirtualKey.NUMLOCK or VirtualKey.SCROLL;
		}

		public bool IsAltKeyDown()
		{
			// For tracking Alt+NumPad unicode key sequence. When you press Alt key down
			// and press a numpad unicode decimal sequence and then release Alt key, the
			// desired effect is to translate the sequence into one Unicode KeyPress.
			// We need to keep track of the Alt+NumPad sequence and surface the final
			// unicode char alone when the Alt key is released.

			return (KeyEvent.dwControlKeyState &
			        (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
		}
	}
}