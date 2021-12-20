using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Kantan.Cli;

// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming

namespace Kantan.Utilities.Structures;

[DebuggerDisplay("EventType: {EventType}")]
[StructLayout(LayoutKind.Explicit)]
public struct InputRecord
{
	private const int OFFSET = 4;

	[FieldOffset(0)]
	public InputEventType EventType;

	[FieldOffset(OFFSET)]
	public KeyEventRecord KeyEvent;

	[FieldOffset(OFFSET)]
	public MouseEventRecord MouseEvent;

	/// <summary>
	/// Window buffer size information if this is a window buffer size event.
	/// </summary>
	[FieldOffset(OFFSET)]
	public WindowBufferSizeEvent WindowBufferSizeEvent;

	/// <summary>
	/// Menu event information if this is a menu event.
	/// </summary>
	[FieldOffset(OFFSET)]
	public MenuEvent MenuEvent;

	/// <summary>
	/// Focus event information if this is a focus event.
	/// </summary>
	[FieldOffset(OFFSET)]
	public FocusEvent FocusEvent;

	public override string ToString()
	{
		return $"{nameof(EventType)}: {EventType}";
	}

	public bool IsKeyDownEvent
	{
		get
		{
			var keyEvent = KeyEvent;
			return EventType == InputEventType.KEY_EVENT && keyEvent.bKeyDown != ConsoleManager.BOOL.FALSE;
		}
	}

	public bool IsMouseScroll
	{
		get
		{
			var mouseEvent = MouseEvent;

			var mouseWheel = mouseEvent.dwEventFlags is MouseEventFlags.MOUSE_WHEELED
				                 or MouseEventFlags.MOUSE_HWHEELED;

			return EventType == InputEventType.MOUSE_EVENT &&
			       mouseEvent.dwEventFlags != MouseEventFlags.MOUSE_MOVED && mouseWheel;
		}
	}

	public bool IsMouseEvent
	{
		get
		{
			var mouseEvent = MouseEvent;

			return EventType == InputEventType.MOUSE_EVENT &&
			       mouseEvent.dwEventFlags != MouseEventFlags.MOUSE_MOVED || IsMouseScroll;
		}
	}

	public bool IsModKey
	{
		get
		{
			// We should also skip over Shift, Control, and Alt, as well as caps lock.
			// Apparently we don't need to check for 0xA0 through 0xA5, which are keys like
			// Left Control & Right Control. See the ConsoleKey enum for these values.
			var keyCode = KeyEvent.wVirtualKeyCode;

			return keyCode is >= VirtualKey.SHIFT and <= VirtualKey.MENU or VirtualKey.CAPITAL
				       or VirtualKey.NUMLOCK or VirtualKey.SCROLL;
		}
	}

	public bool IsAltKeyDown
	{
		get
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

	public ConsoleKeyInfo ToConsoleKeyInfo()
	{
		InputRecord ir = this;

		// We did NOT have a previous keystroke with repeated characters:
		while (true) {

			var keyCode = ir.KeyEvent.wVirtualKeyCode;

			// First check for non-keyboard events & discard them. Generally we tap into only KeyDown events and ignore the KeyUp events
			// but it is possible that we are dealing with a Alt+NumPad unicode key sequence, the final unicode char is revealed only when
			// the Alt key is released (i.e when the sequence is complete). To avoid noise, when the Alt key is down, we should eat up
			// any intermediate key strokes (from NumPad) that collectively forms the Unicode character.

			if (!ir.IsKeyDownEvent) {
				// REVIEW: Unicode IME input comes through as KeyUp event with no accompanying KeyDown.
				if (keyCode != VirtualKey.MENU)
					continue;
			}

			char ch = ir.KeyEvent.UnicodeChar;

			// In a Alt+NumPad unicode sequence, when the alt key is released uChar will represent the final unicode character, we need to
			// surface this. VirtualKeyCode for this event will be Alt from the Alt-Up key event. This is probably not the right code,
			// especially when we don't expose ConsoleKey.Alt, so this will end up being the hex value (0x12). VK_PACKET comes very
			// close to being useful and something that we could look into using for this purpose...

			if (ch == 0) {
				// Skip mod keys.
				if (ir.IsModKey) {
					continue;
				}
			}

			// When Alt is down, it is possible that we are in the middle of a Alt+NumPad unicode sequence.
			// Escape any intermediate NumPad keys whether NumLock is on or not (notepad behavior)
			var key = (ConsoleKey) keyCode;

			if (ir.IsAltKeyDown && key is >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9
				    or ConsoleKey.Clear or ConsoleKey.Insert or >= ConsoleKey.PageUp
				    and <= ConsoleKey.DownArrow) {
				continue;
			}

			if (ir.KeyEvent.wRepeatCount > 1) {
				ir.KeyEvent.wRepeatCount--;
			}

			break;
		}

		var state = ir.KeyEvent.dwControlKeyState;

		return ConsoleManager.GetKeyInfo(ir.KeyEvent.UnicodeChar, (int) ir.KeyEvent.wVirtualKeyCode, state);

	}
}

public enum InputEventType : short
{
	FOCUS_EVENT              = 0x0010,
	KEY_EVENT                = 0x0001,
	MENU_EVENT               = 0x0008,
	MOUSE_EVENT              = 0x0002,
	WINDOW_BUFFER_SIZE_EVENT = 0x0004
}

/// <summary>
/// Reports menu events in a console input record.
/// Use of this event type is not documented.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct MenuEvent
{
	[FieldOffset(0)]
	public int dwCommandId;
}

[DebuggerDisplay("{dwMousePosition.X}, {dwMousePosition.Y}")]
public struct MouseEventRecord
{
	public Coord           dwMousePosition;
	public ButtonState     dwButtonState;
	public ControlKeyState dwControlKeyState;
	public MouseEventFlags dwEventFlags;


	/// <inheritdoc />
	public override string ToString()
	{
		return $"{nameof(dwMousePosition)}: {dwMousePosition}, \n" +
		       $"\t{nameof(dwButtonState)}: {dwButtonState}, \n" +
		       $"\t{nameof(dwControlKeyState)}: {dwControlKeyState}, \n" +
		       $"\t{nameof(dwEventFlags)}: {dwEventFlags}\n";
	}
}

[Flags]
public enum ControlKeyState
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

public enum ButtonState
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

public enum MouseEventFlags
{
	DOUBLE_CLICK   = 0x0002,
	MOUSE_HWHEELED = 0x0008,
	MOUSE_MOVED    = 0x0001,
	MOUSE_WHEELED  = 0x0004,

	/// <summary>
	/// A mouse button was pressed or released
	/// </summary>
	MOUSE_BUTTON = 0,
}

/// <summary>
/// Reports focus events in a console input record.
/// Use of this event type is not documented.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct FocusEvent
{
	[FieldOffset(0)]
	public uint bSetFocus;
}

[DebuggerDisplay("KeyCode: {wVirtualKeyCode}")]
[StructLayout(LayoutKind.Explicit)]
public struct KeyEventRecord
{
	[FieldOffset(0)]
	public ConsoleManager.BOOL bKeyDown;

	[FieldOffset(4)]
	public ushort wRepeatCount;

	[FieldOffset(6)]
	public VirtualKey wVirtualKeyCode;

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

/// <summary>
/// Reports window buffer sizing events in a console input record.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct WindowBufferSizeEvent
{
	[FieldOffset(0)]
	public Coord dwSize;
}