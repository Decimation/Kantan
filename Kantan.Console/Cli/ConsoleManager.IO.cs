using System;
using System.Collections.Concurrent;
using System.ComponentModel;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Local

namespace Kantan.Console.Cli;
#pragma warning disable IDE0059, CA1416
public static partial class ConsoleManager
{
	public static bool InputAvailable
	{
		get
		{
			unsafe {
				var b = Win32.GetNumberOfConsoleInputEvents(StdIn, out var n);

				// var buffer = new InputRecord[1];

				// var buffer = ArrayPool<InputRecord>.Shared.Rent(1);
				const int i      = 1;
				var       buffer = stackalloc InputRecord[i];

				if (n == 0) {
					return false;
				}

				Win32.PeekConsoleInput(StdIn, buffer, i, out var lpNumber);

				var ir = buffer[0];

				if (!ir.IsMouseEvent && (!ir.IsKeyDownEvent || ir.IsModKey)) {

					Win32.ReadConsoleInput(StdIn, buffer, (uint) i,
					                       out var numEventsRead);

				}
				else {
					return true;
				}

				// return lp != 0;
				return false;
			}

		}
	}

	public static bool SetWindowPosition(int left, int newTop)
	{
		if (!OperatingSystem.IsWindows()) {
			return false;
		}

		// var b = !(newTop > SConsole.BufferHeight) && !(newTop < 0);

		bool b = true;

		// Get the size of the current console window
		ConsoleScreenBufferInfo csbi = GetBufferInfo(out _);

		SmallRect srWindow = csbi.srWindow;

		// Check for arithmetic underflows & overflows.

		int newRight = left + srWindow.Right - srWindow.Left;

		int dwSizeX = csbi.dwSize.X - 1;

		if (left < 0 || newRight > dwSizeX || newRight < left) {
			b = false;
		}

		int newBottom = newTop + srWindow.Bottom - srWindow.Top;

		int dwSizeY = csbi.dwSize.Y - 1;

		if (newTop < 0 || newBottom > dwSizeY || newBottom < newTop) {
			b = false;
		}

		if (b) {
			SConsole.SetWindowPosition(0, newTop);
		}

		return b;
	}

	/*private static void ScrollNative(int nRows)
	{
		var csbiInfo   = new ConsoleScreenBufferInfo();
		var srctWindow = new SmallRect(0, 0, 0, 0);

		// Get the current screen buffer window position.

		if (!Native.GetConsoleScreenBufferInfo(StdOut, csbiInfo)) {
			throw new Win32Exception();
		}

		// Check whether the window is too close to the screen buffer top

		if (csbiInfo.srWindow.Top >= nRows) {
			srctWindow.Top    = (short) -nRows; // move top up
			srctWindow.Bottom = (short) -nRows; // move bottom up
			srctWindow.Left   = 0;              // no change
			srctWindow.Right  = 0;              // no change

			if (!Native.SetConsoleWindowInfo(StdOut, false, srctWindow)) {
				throw new Win32Exception();
			}

		}
	}*/

	private static ConsoleScreenBufferInfo GetBufferInfo(out bool ok)
	{
		ok = Win32.GetConsoleScreenBufferInfo(StdOut, out ConsoleScreenBufferInfo csbi);

		return csbi;
	}

	/// <summary>
	///     Attempts to resize the console window
	/// </summary>
	/// <returns><c>true</c> if the operation succeeded</returns>
	public static bool Resize(int cww, int cwh)
	{
		bool canResize = SConsole.LargestWindowWidth >= cww &&
		                 SConsole.LargestWindowHeight >= cwh;

		if (canResize) {
			SConsole.SetWindowSize(cww, cwh);
		}

		return canResize;
	}

	private static ConsoleModes _oldMode;

	internal static ConcurrentStack<InputRecord> History { get; private set; } = new();

	public static IntPtr StdIn { get; private set; }

	public static IntPtr StdOut { get; private set; }

	public static string ReadBufferLine(int y) => ReadBufferXY(SConsole.BufferWidth, 0, y);

	public static string ReadBufferXY(int x, int y) => ReadBufferXY(SConsole.BufferWidth, x, y);

	/// <summary>
	/// Reads characters from the screen buffer, starting at the given position.
	/// </summary>
	/// <param name="nChars">The number of characters to read.</param>
	/// <param name="x">Column position of the first character to read.</param>
	/// <param name="y">Row position of the first character to read.</param>
	/// <returns>A string containing the characters read from the screen buffer.</returns>
	public static string ReadBufferXY(int nChars, int x, int y)
	{
		char[] buff      = new char[nChars];
		int    charsRead = 0;

		if (!Win32.ReadConsoleOutputCharacter(StdOut, buff, nChars, new Coord((ushort) x, (ushort) y),
		                                      ref charsRead)) {
			throw new Win32Exception();
		}

		return new string(buff, 0, charsRead);
	}

	public static int WriteBufferLine(string s, int y) => WriteBufferXY(s.ToCharArray(), s.Length, 0, y);

	/// <summary>
	/// Writes characters to the buffer at the given position.
	/// The cursor position is not updated.
	/// </summary>
	/// <param name="s">The string to be output.</param>
	/// <param name="x">Column position of the starting location.</param>
	/// <param name="y">Row position of the starting location.</param>
	/// <returns></returns>
	public static int WriteBufferXY(string s, int x, int y) => WriteBufferXY(s.ToCharArray(), s.Length, x, y);

	public static int WriteAttributesXY(ConsoleCharAttribute[] attrs, int x, int y)
		=> WriteAttributesXY(attrs, attrs.Length, x, y);

	/// <summary>
	/// Writes character attributes to the screen buffer at the given cursor position.
	/// </summary>
	/// <param name="attrs">An array of attributes to be written to the screen buffer.</param>
	/// <param name="numAttrs">The number of attributes to be written.</param>
	/// <param name="x">Column position in which to write the first attribute.</param>
	/// <param name="y">Row position in which to write the first attribute.</param>
	/// <returns>Returns the number of attributes written.</returns>
	public static int WriteAttributesXY(ConsoleCharAttribute[] attrs, int numAttrs, int x, int y)
	{
		if (numAttrs > attrs.Length) {
			throw new ArgumentException(null, nameof(attrs));
		}

		int attrsWritten = 0;
		var writePos     = new Coord((short) x, (short) y);

		if (!Win32.WriteConsoleOutputAttribute(StdOut, attrs, attrs.Length, writePos, ref attrsWritten)) {
			throw new Win32Exception();
		}

		return attrsWritten;
	}

	/// <summary>
	/// Writes character and attribute information to a rectangular portion of the screen buffer.
	/// </summary>
	/// <param name="buff">The array that contains characters and attributes to be written.</param>
	/// <param name="buffX">Column position of the first character to be written from the array.</param>
	/// <param name="buffY">Row position of the first character to be written from the array.</param>
	/// <param name="left">Column position of the top-left corner of the screen buffer area where characters are to be written.</param>
	/// <param name="top">Row position of the top-left corner of the screen buffer area where characters are to be written.</param>
	/// <param name="right">Column position of the bottom-right corner of the screen buffer area where characters are to be written.</param>
	/// <param name="bottom">Row position of the bottom-right corner of the screen buffer area where characters are to be written.</param>
	public static void WriteBlock(ConsoleCharInfo[,] buff, int buffX, int buffY, int left, int top, int right,
	                              int bottom)
	{
		var bufferSize  = new Coord((short) buff.GetLength(1), (short) buff.GetLength(0));
		var bufferPos   = new Coord((short) buffX, (short) buffY);
		var writeRegion = new SmallRect((short) left, (short) top, (short) right, (short) bottom);

		if (!Win32.WriteConsoleOutput(StdOut, buff, bufferSize, bufferPos, writeRegion)) {
			throw new Win32Exception();
		}
	}

	public static int WriteBufferXY(char[] text, int x, int y) => WriteBufferXY(text, text.Length, x, y);

	/// <summary>
	/// Writes characters from a character array to the screen buffer at the given cursor position.
	/// </summary>
	/// <param name="text">An array containing the characters to be written.</param>
	/// <param name="nChars">The number of characters to be written.</param>
	/// <param name="x">Column position in which to write the first character.</param>
	/// <param name="y">Row position in which to write the first character.</param>
	/// <returns>Returns the number of characters written.</returns>
	public static int WriteBufferXY(char[] text, int nChars, int x, int y)
	{
		if (nChars > text.Length) {
			throw new ArgumentException("Cannot be larger than the array length.", nameof(nChars));
		}

		int charsWritten = 0;

		var writePos = new Coord((ushort) x, (ushort) y);

		if (!Win32.WriteConsoleOutputCharacter(StdOut, text, nChars, writePos, ref charsWritten)) {
			throw new Win32Exception();
		}

		return charsWritten;
	}

	public static ConsoleKeyInfo GetKeyInfo(char c, int k, ControlKeyState state)
	{
		bool shift   = (state & ControlKeyState.ShiftPressed) != 0;
		bool alt     = (state & (ControlKeyState.LeftAltPressed | ControlKeyState.RightAltPressed)) != 0;
		bool control = (state & (ControlKeyState.LeftCtrlPressed | ControlKeyState.RightCtrlPressed)) != 0;

		var info = new ConsoleKeyInfo(c, (ConsoleKey) k, shift, alt, control);

		return info;
	}

	public static InputRecord ReadInput()
	{
		unsafe {
			// var record = new InputRecord[1];
			// var record=ArrayPool<InputRecord>.Shared.Rent(1);

			const int i      = 1;
			var       record = stackalloc InputRecord[i];

			if (!Win32.ReadConsoleInput(StdIn, record, (uint) i, out uint n)) {
				throw new Win32Exception();
			}

			var read = record[0];
			History.Push(read);

			return read;
		}
	}

	public static void Close()
	{
		Win32.SetConsoleMode(StdIn, _oldMode);
		StdIn    = IntPtr.Zero;
		StdOut   = IntPtr.Zero;
		_oldMode = 0;
		History.Clear();
	}

	public static void Init()
	{
		StdOut = Win32.GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE);
		StdIn  = Win32.GetStdHandle(StandardHandle.STD_INPUT_HANDLE);

		if (!Win32.GetConsoleMode(StdIn, out ConsoleModes mode)) {
			throw new Win32Exception();
		}

		_oldMode = mode;

		mode |= ConsoleModes.ENABLE_MOUSE_INPUT;
		mode &= ~ConsoleModes.ENABLE_QUICK_EDIT_MODE;
		mode |= ConsoleModes.ENABLE_EXTENDED_FLAGS;
		mode |= ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING;

		if (!Win32.SetConsoleMode(StdIn, mode)) {
			throw new Win32Exception();
		}
	}

	public static void Highlight(ConsoleCharAttribute attr, int len, ushort x, ushort y)
	{
		var attrs = new ConsoleCharAttribute[len];

		Array.Fill(attrs, attr);

		WriteAttributesXY(attrs, attrs.Length, x, y);
	}

	public static bool Scroll(int increment)
	{
		var newTop = increment + SConsole.WindowTop;

		return SetWindowPosition(0, newTop);
	}

	///<remarks>Technically this reads the input buffer but has the same behavior as clearing it (?)</remarks>
	/// <a href="https://stackoverflow.com/questions/3769770/clear-console-buffer">See</a>
	public static ConsoleKeyInfo[] ClearInputBuffer()
	{
		var rg = new List<ConsoleKeyInfo>();

		while (SConsole.KeyAvailable) {
			rg.Add(SConsole.ReadKey(false)); // skips previous input chars
		}

		rg.Add(SConsole.ReadKey(true)); // reads a char

		return rg.ToArray();
	}

	public static ConsoleCharAttribute HighlightAttribute { get; set; } =
		new(ConsoleColor.Black, ConsoleColor.White);
}