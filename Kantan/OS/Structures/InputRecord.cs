using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Kantan.Cli;

// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming

namespace Kantan.OS.Structures;

[DebuggerDisplay("EventType: {EventType}")]
[StructLayout(LayoutKind.Explicit)]
public struct InputRecord
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

	public bool IsKeyDownEvent
	{
		get
		{
			var keyEvent = KeyEvent;
			return EventType == ConsoleEventType.KEY_EVENT && keyEvent.bKeyDown != BOOL.FALSE;
		}
	}

	public bool IsMouseScroll
	{
		get
		{
			var mouseEvent = MouseEvent;

			var mouseWheel = mouseEvent.dwEventFlags is MouseEventFlags.MOUSE_WHEELED
				                 or MouseEventFlags.MOUSE_HWHEELED;

			return EventType == ConsoleEventType.MOUSE_EVENT &&
			       mouseEvent.dwEventFlags != MouseEventFlags.MOUSE_MOVED && mouseWheel;
		}
	}

	public bool IsMouseEvent
	{
		get
		{
			var mouseEvent = MouseEvent;

			return EventType == ConsoleEventType.MOUSE_EVENT &&
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

public enum ConsoleEventType : short
{
	FOCUS_EVENT              = 0x0010,
	KEY_EVENT                = 0x0001,
	MENU_EVENT               = 0x0008,
	MOUSE_EVENT              = 0x0002,
	WINDOW_BUFFER_SIZE_EVENT = 0x0004
}