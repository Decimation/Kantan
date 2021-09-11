using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

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

		public static bool IsKeyDownEvent(InputRecord ir)
		{
			var keyEvent = ir.KeyEvent;
			return ir.EventType == ConsoleEventType.KEY_EVENT && keyEvent.bKeyDown != BOOL.FALSE;
		}

		public static bool IsMouseScroll(InputRecord ir)
		{
			var mouseEvent = ir.MouseEvent;

			var mouseWheel = mouseEvent.dwEventFlags is EventFlags.MOUSE_WHEELED
				                 or EventFlags.MOUSE_HWHEELED;

			return ir.EventType == ConsoleEventType.MOUSE_EVENT &&
			       mouseEvent.dwEventFlags != EventFlags.MOUSE_MOVED && mouseWheel;
		}

		public static bool IsMouseEvent(InputRecord ir)
		{
			var mouseEvent = ir.MouseEvent;

			return ir.EventType == ConsoleEventType.MOUSE_EVENT &&
			       mouseEvent.dwEventFlags != EventFlags.MOUSE_MOVED || IsMouseScroll(ir);
		}

		public static bool IsModKey(InputRecord ir)
		{
			// We should also skip over Shift, Control, and Alt, as well as caps lock.
			// Apparently we don't need to check for 0xA0 through 0xA5, which are keys like
			// Left Control & Right Control. See the ConsoleKey enum for these values.
			var keyCode = (VirtualKey) ir.KeyEvent.wVirtualKeyCode;

			return keyCode is >= VirtualKey.SHIFT and <= VirtualKey.MENU or VirtualKey.CAPITAL
				       or VirtualKey.NUMLOCK or VirtualKey.SCROLL;
		}

		public static bool IsAltKeyDown(InputRecord ir)
		{
			// For tracking Alt+NumPad unicode key sequence. When you press Alt key down
			// and press a numpad unicode decimal sequence and then release Alt key, the
			// desired effect is to translate the sequence into one Unicode KeyPress.
			// We need to keep track of the Alt+NumPad sequence and surface the final
			// unicode char alone when the Alt key is released.

			return (ir.KeyEvent.dwControlKeyState &
			        (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
		}
	}

	internal enum ConsoleEventType : short
	{
		FOCUS_EVENT              = 0x0010,
		KEY_EVENT                = 0x0001,
		MENU_EVENT               = 0x0008,
		MOUSE_EVENT              = 0x0002,
		WINDOW_BUFFER_SIZE_EVENT = 0x0004
	}


	internal enum EventFlags
	{
		DOUBLE_CLICK   = 0x0002,
		MOUSE_HWHEELED = 0x0008,
		MOUSE_MOVED    = 0x0001,
		MOUSE_WHEELED  = 0x0004
	}

	internal enum ButtonState
	{
		FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001,
		FROM_LEFT_2ND_BUTTON_PRESSED = 0x0004,
		FROM_LEFT_3RD_BUTTON_PRESSED = 0x0008,
		FROM_LEFT_4TH_BUTTON_PRESSED = 0x0010,
		RIGHTMOST_BUTTON_PRESSED     = 0x0002,

		/// <summary>
		///     For mouse wheel events, if this flag is set, the wheel was scrolled down.
		///     If cleared, the wheel was scrolled up.
		///     This is not officially documented.
		/// </summary>
		SCROLL_DOWN = unchecked((int)0xFF000000)
	}


	[Flags]
	internal enum ControlKeyState
	{
		RightAltPressed  = 0x0001,
		LeftAltPressed   = 0x0002,
		RightCtrlPressed = 0x0004,
		LeftCtrlPressed  = 0x0008,
		ShiftPressed     = 0x0010,
		NumLockOn        = 0x0020,
		ScrollLockOn     = 0x0040,
		CapsLockOn       = 0x0080,
		EnhancedKey      = 0x0100
	}
}