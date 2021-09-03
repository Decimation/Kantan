using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global

namespace Kantan.Native
{
	internal static class NativeInput
	{
		private static IntPtr _stdHandle;

		private static int _oldMode;

		private static bool IsKeyDownEvent(INPUT_RECORD ir)
		{
			return ir.EventType == ConsoleEventType.KEY_EVENT && ir.KeyEvent.bKeyDown != BOOL.FALSE;
		}

		private static bool IsMouseEvent(INPUT_RECORD ir)
		{
			return ir.EventType == ConsoleEventType.MOUSE_EVENT && ir.MouseEvent.dwButtonState == 0x1;
		}

		private static bool IsModKey(INPUT_RECORD ir)
		{
			// We should also skip over Shift, Control, and Alt, as well as caps lock.
			// Apparently we don't need to check for 0xA0 through 0xA5, which are keys like
			// Left Control & Right Control. See the ConsoleKey enum for these values.
			var keyCode = ir.KeyEvent.wVirtualKeyCode;

			return keyCode >= 0x10 && keyCode <= 0x12
			       || keyCode == 0x14 || keyCode == 0x90 || keyCode == 0x91;

		}

		private const short AltVKCode = 0x12;

		public static ConsoleKeyInfo ReadKey(INPUT_RECORD ir)
		{
			int numEventsRead = -1;

			bool r;

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

				char ch = (char) ir.KeyEvent.UnicodeChar;

				// In a Alt+NumPad unicode sequence, when the alt key is released uChar will represent the final unicode character, we need to
				// surface this. VirtualKeyCode for this event will be Alt from the Alt-Up key event. This is probably not the right code,
				// especially when we don't expose ConsoleKey.Alt, so this will end up being the hex value (0x12). VK_PACKET comes very
				// close to being useful and something that we could look into using for this purpose...

				if (ch == 0) {
					// Skip mod keys.
					if (IsModKey(ir))
						continue;
				}

				// When Alt is down, it is possible that we are in the middle of a Alt+NumPad unicode sequence.
				// Escape any intermediate NumPad keys whether NumLock is on or not (notepad behavior)
				ConsoleKey key = (ConsoleKey) keyCode;

				if (IsAltKeyDown(ir) && ((key >= ConsoleKey.NumPad0 && key <= ConsoleKey.NumPad9)
				                         || (key == ConsoleKey.Clear) || (key == ConsoleKey.Insert)
				                         || (key >= ConsoleKey.PageUp && key <= ConsoleKey.DownArrow))) {
					continue;
				}

				if (ir.KeyEvent.wRepeatCount > 1) {
					ir.KeyEvent.wRepeatCount--;
					//_cachedInputRecord = ir;
				}

				break;
			}

			ControlKeyState state = (ControlKeyState) ir.KeyEvent.dwControlKeyState;
			bool shift = (state & ControlKeyState.ShiftPressed) != 0;
			bool alt = (state & (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
			bool control = (state & (ControlKeyState.LeftCtrlPressed | ControlKeyState.RightCtrlPressed)) != 0;

			ConsoleKeyInfo info = new ConsoleKeyInfo((char) ir.KeyEvent.UnicodeChar,
			                                         (ConsoleKey) ir.KeyEvent.wVirtualKeyCode,
			                                         shift, alt, control);
			return info;

		}


		// For tracking Alt+NumPad unicode key sequence. When you press Alt key down
		// and press a numpad unicode decimal sequence and then release Alt key, the
		// desired effect is to translate the sequence into one Unicode KeyPress.
		// We need to keep track of the Alt+NumPad sequence and surface the final
		// unicode char alone when the Alt key is released.
		private static bool IsAltKeyDown(INPUT_RECORD ir)
		{
			return (((ControlKeyState) ir.KeyEvent.dwControlKeyState)
			        & (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
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

		internal static bool KeyAvailable
		{
			get
			{
				/*if (_cachedInputRecord.eventType == Interop.KEY_EVENT)
					return true;*/

				while (true) {
					bool r = PeekConsoleInput(_stdHandle, out INPUT_RECORD ir, 1, out uint numEventsRead);

					if (!r) {
						//
						throw new Win32Exception();
					}

					if (numEventsRead == 0)
						return false;


					// Skip non key-down && mod key events.
					if (!IsMouseEvent(ir) && (!IsKeyDownEvent(ir) || IsModKey(ir))) {
						var rg = new INPUT_RECORD[1];
						r = ReadConsoleInput(_stdHandle, rg, 1, out numEventsRead);

						if (!r)
							throw new Win32Exception();
					}
					else {
						return true;
					}

				}
			} // get
		}

		internal static INPUT_RECORD Read()
		{
			var record = new INPUT_RECORD[1];

			if (!ReadConsoleInput(_stdHandle, record, 1, out uint lpNumberOfEventsRead)) {
				throw new Win32Exception();
			}

			var read = record[0];

			return read;

		}

		internal static void Close()
		{
			SetConsoleMode(_stdHandle, _oldMode);
			_stdHandle = IntPtr.Zero;
			_oldMode   = 0;
		}

		internal static void Init()
		{
			_stdHandle = GetStdHandle(STD_INPUT_HANDLE);
			int mode = 0;

			if (!GetConsoleMode(_stdHandle, ref mode)) {
				throw new Win32Exception();
			}

			_oldMode = mode;

			mode |= ENABLE_MOUSE_INPUT;
			mode &= ~ENABLE_QUICK_EDIT_MODE;
			mode |= ENABLE_EXTENDED_FLAGS;

			if (!SetConsoleMode(_stdHandle, mode)) {
				throw new Win32Exception();
			}
		}

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool PeekConsoleInput(IntPtr hConsoleInput,
		                                             out INPUT_RECORD lpBuffer,
		                                             uint nLength,
		                                             out uint lpNumberOfEventsRead);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, ref int lpMode);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WriteConsoleInput(IntPtr hConsoleInput, INPUT_RECORD[] lpBuffer, uint nLength,
		                                              out uint lpNumberOfEventsWritten);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ReadConsoleInput(IntPtr hConsoleInput,
		                                             [Out] INPUT_RECORD[] lpBuffer,
		                                             uint nLength,
		                                             out uint lpNumberOfEventsRead);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);

		internal const int STD_INPUT_HANDLE = -10;

		internal const int ENABLE_MOUSE_INPUT     = 0x0010;
		internal const int ENABLE_QUICK_EDIT_MODE = 0x0040;
		internal const int ENABLE_EXTENDED_FLAGS  = 0x0080;

		private const string KERNEL32_DLL = "kernel32.dll";
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
		KEY_EVENT   = 1,
		MOUSE_EVENT = 2
	}

	[DebuggerDisplay("{X}, {Y}")]
	internal struct COORD
	{
		public ushort X;
		public ushort Y;
	}

	[DebuggerDisplay("EventType: {EventType}")]
	[StructLayout(LayoutKind.Explicit)]
	internal struct INPUT_RECORD
	{
		[FieldOffset(0)]
		public ConsoleEventType EventType;

		[FieldOffset(4)]
		public KEY_EVENT_RECORD KeyEvent;

		[FieldOffset(4)]
		public MOUSE_EVENT_RECORD MouseEvent;

		/// <inheritdoc />
		public override string ToString()
		{
			var s = EventType == ConsoleEventType.KEY_EVENT ? KeyEvent.ToString() : MouseEvent.ToString();
			return $"{EventType}\n" + s;
		}
	}

	[DebuggerDisplay("KeyCode: {wVirtualKeyCode}")]
	[StructLayout(LayoutKind.Explicit)]
	internal struct KEY_EVENT_RECORD
	{
		[FieldOffset(0)]
		//[MarshalAs(UnmanagedType.Bool)]
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
		public int dwControlKeyState;

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

	[DebuggerDisplay("{dwMousePosition.X}, {dwMousePosition.Y}")]
	internal struct MOUSE_EVENT_RECORD
	{
		public COORD dwMousePosition;
		public int   dwButtonState;
		public int   dwControlKeyState;
		public int   dwEventFlags;

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