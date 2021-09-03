using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global

namespace Kantan.Native
{
	internal static class ConsoleInterop
	{
		private static IntPtr _stdIn;

		private static ConsoleModes _oldMode;

		private static IntPtr _stdOut;

		// Note: some code comes from:
		// .NET BCL
		// http://mischel.com/pubs/consoledotnet/consoledotnet.zip,
		// https://www.medo64.com/2013/05/console-mouse-input-in-c/


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
			return ir.EventType == ConsoleEventType.KEY_EVENT && ir.KeyEvent.bKeyDown != BOOL.FALSE;
		}

		private static bool IsMouseEvent(InputRecord ir)
		{
			return ir.EventType == ConsoleEventType.MOUSE_EVENT &&
			       ir.MouseEvent.dwButtonState == ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED;
		}

		private static bool IsModKey(InputRecord ir)
		{
			// We should also skip over Shift, Control, and Alt, as well as caps lock.
			// Apparently we don't need to check for 0xA0 through 0xA5, which are keys like
			// Left Control & Right Control. See the ConsoleKey enum for these values.
			var keyCode = ir.KeyEvent.wVirtualKeyCode;

			return keyCode is >= 0x10 and <= 0x12 or 0x14 or 0x90 or 0x91;
		}

		public static ConsoleKeyInfo GetKeyInfoFromRecord(InputRecord ir)
		{

			// We did NOT have a previous keystroke with repeated characters:
			while (true) {

				var keyCode = ir.KeyEvent.wVirtualKeyCode;

				// First check for non-keyboard events & discard them. Generally we tap into only KeyDown events and ignore the KeyUp events
				// but it is possible that we are dealing with a Alt+NumPad unicode key sequence, the final unicode char is revealed only when
				// the Alt key is released (i.e when the sequence is complete). To avoid noise, when the Alt key is down, we should eat up
				// any intermediate key strokes (from NumPad) that collectively forms the Unicode character.

				if (!IsKeyDownEvent(ir)) {
					// REVIEW: Unicode IME input comes through as KeyUp event with no accompanying KeyDown.
					if (keyCode != AltVKCode)
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

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool PeekConsoleInput(IntPtr hConsoleInput, out InputRecord lpBuffer,
		                                             uint nLength, out uint lpNumberOfEventsRead);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr GetStdHandle(StandardHandle nStdHandle);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WriteConsoleInput(IntPtr hConsoleInput, InputRecord[] lpBuffer, uint nLength,
		                                              out uint lpNumberOfEventsWritten);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] InputRecord[] lpBuffer,
		                                             uint nLength, out uint lpNumberOfEventsRead);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ReadConsoleOutputCharacter(IntPtr hConsoleOutput,
		                                                       [Out]
		                                                       [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
		                                                       char[] lpCharacter, int nLength, Coord dwReadCoord,
		                                                       ref int lpNumberOfCharsRead);

		private const short AltVKCode = 0x12;

		private const string KERNEL32_DLL = "kernel32.dll";
	}


	internal enum ButtonState
	{
		FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001,
		FROM_LEFT_2ND_BUTTON_PRESSED = 0x0004,
		FROM_LEFT_3RD_BUTTON_PRESSED = 0x0008,
		FROM_LEFT_4TH_BUTTON_PRESSED = 0x0010,
		RIGHTMOST_BUTTON_PRESSED     = 0x0002,
	}


	internal enum StandardHandle : int
	{
		STD_ERROR_HANDLE  = -12,
		STD_INPUT_HANDLE  = -10,
		STD_OUTPUT_HANDLE = -11,
	}

	[Flags]
	internal enum ConsoleModes : uint
	{
		#region Input

		ENABLE_PROCESSED_INPUT = 0x0001,
		ENABLE_LINE_INPUT      = 0x0002,
		ENABLE_ECHO_INPUT      = 0x0004,
		ENABLE_WINDOW_INPUT    = 0x0008,
		ENABLE_MOUSE_INPUT     = 0x0010,
		ENABLE_INSERT_MODE     = 0x0020,
		ENABLE_QUICK_EDIT_MODE = 0x0040,
		ENABLE_EXTENDED_FLAGS  = 0x0080,
		ENABLE_AUTO_POSITION   = 0x0100,

		#endregion

		#region Output

		ENABLE_PROCESSED_OUTPUT            = 0x0001,
		ENABLE_WRAP_AT_EOL_OUTPUT          = 0x0002,
		ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
		DISABLE_NEWLINE_AUTO_RETURN        = 0x0008,
		ENABLE_LVB_GRID_WORLDWIDE          = 0x0010

		#endregion
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

	/// <summary>
	/// Blittable version of Windows BOOL type. It is convenient in situations where
	/// manual marshalling is required, or to avoid overhead of regular bool marshalling.
	/// </summary>
	/// <remarks>
	/// Some Windows APIs return arbitrary integer values although the return type is defined
	/// as BOOL. It is best to never compare BOOL to TRUE. Always use bResult != BOOL.FALSE
	/// or bResult == BOOL.FALSE .
	/// </remarks>
	internal enum BOOL : int
	{
		FALSE = 0,
		TRUE  = 1,
	}

	internal enum ConsoleEventType : short
	{
		FOCUS_EVENT              = 0x0010,
		KEY_EVENT                = 0x0001,
		MENU_EVENT               = 0x0008,
		MOUSE_EVENT              = 0x0002,
		WINDOW_BUFFER_SIZE_EVENT = 0x0004,
	}

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
	}

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


	internal enum EventFlags
	{
		DOUBLE_CLICK   = 0x0002,
		MOUSE_HWHEELED = 0x0008,
		MOUSE_MOVED    = 0x0001,
		MOUSE_WHEELED  = 0x0004,
	}

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