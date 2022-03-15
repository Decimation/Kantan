global using MA = System.Runtime.InteropServices.MarshalAsAttribute;
global using UT = System.Runtime.InteropServices.UnmanagedType;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

// ReSharper disable CommentTypo

// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming


namespace Kantan.Cli;

public static partial class ConsoleManager
{
	public static class Win32
	{
		private const string KERNEL32_DLL = "kernel32.dll";

		/*
		 * Note: some code comes from:
		 * Novus
		 * .NET BCL
		 * http://mischel.com/pubs/consoledotnet/consoledotnet.zip,
		 * https://www.medo64.com/2013/05/console-mouse-input-in-c/
		 */
		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern unsafe bool SetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput,
		                                                                [In] ConsoleScreenBufferInfoEx
			                                                                lpConsoleScreenBufferInfo);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern unsafe bool GetConsoleScreenBufferInfoEx(IntPtr hConsoleOutput,
		                                                                /*out*/ [Out, In] ConsoleScreenBufferInfoEx
			                                                                lpConsoleScreenBufferInfo);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput,
		                                                       out ConsoleScreenBufferInfo lpConsoleScreenBufferInfo);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool GetConsoleSelectionInfo(out ConsoleSelectionInfo lpConsoleSelectionInfo);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern int GetConsoleOutputCP();

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool SetConsoleCP(int wCodePageID);

		/*[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MA(UT.Bool)]
		internal static extern bool PeekConsoleInput(IntPtr hConsoleInput, out InputRecord lpBuffer,
		                                             uint nLength, out uint lpNumberOfEventsRead);*/

		[DllImport(KERNEL32_DLL, EntryPoint = "PeekConsoleInputW", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern BOOL PeekConsoleInput(IntPtr hConsoleInput,
		                                             [MA(UT.LPArray), Out] InputRecord[] lpBuffer,
		                                             uint nLength, out uint lpNumberOfEventsRead);
		[DllImport(KERNEL32_DLL, EntryPoint = "PeekConsoleInputW", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern unsafe BOOL PeekConsoleInput(IntPtr hConsoleInput,
		                                                  [Out]  InputRecord* lpBuffer,
		                                                    uint nLength, out uint lpNumberOfEventsRead);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MA(UT.Bool)]
		internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MA(UT.Bool)]
		internal static extern bool GetNumberOfConsoleInputEvents(IntPtr hConsoleHandle, out uint lpMode);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern IntPtr GetStdHandle(StandardHandle nStdHandle);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode)]
		[return: MA(UT.Bool)]
		internal static extern bool WriteConsoleInput(IntPtr hConsoleInput, InputRecord[] lpBuffer, uint nLength,
		                                              out uint lpNumberOfEventsWritten);

		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode)]
		[return: MA(UT.Bool)]
		internal static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] InputRecord[] lpBuffer,
		                                             uint nLength, out uint lpNumberOfEventsRead);
		[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode)]
		[return: MA(UT.Bool)]
		internal static unsafe extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] InputRecord* lpBuffer,
		                                             uint nLength, out uint lpNumberOfEventsRead);
		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MA(UT.Bool)]
		internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MA(UT.Bool)]
		internal static extern bool SetConsoleTextAttribute(IntPtr hConsoleHandle, ConsoleTextAttributes dwMode);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		[return: MA(UT.Bool)]
		internal static extern bool ReadConsoleOutputCharacter(IntPtr hConsoleOutput,
		                                                       [Out] [MA(UT.LPArray, SizeParamIndex = 2)]
		                                                       char[] lpCharacter,
		                                                       int nLength, Coord dwReadCoord,
		                                                       ref int lpNumberOfCharsRead);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool SetConsoleWindowInfo(IntPtr hConsoleOutput, bool bAbsolute,
		                                                 [In] [MA(UT.LPStruct)] SmallRect lpConsoleWindow);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput,
		                                                       [In] [Out] [MA(UT.LPStruct)]
		                                                       ConsoleScreenBufferInfo lpConsoleScreenBufferInfo);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool GetConsoleSelectionInfo(
			[In] [Out] [MA(UT.LPStruct)] ConsoleSelectionInfo lpConsoleSelectionInfo);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool ScrollConsoleScreenBuffer(IntPtr hConsoleOutput,
		                                                      [In] [MA(UT.LPStruct)] SmallRect lpScrollRectangle,
		                                                      [In] [MA(UT.LPStruct)] SmallRect lpClipRectangle,
		                                                      Coord dwDestinationOrigin,
		                                                      ref ConsoleCharInfo lpFill);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool WriteConsoleOutputAttribute(IntPtr hConsoleOutput,
		                                                        [In] [MA(UT.LPArray, SizeParamIndex = 2)]
		                                                        ConsoleCharAttribute[] lpCharacter, int nLength,
		                                                        Coord dwWriteCoord, ref int lpNumberOfCharsWritten);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool WriteConsoleOutput(IntPtr hConsoleOutput,
		                                               [In] [MA(UT.LPArray, SizeParamIndex = 2)]
		                                               ConsoleCharInfo[,] lpBuffer,
		                                               Coord dwBufferSize,
		                                               Coord dwBufferCoord,
		                                               [In, Out] [MA(UT.LPStruct)] SmallRect lpWriteRegion);

		[DllImport(KERNEL32_DLL, SetLastError = true)]
		internal static extern bool WriteConsoleOutputCharacter(IntPtr hConsoleOutput,
		                                                        [In] [MA(UT.LPArray, SizeParamIndex = 2)]
		                                                        char[] lpCharacter, int nLength,
		                                                        Coord dwWriteCoord, ref int lpNumberOfCharsWritten);
	}

	[Flags]
	public enum ConsoleTextAttributes : ushort
	{
		FOREGROUND_BLUE            = 0x0001, // text color contains blue.
		FOREGROUND_GREEN           = 0x0002, // text color contains green.
		FOREGROUND_RED             = 0x0004, // text color contains red.
		FOREGROUND_INTENSITY       = 0x0008, // text color is intensified.
		BACKGROUND_BLUE            = 0x0010, // background color contains blue.
		BACKGROUND_GREEN           = 0x0020, // background color contains green.
		BACKGROUND_RED             = 0x0040, // background color contains red.
		BACKGROUND_INTENSITY       = 0x0080, // background color is intensified.
		COMMON_LVB_LEADING_BYTE    = 0x0100, // Leading Byte of DBCS
		COMMON_LVB_TRAILING_BYTE   = 0x0200, // Trailing Byte of DBCS
		COMMON_LVB_GRID_HORIZONTAL = 0x0400, // DBCS: Grid attribute: top horizontal.
		COMMON_LVB_GRID_LVERTICAL  = 0x0800, // DBCS: Grid attribute: left vertical.
		COMMON_LVB_GRID_RVERTICAL  = 0x1000, // DBCS: Grid attribute: right vertical.
		COMMON_LVB_REVERSE_VIDEO   = 0x4000, // DBCS: Reverse fore/back ground attribute.
		COMMON_LVB_UNDERSCORE      = 0x8000, // DBCS: Underscore.
		COMMON_LVB_SBCSDBCS        = 0x0300, // SBCS or DBCS flag.
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

	internal enum StandardHandle
	{
		STD_ERROR_HANDLE  = -12,
		STD_INPUT_HANDLE  = -10,
		STD_OUTPUT_HANDLE = -11
	}

	/// <summary>
	///     Blittable version of Windows BOOL type. It is convenient in situations where
	///     manual marshalling is required, or to avoid overhead of regular bool marshalling.
	/// </summary>
	/// <remarks>
	///     Some Windows APIs return arbitrary integer values although the return type is defined
	///     as BOOL. It is best to never compare BOOL to TRUE. Always use bResult != BOOL.FALSE
	///     or bResult == BOOL.FALSE .
	/// </remarks>
	public enum BOOL
	{
		FALSE = 0,
		TRUE  = 1
	}

	/// <summary>
	/// Contains information for a console selection.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ConsoleSelectionInfo
	{
		public int dwFlags;

		public Coord dwSelectionAnchor;

		[MA(UT.Struct)]
		public SmallRect srSelection;
	}


	/// <summary>
	/// Specifies a Unicode or ASCII character and its attributes.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct ConsoleCharInfo
	{
		[FieldOffset(0)]
		public char cUnicodeChar;

		[FieldOffset(0)]
		public byte bAsciiChar;

		[FieldOffset(2)]
		public ConsoleCharAttribute attr;

		/// <summary>
		/// Creates a new instance of the ConsoleCharInfo structure.
		/// </summary>
		/// <param name="uChar">The Unicode character.</param>
		/// <param name="attr">Character attributes.</param>
		public ConsoleCharInfo(char uChar, ConsoleCharAttribute attr)
		{
			bAsciiChar   = 0;
			cUnicodeChar = uChar;
			this.attr    = attr;
		}

		/// <summary>
		/// Creates a new instance of the ConsoleCharInfo structure.
		/// </summary>
		/// <param name="aChar">The ASCII character.</param>
		/// <param name="attr">Character attributes.</param>
		public ConsoleCharInfo(byte aChar, ConsoleCharAttribute attr)
		{
			cUnicodeChar = '\x0';
			bAsciiChar   = aChar;
			this.attr    = attr;
		}


		/// <summary>
		/// Gets or sets the foreground color attribute.
		/// </summary>
		public ConsoleColor Foreground
		{
			get => attr.Foreground;
			set => attr.Foreground = value;
		}

		/// <summary>
		/// Gets or sets the background color attribute.
		/// </summary>
		public ConsoleColor Background
		{
			get => attr.Background;
			set => attr.Background = value;
		}
	}

	/// <summary>
	/// Helper class that simplifies working with foreground and background colors.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ConsoleCharAttribute
	{
		public short attr;

		/// <summary>
		/// Creates a new instance of the ConsoleCharAttribute structure
		/// </summary>
		/// <param name="fg">The foreground color.</param>
		/// <param name="bg">The background color.</param>
		public ConsoleCharAttribute(ConsoleColor fg, ConsoleColor bg)
		{
			attr = (short) (((ushort) bg << 4) | (ushort) fg);
		}

		/// <summary>
		/// Creates a new instance of the ConsoleCharAttribute structure.
		/// </summary>
		/// <param name="wAttr">The combined foreground/background attribute.</param>
		public ConsoleCharAttribute(short wAttr)
		{
			attr = wAttr;
		}

		/// <summary>
		/// Gets or sets the foreground color attribute.
		/// </summary>
		public ConsoleColor Foreground
		{
			get => (ConsoleColor) (attr & 0x0f);
			set => attr = (short) ((attr & 0xfff0) | (ushort) value);
		}

		/// <summary>
		/// Gets or sets the background color attribute.
		/// </summary>
		public ConsoleColor Background
		{
			get => (ConsoleColor) ((attr >> 4) & 0x0f);
			set => attr = (short) ((attr & 0xff0f) | ((ushort) value << 4));
		}
	}

	public struct ConsoleScreenBufferInfo
	{
		public Coord     dwSize;
		public Coord     dwCursorPosition;
		public short     wAttributes;
		public SmallRect srWindow;
		public Coord     dwMaximumWindowSize;
	}

	/*[StructLayout(LayoutKind.Sequential)]
	struct COLORREF
	{
	    public byte R;
	    public byte G;
	    public byte B;
	}*/
	[StructLayout(LayoutKind.Sequential)]
	public struct COLORREF
	{
		public uint ColorDWORD;

		public COLORREF(Color color)
		{
			ColorDWORD = color.R + (((uint) color.G) << 8) + (((uint) color.B) << 16);
		}

		public Color GetColor()
		{
			return Color.FromArgb((int) (0x000000FFU & ColorDWORD),
			                      (int) (0x0000FF00U & ColorDWORD) >> 8,
			                      (int) (0x00FF0000U & ColorDWORD) >> 16);
		}

		public void SetColor(Color color)
		{
			ColorDWORD = color.R + (((uint) color.G) << 8) + (((uint) color.B) << 16);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public sealed class ConsoleScreenBufferInfoEx
	{
		public uint           cbSize;
		public Coord          dwSize;
		public Coord          dwCursorPosition;
		public ConsoleTextAttributes wAttributes;
		public SmallRect      srWindow;
		public Coord          dwMaximumWindowSize;

		public ConsoleScreenBufferInfoEx()
		{
			cbSize = (uint) Marshal.SizeOf<ConsoleScreenBufferInfoEx>();
		}

		public ushort wPopupAttributes;

		public bool bFullscreenSupported;

		[MA(UT.ByValArray, SizeConst = 16)]
		public COLORREF[] ColorTable;
	}
}

[DebuggerDisplay("{X}, {Y}")]
public struct Coord
{
	public ushort X;
	public ushort Y;


	public Coord(ushort x, ushort y)
	{
		X = x;
		Y = y;
	}

	public Coord(short x, short y) : this((ushort) x, (ushort) y) { }
}

/// <summary>
/// Defines the coordinates of the upper left and lower right corners of
/// a rectangle.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SmallRect
{
	public short left;
	public short top;
	public short right;
	public short bottom;

	/// <summary>
	/// Creates a new instance of the SmallRect structure.
	/// </summary>
	/// <param name="mLeft">Column position of top left corner.</param>
	/// <param name="mTop">Row position of the top left corner.</param>
	/// <param name="mRight">Column position of the bottom right corner.</param>
	/// <param name="mBottom">Row position of the bottom right corner.</param>
	public SmallRect(short mLeft, short mTop, short mRight, short mBottom)
	{
		left   = mLeft;
		top    = mTop;
		right  = mRight;
		bottom = mBottom;
	}

	/// <summary>
	/// Gets or sets the column position of the top left corner of a rectangle.
	/// </summary>
	public short Left
	{
		get { return left; }
		set { left = value; }
	}

	/// <summary>
	/// Gets or sets the row position of the top left corner of a rectangle.
	/// </summary>
	public short Top
	{
		get { return top; }
		set { top = value; }
	}

	/// <summary>
	/// Gets or sets the column position of the bottom right corner of a rectangle.
	/// </summary>
	public short Right
	{
		get { return right; }
		set { right = value; }
	}

	/// <summary>
	/// Gets or sets the row position of the bottom right corner of a rectangle.
	/// </summary>
	public short Bottom
	{
		get { return bottom; }
		set { bottom = value; }
	}

	/// <summary>
	/// Gets or sets the width of a rectangle.  When setting the width, the
	/// column position of the bottom right corner is adjusted.
	/// </summary>
	public short Width
	{
		get { return (short) (right - left + 1); }
		set { right = (short) (left + value - 1); }
	}

	/// <summary>
	/// Gets or sets the height of a rectangle.  When setting the height, the
	/// row position of the bottom right corner is adjusted.
	/// </summary>
	public short Height
	{
		get { return (short) (bottom - top + 1); }
		set { bottom = (short) (top + value - 1); }
	}
}

public enum VirtualKey : short
{
	/// <summary>
	///     Left mouse button
	/// </summary>
	LBUTTON = 0x01,

	/// <summary>
	///     Right mouse button
	/// </summary>
	RBUTTON = 0x02,

	/// <summary>
	///     Control-break processing
	/// </summary>
	CANCEL = 0x03,

	/// <summary>
	///     Middle mouse button (three-button mouse)
	/// </summary>
	MBUTTON = 0x04,

	/// <summary>
	///     Windows 2000/XP: X1 mouse button
	/// </summary>
	XBUTTON1 = 0x05,

	/// <summary>
	///     Windows 2000/XP: X2 mouse button
	/// </summary>
	XBUTTON2 = 0x06,

	/// <summary>
	///     BACKSPACE key
	/// </summary>
	BACK = 0x08,

	/// <summary>
	///     TAB key
	/// </summary>
	TAB = 0x09,

	/// <summary>
	///     CLEAR key
	/// </summary>
	CLEAR = 0x0C,

	/// <summary>
	///     ENTER key
	/// </summary>
	RETURN = 0x0D,

	/// <summary>
	///     SHIFT key
	/// </summary>
	SHIFT = 0x10,

	/// <summary>
	///     CTRL key
	/// </summary>
	CONTROL = 0x11,

	/// <summary>
	///     ALT key
	/// </summary>
	MENU = 0x12,

	/// <summary>
	///     PAUSE key
	/// </summary>
	PAUSE = 0x13,

	/// <summary>
	///     CAPS LOCK key
	/// </summary>
	CAPITAL = 0x14,

	/// <summary>
	///     Input Method Editor (IME) Kana mode
	/// </summary>
	KANA = 0x15,

	/// <summary>
	///     IME Hangul mode
	/// </summary>
	HANGUL = 0x15,

	/// <summary>
	///     IME Junja mode
	/// </summary>
	JUNJA = 0x17,

	/// <summary>
	///     IME final mode
	/// </summary>
	FINAL = 0x18,

	/// <summary>
	///     IME Hanja mode
	/// </summary>
	HANJA = 0x19,

	/// <summary>
	///     IME Kanji mode
	/// </summary>
	KANJI = 0x19,

	/// <summary>
	///     ESC key
	/// </summary>
	ESCAPE = 0x1B,

	/// <summary>
	///     IME convert
	/// </summary>
	CONVERT = 0x1C,

	/// <summary>
	///     IME nonconvert
	/// </summary>
	NONCONVERT = 0x1D,

	/// <summary>
	///     IME accept
	/// </summary>
	ACCEPT = 0x1E,

	/// <summary>
	///     IME mode change request
	/// </summary>
	MODECHANGE = 0x1F,

	/// <summary>
	///     SPACEBAR
	/// </summary>
	SPACE = 0x20,

	/// <summary>
	///     PAGE UP key
	/// </summary>
	PRIOR = 0x21,

	/// <summary>
	///     PAGE DOWN key
	/// </summary>
	NEXT = 0x22,

	/// <summary>
	///     END key
	/// </summary>
	END = 0x23,

	/// <summary>
	///     HOME key
	/// </summary>
	HOME = 0x24,

	/// <summary>
	///     LEFT ARROW key
	/// </summary>
	LEFT = 0x25,

	/// <summary>
	///     UP ARROW key
	/// </summary>
	UP = 0x26,

	/// <summary>
	///     RIGHT ARROW key
	/// </summary>
	RIGHT = 0x27,

	/// <summary>
	///     DOWN ARROW key
	/// </summary>
	DOWN = 0x28,

	/// <summary>
	///     SELECT key
	/// </summary>
	SELECT = 0x29,

	/// <summary>
	///     PRINT key
	/// </summary>
	PRINT = 0x2A,

	/// <summary>
	///     EXECUTE key
	/// </summary>
	EXECUTE = 0x2B,

	/// <summary>
	///     PRINT SCREEN key
	/// </summary>
	SNAPSHOT = 0x2C,

	/// <summary>
	///     INS key
	/// </summary>
	INSERT = 0x2D,

	/// <summary>
	///     DEL key
	/// </summary>
	DELETE = 0x2E,

	/// <summary>
	///     HELP key
	/// </summary>
	HELP = 0x2F,

	/// <summary>
	///     0 key
	/// </summary>
	KEY_0 = 0x30,

	/// <summary>
	///     1 key
	/// </summary>
	KEY_1 = 0x31,

	/// <summary>
	///     2 key
	/// </summary>
	KEY_2 = 0x32,

	/// <summary>
	///     3 key
	/// </summary>
	KEY_3 = 0x33,

	/// <summary>
	///     4 key
	/// </summary>
	KEY_4 = 0x34,

	/// <summary>
	///     5 key
	/// </summary>
	KEY_5 = 0x35,

	/// <summary>
	///     6 key
	/// </summary>
	KEY_6 = 0x36,

	/// <summary>
	///     7 key
	/// </summary>
	KEY_7 = 0x37,

	/// <summary>
	///     8 key
	/// </summary>
	KEY_8 = 0x38,

	/// <summary>
	///     9 key
	/// </summary>
	KEY_9 = 0x39,

	/// <summary>
	///     A key
	/// </summary>
	KEY_A = 0x41,

	/// <summary>
	///     B key
	/// </summary>
	KEY_B = 0x42,

	/// <summary>
	///     C key
	/// </summary>
	KEY_C = 0x43,

	/// <summary>
	///     D key
	/// </summary>
	KEY_D = 0x44,

	/// <summary>
	///     E key
	/// </summary>
	KEY_E = 0x45,

	/// <summary>
	///     F key
	/// </summary>
	KEY_F = 0x46,

	/// <summary>
	///     G key
	/// </summary>
	KEY_G = 0x47,

	/// <summary>
	///     H key
	/// </summary>
	KEY_H = 0x48,

	/// <summary>
	///     I key
	/// </summary>
	KEY_I = 0x49,

	/// <summary>
	///     J key
	/// </summary>
	KEY_J = 0x4A,

	/// <summary>
	///     K key
	/// </summary>
	KEY_K = 0x4B,

	/// <summary>
	///     L key
	/// </summary>
	KEY_L = 0x4C,

	/// <summary>
	///     M key
	/// </summary>
	KEY_M = 0x4D,

	/// <summary>
	///     N key
	/// </summary>
	KEY_N = 0x4E,

	/// <summary>
	///     O key
	/// </summary>
	KEY_O = 0x4F,

	/// <summary>
	///     P key
	/// </summary>
	KEY_P = 0x50,

	/// <summary>
	///     Q key
	/// </summary>
	KEY_Q = 0x51,

	/// <summary>
	///     R key
	/// </summary>
	KEY_R = 0x52,

	/// <summary>
	///     S key
	/// </summary>
	KEY_S = 0x53,

	/// <summary>
	///     T key
	/// </summary>
	KEY_T = 0x54,

	/// <summary>
	///     U key
	/// </summary>
	KEY_U = 0x55,

	/// <summary>
	///     V key
	/// </summary>
	KEY_V = 0x56,

	/// <summary>
	///     W key
	/// </summary>
	KEY_W = 0x57,

	/// <summary>
	///     X key
	/// </summary>
	KEY_X = 0x58,

	/// <summary>
	///     Y key
	/// </summary>
	KEY_Y = 0x59,

	/// <summary>
	///     Z key
	/// </summary>
	KEY_Z = 0x5A,

	/// <summary>
	///     Left Windows key (Microsoft Natural keyboard)
	/// </summary>
	LWIN = 0x5B,

	/// <summary>
	///     Right Windows key (Natural keyboard)
	/// </summary>
	RWIN = 0x5C,

	/// <summary>
	///     Applications key (Natural keyboard)
	/// </summary>
	APPS = 0x5D,

	/// <summary>
	///     Computer Sleep key
	/// </summary>
	SLEEP = 0x5F,

	/// <summary>
	///     Numeric keypad 0 key
	/// </summary>
	NUMPAD0 = 0x60,

	/// <summary>
	///     Numeric keypad 1 key
	/// </summary>
	NUMPAD1 = 0x61,

	/// <summary>
	///     Numeric keypad 2 key
	/// </summary>
	NUMPAD2 = 0x62,

	/// <summary>
	///     Numeric keypad 3 key
	/// </summary>
	NUMPAD3 = 0x63,

	/// <summary>
	///     Numeric keypad 4 key
	/// </summary>
	NUMPAD4 = 0x64,

	/// <summary>
	///     Numeric keypad 5 key
	/// </summary>
	NUMPAD5 = 0x65,

	/// <summary>
	///     Numeric keypad 6 key
	/// </summary>
	NUMPAD6 = 0x66,

	/// <summary>
	///     Numeric keypad 7 key
	/// </summary>
	NUMPAD7 = 0x67,

	/// <summary>
	///     Numeric keypad 8 key
	/// </summary>
	NUMPAD8 = 0x68,

	/// <summary>
	///     Numeric keypad 9 key
	/// </summary>
	NUMPAD9 = 0x69,

	/// <summary>
	///     Multiply key
	/// </summary>
	MULTIPLY = 0x6A,

	/// <summary>
	///     Add key
	/// </summary>
	ADD = 0x6B,

	/// <summary>
	///     Separator key
	/// </summary>
	SEPARATOR = 0x6C,

	/// <summary>
	///     Subtract key
	/// </summary>
	SUBTRACT = 0x6D,

	/// <summary>
	///     Decimal key
	/// </summary>
	DECIMAL = 0x6E,

	/// <summary>
	///     Divide key
	/// </summary>
	DIVIDE = 0x6F,

	/// <summary>
	///     F1 key
	/// </summary>
	F1 = 0x70,

	/// <summary>
	///     F2 key
	/// </summary>
	F2 = 0x71,

	/// <summary>
	///     F3 key
	/// </summary>
	F3 = 0x72,

	/// <summary>
	///     F4 key
	/// </summary>
	F4 = 0x73,

	/// <summary>
	///     F5 key
	/// </summary>
	F5 = 0x74,

	/// <summary>
	///     F6 key
	/// </summary>
	F6 = 0x75,

	/// <summary>
	///     F7 key
	/// </summary>
	F7 = 0x76,

	/// <summary>
	///     F8 key
	/// </summary>
	F8 = 0x77,

	/// <summary>
	///     F9 key
	/// </summary>
	F9 = 0x78,

	/// <summary>
	///     F10 key
	/// </summary>
	F10 = 0x79,

	/// <summary>
	///     F11 key
	/// </summary>
	F11 = 0x7A,

	/// <summary>
	///     F12 key
	/// </summary>
	F12 = 0x7B,

	/// <summary>
	///     F13 key
	/// </summary>
	F13 = 0x7C,

	/// <summary>
	///     F14 key
	/// </summary>
	F14 = 0x7D,

	/// <summary>
	///     F15 key
	/// </summary>
	F15 = 0x7E,

	/// <summary>
	///     F16 key
	/// </summary>
	F16 = 0x7F,

	/// <summary>
	///     F17 key
	/// </summary>
	F17 = 0x80,

	/// <summary>
	///     F18 key
	/// </summary>
	F18 = 0x81,

	/// <summary>
	///     F19 key
	/// </summary>
	F19 = 0x82,

	/// <summary>
	///     F20 key
	/// </summary>
	F20 = 0x83,

	/// <summary>
	///     F21 key
	/// </summary>
	F21 = 0x84,

	/// <summary>
	///     F22 key, (PPC only) Key used to lock device.
	/// </summary>
	F22 = 0x85,

	/// <summary>
	///     F23 key
	/// </summary>
	F23 = 0x86,

	/// <summary>
	///     F24 key
	/// </summary>
	F24 = 0x87,

	/// <summary>
	///     NUM LOCK key
	/// </summary>
	NUMLOCK = 0x90,

	/// <summary>
	///     SCROLL LOCK key
	/// </summary>
	SCROLL = 0x91,

	/// <summary>
	///     Left SHIFT key
	/// </summary>
	LSHIFT = 0xA0,

	/// <summary>
	///     Right SHIFT key
	/// </summary>
	RSHIFT = 0xA1,

	/// <summary>
	///     Left CONTROL key
	/// </summary>
	LCONTROL = 0xA2,

	/// <summary>
	///     Right CONTROL key
	/// </summary>
	RCONTROL = 0xA3,

	/// <summary>
	///     Left MENU key
	/// </summary>
	LMENU = 0xA4,

	/// <summary>
	///     Right MENU key
	/// </summary>
	RMENU = 0xA5,

	/// <summary>
	///     Windows 2000/XP: Browser Back key
	/// </summary>
	BROWSER_BACK = 0xA6,

	/// <summary>
	///     Windows 2000/XP: Browser Forward key
	/// </summary>
	BROWSER_FORWARD = 0xA7,

	/// <summary>
	///     Windows 2000/XP: Browser Refresh key
	/// </summary>
	BROWSER_REFRESH = 0xA8,

	/// <summary>
	///     Windows 2000/XP: Browser Stop key
	/// </summary>
	BROWSER_STOP = 0xA9,

	/// <summary>
	///     Windows 2000/XP: Browser Search key
	/// </summary>
	BROWSER_SEARCH = 0xAA,

	/// <summary>
	///     Windows 2000/XP: Browser Favorites key
	/// </summary>
	BROWSER_FAVORITES = 0xAB,

	/// <summary>
	///     Windows 2000/XP: Browser Start and Home key
	/// </summary>
	BROWSER_HOME = 0xAC,

	/// <summary>
	///     Windows 2000/XP: Volume Mute key
	/// </summary>
	VOLUME_MUTE = 0xAD,

	/// <summary>
	///     Windows 2000/XP: Volume Down key
	/// </summary>
	VOLUME_DOWN = 0xAE,

	/// <summary>
	///     Windows 2000/XP: Volume Up key
	/// </summary>
	VOLUME_UP = 0xAF,

	/// <summary>
	///     Windows 2000/XP: Next Track key
	/// </summary>
	MEDIA_NEXT_TRACK = 0xB0,

	/// <summary>
	///     Windows 2000/XP: Previous Track key
	/// </summary>
	MEDIA_PREV_TRACK = 0xB1,

	/// <summary>
	///     Windows 2000/XP: Stop Media key
	/// </summary>
	MEDIA_STOP = 0xB2,

	/// <summary>
	///     Windows 2000/XP: Play/Pause Media key
	/// </summary>
	MEDIA_PLAY_PAUSE = 0xB3,

	/// <summary>
	///     Windows 2000/XP: Start Mail key
	/// </summary>
	LAUNCH_MAIL = 0xB4,

	/// <summary>
	///     Windows 2000/XP: Select Media key
	/// </summary>
	LAUNCH_MEDIA_SELECT = 0xB5,

	/// <summary>
	///     Windows 2000/XP: Start Application 1 key
	/// </summary>
	LAUNCH_APP1 = 0xB6,

	/// <summary>
	///     Windows 2000/XP: Start Application 2 key
	/// </summary>
	LAUNCH_APP2 = 0xB7,

	/// <summary>
	///     Used for miscellaneous characters; it can vary by keyboard.
	/// </summary>
	OEM_1 = 0xBA,

	/// <summary>
	///     Windows 2000/XP: For any country/region, the '+' key
	/// </summary>
	OEM_PLUS = 0xBB,

	/// <summary>
	///     Windows 2000/XP: For any country/region, the ',' key
	/// </summary>
	OEM_COMMA = 0xBC,

	/// <summary>
	///     Windows 2000/XP: For any country/region, the '-' key
	/// </summary>
	OEM_MINUS = 0xBD,

	/// <summary>
	///     Windows 2000/XP: For any country/region, the '.' key
	/// </summary>
	OEM_PERIOD = 0xBE,

	/// <summary>
	///     Used for miscellaneous characters; it can vary by keyboard.
	/// </summary>
	OEM_2 = 0xBF,

	/// <summary>
	///     Used for miscellaneous characters; it can vary by keyboard.
	/// </summary>
	OEM_3 = 0xC0,

	/// <summary>
	///     Used for miscellaneous characters; it can vary by keyboard.
	/// </summary>
	OEM_4 = 0xDB,

	/// <summary>
	///     Used for miscellaneous characters; it can vary by keyboard.
	/// </summary>
	OEM_5 = 0xDC,

	/// <summary>
	///     Used for miscellaneous characters; it can vary by keyboard.
	/// </summary>
	OEM_6 = 0xDD,

	/// <summary>
	///     Used for miscellaneous characters; it can vary by keyboard.
	/// </summary>
	OEM_7 = 0xDE,

	/// <summary>
	///     Used for miscellaneous characters; it can vary by keyboard.
	/// </summary>
	OEM_8 = 0xDF,

	/// <summary>
	///     Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
	/// </summary>
	OEM_102 = 0xE2,

	/// <summary>
	///     Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
	/// </summary>
	PROCESSKEY = 0xE5,

	/// <summary>
	///     Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
	///     The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more
	///     information,
	///     see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
	/// </summary>
	PACKET = 0xE7,

	/// <summary>
	///     Attn key
	/// </summary>
	ATTN = 0xF6,

	/// <summary>
	///     CrSel key
	/// </summary>
	CRSEL = 0xF7,

	/// <summary>
	///     ExSel key
	/// </summary>
	EXSEL = 0xF8,

	/// <summary>
	///     Erase EOF key
	/// </summary>
	EREOF = 0xF9,

	/// <summary>
	///     Play key
	/// </summary>
	PLAY = 0xFA,

	/// <summary>
	///     Zoom key
	/// </summary>
	ZOOM = 0xFB,

	/// <summary>
	///     Reserved
	/// </summary>
	NONAME = 0xFC,

	/// <summary>
	///     PA1 key
	/// </summary>
	PA1 = 0xFD,

	/// <summary>
	///     Clear key
	/// </summary>
	OEM_CLEAR = 0xFE
}

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

/// <summary>
/// Reports window buffer sizing events in a console input record.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct WindowBufferSizeEvent
{
	[FieldOffset(0)]
	public Coord dwSize;
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
	SCROLL_DOWN = unchecked((int) 0xFF000000)
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