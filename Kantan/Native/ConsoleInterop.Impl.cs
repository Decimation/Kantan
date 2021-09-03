using System;
using System.ComponentModel;
using Kantan.Native.Structures;
// ReSharper disable IdentifierTypo

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace Kantan.Native
{
	internal static partial class ConsoleInterop
	{
		internal static IntPtr       _stdIn;
		private static  ConsoleModes _oldMode;
		internal static IntPtr       _stdOut;

		/// <summary>
		/// Reads characters from the screen buffer, starting at the given position.
		/// </summary>
		/// <param name="nChars">The number of characters to read.</param>
		/// <param name="x">Column position of the first character to read.</param>
		/// <param name="y">Row position of the first character to read.</param>
		/// <returns>A string containing the characters read from the screen buffer.</returns>
		internal static string ReadXY(int nChars, int x, int y)
		{
			char[] buff      = new char[nChars];
			int    charsRead = 0;

			if (!ReadConsoleOutputCharacter(_stdOut, buff, nChars, new Coord((ushort) x, (ushort) y), ref charsRead)) {
				throw new Win32Exception();
			}

			return new string(buff, 0, charsRead);
		}

		private static bool IsKeyDownEvent(InputRecord ir)
		{
			var keyEvent = ir.KeyEvent;
			return ir.EventType == ConsoleEventType.KEY_EVENT && keyEvent.bKeyDown != BOOL.FALSE;
		}

		private static bool IsMouseEvent(InputRecord ir)
		{
			var mouseEvent = ir.MouseEvent;

			return ir.EventType == ConsoleEventType.MOUSE_EVENT &&
			       mouseEvent.dwEventFlags != EventFlags.MOUSE_MOVED &&
			       mouseEvent.dwEventFlags != EventFlags.MOUSE_WHEELED &&
			       mouseEvent.dwEventFlags != EventFlags.MOUSE_HWHEELED;
		}

		private static bool IsModKey(InputRecord ir)
		{
			// We should also skip over Shift, Control, and Alt, as well as caps lock.
			// Apparently we don't need to check for 0xA0 through 0xA5, which are keys like
			// Left Control & Right Control. See the ConsoleKey enum for these values.
			var keyCode = (VirtualKey) ir.KeyEvent.wVirtualKeyCode;

			return keyCode is >= VirtualKey.SHIFT and <= VirtualKey.MENU or VirtualKey.CAPITAL
				       or VirtualKey.NUMLOCK or VirtualKey.SCROLL;
		}

		public static ConsoleKeyInfo GetKeyInfoFromRecord(InputRecord ir)
		{

			// We did NOT have a previous keystroke with repeated characters:
			while (true) {

				var keyCode = (VirtualKey) ir.KeyEvent.wVirtualKeyCode;

				// First check for non-keyboard events & discard them. Generally we tap into only KeyDown events and ignore the KeyUp events
				// but it is possible that we are dealing with a Alt+NumPad unicode key sequence, the final unicode char is revealed only when
				// the Alt key is released (i.e when the sequence is complete). To avoid noise, when the Alt key is down, we should eat up
				// any intermediate key strokes (from NumPad) that collectively forms the Unicode character.

				if (!IsKeyDownEvent(ir)) {
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
					if (IsModKey(ir)) {
						continue;
					}
				}

				// When Alt is down, it is possible that we are in the middle of a Alt+NumPad unicode sequence.
				// Escape any intermediate NumPad keys whether NumLock is on or not (notepad behavior)
				var key = (ConsoleKey) keyCode;

				if (IsAltKeyDown(ir) && key is >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9
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

			return GetKeyInfo(ir.KeyEvent.UnicodeChar, ir.KeyEvent.wVirtualKeyCode, state);

		}

		internal static ConsoleKeyInfo GetKeyInfo(char c, int k, ControlKeyState state)
		{
			bool shift   = (state & ControlKeyState.ShiftPressed) != 0;
			bool alt     = (state & (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
			bool control = (state & (ControlKeyState.LeftCtrlPressed | ControlKeyState.RightCtrlPressed)) != 0;

			var info = new ConsoleKeyInfo(c, (ConsoleKey) k, shift, alt, control);
			return info;
		}

		private static bool IsAltKeyDown(InputRecord ir)
		{
			// For tracking Alt+NumPad unicode key sequence. When you press Alt key down
			// and press a numpad unicode decimal sequence and then release Alt key, the
			// desired effect is to translate the sequence into one Unicode KeyPress.
			// We need to keep track of the Alt+NumPad sequence and surface the final
			// unicode char alone when the Alt key is released.

			return (ir.KeyEvent.dwControlKeyState &
			        (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
		}

		internal static bool InputAvailable
		{
			get
			{
				while (true) {

					if (!PeekConsoleInput(_stdIn, out InputRecord ir, 1, out uint numEventsRead)) {
						throw new Win32Exception();
					}

					//Debug.WriteLine(ir);

					if (numEventsRead == 0) {
						return false;
					}

					// Skip non key-down && mod key events.
					if (!IsMouseEvent(ir) && (!IsKeyDownEvent(ir) || IsModKey(ir))) {
						var rg = new InputRecord[1];

						if (!ReadConsoleInput(_stdIn, rg, 1, out numEventsRead))
							throw new Win32Exception();
					}
					else {
						return true;
					}

				}
			}
		}

		internal static InputRecord ReadInput()
		{
			var record = new InputRecord[1];

			if (!ReadConsoleInput(_stdIn, record, 1, out uint lpNumberOfEventsRead)) {
				throw new Win32Exception();
			}

			var read = record[0];

			return read;

		}

		internal static void Close()
		{
			SetConsoleMode(_stdIn, _oldMode);
			_stdIn   = IntPtr.Zero;
			_stdOut  = IntPtr.Zero;
			_oldMode = 0;
		}

		internal static void Init()
		{
			_stdOut = GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE);
			_stdIn  = GetStdHandle(StandardHandle.STD_INPUT_HANDLE);

			if (!GetConsoleMode(_stdIn, out ConsoleModes mode)) {
				throw new Win32Exception();
			}

			_oldMode = mode;

			mode |= ConsoleModes.ENABLE_MOUSE_INPUT;
			mode &= ~ConsoleModes.ENABLE_QUICK_EDIT_MODE;
			mode |= ConsoleModes.ENABLE_EXTENDED_FLAGS;

			if (!SetConsoleMode(_stdIn, mode)) {
				throw new Win32Exception();
			}
		}

		internal static void ScrollRelative(int iRows)
		{
			var csbiInfo   = new ConsoleScreenBufferInfo();
			var srctWindow = new SmallRect(0, 0, 0, 0);

			// Get the current screen buffer window position.

			if (!GetConsoleScreenBufferInfo(_stdOut, csbiInfo)) {
				throw new Win32Exception();
			}

			// Check whether the window is too close to the screen buffer top

			if (csbiInfo.srWindow.Top >= iRows) {
				srctWindow.Top    = (short) -iRows; // move top up
				srctWindow.Bottom = (short) -iRows; // move bottom up
				srctWindow.Left   = 0;              // no change
				srctWindow.Right  = 0;              // no change

				if (!SetConsoleWindowInfo(_stdOut, false, srctWindow)) {
					throw new Win32Exception();
				}

			}
		}
	}
}