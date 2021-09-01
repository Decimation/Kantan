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
		
		[DllImport("kernel32.dll", EntryPoint = "PeekConsoleInputW", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool PeekConsoleInput(
			IntPtr hConsoleInput,
			out INPUT_RECORD lpBuffer,
			uint nLength,
			out uint lpNumberOfEventsRead);
		

		internal static bool Peek(out INPUT_RECORD r)
		{


			var b = !(PeekConsoleInput(_stdHandle,  out r, 1, out var recordLen));

			if (recordLen == 0) {
				return false;
			}
			

			//Console.SetCursorPosition(0, 0);
			
			return b;

		}
		internal static INPUT_RECORD Read()
		{
			var  record    = new INPUT_RECORD();
			uint recordLen = 0;

			if (!(ReadConsoleInput(_stdHandle, ref record, 1, ref recordLen))) {
				throw new Win32Exception();
			}

			//Console.SetCursorPosition(0, 0);

			switch (record.EventType) {
				case MOUSE_EVENT:
				{
					goto ret;
				}
			}

			ret:
			WriteConsoleInput(_stdHandle, new[] { record }, 1, out recordLen);
			
			return record;

		}

		internal static void Close()
		{
			SetConsoleMode(_stdHandle, _oldMode);
			_stdHandle = IntPtr.Zero;

		}

		internal static void Init()
		{
			_stdHandle = GetStdHandle(STD_INPUT_HANDLE);
			int mode = 0;

			if (!(GetConsoleMode(_stdHandle, ref mode))) {
				throw new Win32Exception();
			}

			_oldMode = mode;

			mode |= ENABLE_MOUSE_INPUT;
			mode &= ~ENABLE_QUICK_EDIT_MODE;
			mode |= ENABLE_EXTENDED_FLAGS;

			if (!(SetConsoleMode(_stdHandle, mode))) {
				throw new Win32Exception();
			}
		}

		internal const int STD_INPUT_HANDLE = -10;

		internal const int ENABLE_MOUSE_INPUT     = 0x0010;
		internal const int ENABLE_QUICK_EDIT_MODE = 0x0040;
		internal const int ENABLE_EXTENDED_FLAGS  = 0x0080;

		internal const int KEY_EVENT   = 1;
		internal const int MOUSE_EVENT = 2;

		private const string KERNEL32_DLL = "kernel32.dll";

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, ref int lpMode);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode)]
		private static extern bool WriteConsoleInput(IntPtr hConsoleInput, INPUT_RECORD[] lpBuffer, uint nLength,
		                                             out uint lpNumberOfEventsWritten);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ReadConsoleInput(IntPtr hConsoleInput, ref INPUT_RECORD lpBuffer,
		                                             uint nLength, ref uint lpNumberOfEventsRead);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);
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
		public short EventType;

		[FieldOffset(4)]
		public KEY_EVENT_RECORD KeyEvent;

		[FieldOffset(4)]
		public MOUSE_EVENT_RECORD MouseEvent;
	}

	[DebuggerDisplay("KeyCode: {wVirtualKeyCode}")]
	[StructLayout(LayoutKind.Explicit)]
	internal struct KEY_EVENT_RECORD
	{
		[FieldOffset(0)]
		[MarshalAs(UnmanagedType.Bool)]
		public bool bKeyDown;

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
	};

	[DebuggerDisplay("{dwMousePosition.X}, {dwMousePosition.Y}")]
	internal struct MOUSE_EVENT_RECORD
	{
		public COORD dwMousePosition;
		public int   dwButtonState;
		public int   dwControlKeyState;
		public int   dwEventFlags;
	}
}