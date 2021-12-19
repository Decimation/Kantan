global using UT = System.Runtime.InteropServices.UnmanagedType;
global using MA = System.Runtime.InteropServices.MarshalAsAttribute;
using System;
using System.Runtime.InteropServices;
using Kantan.Win32.Structures;

// ReSharper disable CommentTypo

// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global

namespace Kantan.Win32;

internal static class Native
{
	private const string KERNEL32_DLL = "kernel32.dll";

	/*
	 * Note: some code comes from:
	 * Novus
	 * .NET BCL
	 * http://mischel.com/pubs/consoledotnet/consoledotnet.zip,
	 * https://www.medo64.com/2013/05/console-mouse-input-in-c/
	 */

	private const string USER32_DLL = "user32.dll";

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	internal static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput,
	                                                       out ConsoleScreenBufferInfo lpConsoleScreenBufferInfo);

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	internal static extern bool GetConsoleSelectionInfo(out ConsoleSelectionInfo lpConsoleSelectionInfo);

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	internal static extern int GetConsoleOutputCP();

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	internal static extern bool SetConsoleCP(int wCodePageID);

	[DllImport(KERNEL32_DLL, CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MA(UT.Bool)]
	internal static extern bool PeekConsoleInput(IntPtr hConsoleInput, out InputRecord lpBuffer,
	                                             uint nLength, out uint lpNumberOfEventsRead);

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

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

	[DllImport(KERNEL32_DLL, SetLastError = true)]
	[return: MA(UT.Bool)]
	internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

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

	[DllImport(USER32_DLL, SetLastError = true, CharSet = CharSet.Unicode)]
	private static extern IntPtr FindWindow(IntPtr zeroOnly, string lpWindowName);

	public static IntPtr FindWindow(string lpWindowName) => FindWindow(IntPtr.Zero, lpWindowName);

	[DllImport(USER32_DLL)]
	public static extern short GetKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	public static extern short GetAsyncKeyState(VirtualKey k);

	[DllImport(USER32_DLL)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetKeyboardState([MarshalAs(UnmanagedType.LPArray), In] byte[] r);

	[DllImport(USER32_DLL, CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetForegroundWindow();
}

#region Enums

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

#endregion