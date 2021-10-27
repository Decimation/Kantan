using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using JetBrains.Annotations;
using Kantan.Cli.Controls;
using Kantan.Native;
using Kantan.Native.Structures;
using Kantan.Text;
using Kantan.Utilities;
using static Kantan.Internal.Common;
using static Kantan.Text.Strings;

// ReSharper disable IdentifierTypo
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable CognitiveComplexity
// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
// ReSharper disable InvocationIsSkipped
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable UnusedVariable
// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable SuggestVarOrType_Elsewhere

#pragma warning disable 8601
#pragma warning disable CA1416

#pragma warning disable 8602, CS8604, IDE0059, IDE0051
#nullable enable

namespace Kantan.Cli
{
	/// <summary>
	///     Extended console.
	/// </summary>
	/// <list type="bullet">
	///     <item>
	///         <description>
	///             <see cref="ConsoleManager" />
	///         </description>
	///     </item>
	///     <item>
	///         <description>
	///             <see cref="ConsoleOption" />
	///         </description>
	///     </item>
	///     <item>
	///         <description>
	///             <see cref="ConsoleDialog" />
	///         </description>
	///     </item>
	///     <item>
	///         <description>
	///             <see cref="ConsoleProgressIndicator" />
	///         </description>
	///     </item>
	/// </list>
	public static class ConsoleManager
	{
		public static Color ColorHeader  { get; set; } = Color.Red;
		public static Color ColorOptions { get; set; } = Color.Aquamarine;
		public static Color ColorError   { get; set; } = Color.Red;


		public static ConsoleCharAttribute HighlightAttribute { get; set; } =
			new(ConsoleColor.Black, ConsoleColor.White);

		public static int BufferLimit { get; set; } = Console.BufferWidth - 10;

		private static TimeSpan PauseTime { get; set; } = TimeSpan.FromSeconds(1);

		/*
		 * https://github.com/Decimation/SimpleCore/blob/2d6009cfc498de07d5f507192c3cbe1983ff1a11/SimpleCore.Cli/NConsole.cs
		 * https://gist.github.com/ZacharyPatten/798ed612d692a560bdd529367b6a7dbd
		 * https://github.com/ZacharyPatten/Towel
		 *
		 *
		 * https://stackoverflow.com/questions/888533/how-can-i-update-the-current-line-in-a-c-sharp-windows-console-app
		 * https://stackoverflow.com/questions/5435460/console-application-how-to-update-the-display-without-flicker
		 * https://github.com/migueldeicaza/gui.cs
		 * https://github.com/TomaszRewak/C-sharp-console-gui-framework
		 * https://github.com/goblinfactory/konsole
		 */

		/*
		 * TODO: this design isn't great
		 */

		public static int ScrollIncrement { get; set; } = 1;

		private const char OPTION_N = 'N';
		private const char OPTION_Y = 'Y';

		public static void Init()
		{
			//Console.OutputEncoding = Encoding.Unicode;
		}

		#region IO

		/// <summary>
		///     Root formatting function.
		/// </summary>
		[StringFormatMethod(STRING_FORMAT_ARG)]
		internal static string FormatString(string? delim, string msg, bool repDelim = true)
		{
			//Debug.WriteLine(l.FuncJoin((s) => $"[{s}]"));

			string[] split = msg.Split(Constants.NEW_LINE);

			bool d1     = false;
			bool useBox = delim is null;

			for (int i = 0; i < split.Length; i++) {
				string a = Constants.SPACE + split[i];

				string b;

				if (useBox) {

					delim = GetUnicodeBoxPipe(split, i);
				}

				if (String.IsNullOrWhiteSpace(a)) {
					b = String.Empty;
				}
				else {
					if (repDelim || !d1) {
						b  = delim + a;
						d1 = true;
					}

					else b = new string(' ', delim.Length) + a;

				}

				string c = b.Truncate(BufferLimit);

				if (c.Length < b.Length) {
					c += Constants.ELLIPSES;
				}

				split[i] = c;
			}

			return String.Join(Constants.NEW_LINE, split);
		}

		public static string ReadLine(string? prompt = null, Predicate<string>? invalid = null,
		                              string? errPrompt = null)
		{
			invalid ??= String.IsNullOrWhiteSpace;

			string? input;
			bool    isInvalid;

			do {
				//https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console

				ClearCurrentLineCR();

				if (prompt != null) {
					string str = $"{Constants.CHEVRON} {prompt}: ".AddColor(ColorOptions);

					Console.Write(str);
				}

				input = Console.ReadLine();

				isInvalid = invalid(input);

				if (isInvalid) {
					errPrompt ??= "Invalid input";
					Console.WriteLine(errPrompt.AddColor(ColorError));
					Thread.Sleep(PauseTime);
					//Console.Write(new string('\r',7));
					/*ClearLastLine();
					Console.CursorTop--;
					*/

					ClearCurrentLine();
					Console.CursorTop--;

				}

			} while (isInvalid);

			return input;
		}

		[StringFormatMethod(STRING_FORMAT_ARG)]
		public static bool ReadConfirmation(string msg, params object[] args)
		{
			Write($"{Constants.ASTERISK} {String.Format(msg, args)} ({OPTION_Y}/{OPTION_N}): ");

			char key = Char.ToUpper(ReadKey().KeyChar);

			Console.WriteLine();

			return key switch
			{
				OPTION_N => false,
				OPTION_Y => true,
				_        => ReadConfirmation(msg, args)
			};
		}

		public static ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);

		public static ConsoleKeyInfo ReadKey() => ReadKey(false);

		#region Wait

		public static void WaitForInput()
		{
			Console.WriteLine();
			Console.WriteLine("Press any key to continue...");
			ReadKey();
		}

		public static void WaitForTimeSpan(TimeSpan span) => Thread.Sleep(span);

		public static void WaitForSecond() => WaitForTimeSpan(PauseTime);

		#endregion

		#endregion IO

		#region Write

		public static void Print(params object[] args) => Console.WriteLine(args.QuickJoin());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NewLine() => Console.WriteLine();

		/// <summary>
		///     Root write method.
		/// </summary>
		[StringFormatMethod(STRING_FORMAT_ARG)]
		public static void Write(bool newLine, string msg, params object[] args)
		{
			string fmt = String.Format(msg, args);

			if (newLine) {
				Console.WriteLine(fmt);
			}
			else {
				Console.Write(fmt);
			}
		}

		[StringFormatMethod(STRING_FORMAT_ARG)]
		public static void Write(string msg, params object[] args) => Write(true, msg, args);

		[StringFormatMethod(STRING_FORMAT_ARG)]
		public static void WriteOnCurrentLine(string msg, params object[] args)
		{
			msg = String.Format(msg, args);

			string clear = new('\b', msg.Length);
			Console.Write(clear);
			Write(msg);
		}

		public static void WriteOnLine(string s, int newTop)
		{
			(int left, int top) = Console.GetCursorPosition();
			ClearLine(newTop);
			Console.WriteLine(s);
			Console.SetCursorPosition(left, top);
		}

		#endregion

		#region Clear

		private static void ClearCurrentLineCR()
		{
			Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
		}

		public static void ClearLine(int newTop, int? top = null)
		{
			(int left, int top1) = Console.GetCursorPosition();

			top ??= top1;

			Console.SetCursorPosition(0, newTop);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(left, top.Value);

		}

		public static void ClearCurrentLine()
		{
			Console.SetCursorPosition(0, Console.CursorTop - 1);

			int top = Console.CursorTop;
			/*Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(0, top);*/
			ClearLine(Console.CursorTop, top);

		}

		public static void ClearLastLine()
		{
			/*var top = Console.CursorTop;
			Console.SetCursorPosition(0, top - 1);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(0, top);*/

			ClearLine(Console.CursorTop - 1);
		}

		#endregion

		public static bool SetWindowPosition(int left, int newTop)
		{
			if (!OperatingSystem.IsWindows()) {
				return false;
			}

			// var b = !(newTop > Console.BufferHeight) && !(newTop < 0);

			bool b = true;

			// Get the size of the current console window
			ConsoleScreenBufferInfo csbi = GetBufferInfo(out _);

			SmallRect srWindow = csbi.srWindow;

			// Check for arithmetic underflows & overflows.

			int newRight = left + srWindow.Right - srWindow.Left;

			int dwSizeX   = csbi.dwSize.X - 1;
			

			if (left < 0 || newRight > dwSizeX || newRight < left) {
				b = false;
			}

			int newBottom = newTop + srWindow.Bottom - srWindow.Top;
			
			int dwSizeY = csbi.dwSize.Y - 1;
			
			if (newTop < 0 || newBottom > dwSizeY || newBottom < newTop) {
				b = false;
			}
			
			if (b) {
				Console.SetWindowPosition(0, newTop);
			}

			return b;
		}

		public static bool Scroll(int increment)
		{
			var newTop = increment + Console.WindowTop;

			return SetWindowPosition(0, newTop);
		}

		[SupportedOSPlatform(OS_WINDOWS)]
		private static ConsoleScreenBufferInfo GetBufferInfo(out bool ok)
		{
			ok = Win32.GetConsoleScreenBufferInfo(_stdOut, out ConsoleScreenBufferInfo csbi);

			return csbi;
		}

		/// <summary>
		///     Attempts to resize the console window
		/// </summary>
		/// <returns><c>true</c> if the operation succeeded</returns>
		[SupportedOSPlatform(OS_WINDOWS)]
		public static bool Resize(int cww, int cwh)
		{
			bool canResize = Console.LargestWindowWidth >= cww &&
			                 Console.LargestWindowHeight >= cwh;

			if (canResize) {
				Console.SetWindowSize(cww, cwh);
			}

			return canResize;
		}

		internal static bool         _click;
		private static  IntPtr       _stdIn;
		private static  IntPtr       _stdOut;
		private static  ConsoleModes _oldMode;

		[SupportedOSPlatform(OS_WINDOWS)]
		public static string ReadBufferLine(int y) => ReadBufferXY(Console.BufferWidth, 0, y);

		[SupportedOSPlatform(OS_WINDOWS)]
		public static string ReadBufferXY(int x, int y) => ReadBufferXY(Console.BufferWidth, x, y);

		/// <summary>
		/// Reads characters from the screen buffer, starting at the given position.
		/// </summary>
		/// <param name="nChars">The number of characters to read.</param>
		/// <param name="x">Column position of the first character to read.</param>
		/// <param name="y">Row position of the first character to read.</param>
		/// <returns>A string containing the characters read from the screen buffer.</returns>
		[SupportedOSPlatform(OS_WINDOWS)]
		public static string ReadBufferXY(int nChars, int x, int y)
		{
			char[] buff      = new char[nChars];
			int    charsRead = 0;

			if (!Win32.ReadConsoleOutputCharacter(_stdOut, buff, nChars, new Coord((ushort) x, (ushort) y),
			                                      ref charsRead)) {
				throw new Win32Exception();
			}

			return new string(buff, 0, charsRead);
		}

		[SupportedOSPlatform(OS_WINDOWS)]
		public static int WriteBufferLine(string text, int y) => WriteBufferXY(text.ToCharArray(), text.Length, 0, y);

		/// <summary>
		/// Writes characters to the buffer at the given position.
		/// The cursor position is not updated.
		/// </summary>
		/// <param name="text">The string to be output.</param>
		/// <param name="x">Column position of the starting location.</param>
		/// <param name="y">Row position of the starting location.</param>
		/// <returns></returns>
		[SupportedOSPlatform(OS_WINDOWS)]
		public static int WriteBufferXY(string text, int x, int y)
			=> WriteBufferXY(text.ToCharArray(), text.Length, x, y);

		public static int WriteAttributesXY(ConsoleCharAttribute[] attrs, int x, int y)
			=> WriteAttributesXY(attrs, attrs.Length, x, y);

		/// <summary>
		/// Writes character attributes to the screen buffer at the given cursor position.
		/// </summary>
		/// <param name="attrs">An array of attributes to be written to the screen buffer.</param>
		/// <param name="nattrs">The number of attributes to be written.</param>
		/// <param name="x">Column position in which to write the first attribute.</param>
		/// <param name="y">Row position in which to write the first attribute.</param>
		/// <returns>Returns the number of attributes written.</returns>
		public static int WriteAttributesXY(ConsoleCharAttribute[] attrs, int nattrs, int x, int y)
		{
			if (nattrs > attrs.Length) {
				throw new ArgumentException();
			}

			int   attrsWritten = 0;
			Coord writePos     = new Coord((short) x, (short) y);

			if (!Win32.WriteConsoleOutputAttribute(_stdOut, attrs, attrs.Length, writePos, ref attrsWritten)) {
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

			if (!Win32.WriteConsoleOutput(_stdOut, buff, bufferSize, bufferPos, writeRegion)) {
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
		[SupportedOSPlatform(OS_WINDOWS)]
		public static int WriteBufferXY(char[] text, int nChars, int x, int y)
		{
			if (nChars > text.Length) {
				throw new ArgumentException("Cannot be larger than the array length.", nameof(nChars));
			}

			int charsWritten = 0;

			var writePos = new Coord((ushort) x, (ushort) y);

			if (!Win32.WriteConsoleOutputCharacter(_stdOut, text, nChars, writePos, ref charsWritten)) {
				throw new Win32Exception();
			}

			return charsWritten;
		}

		private static bool IsKeyDownEvent(InputRecord ir)
		{
			var keyEvent = ir.KeyEvent;
			return ir.EventType == ConsoleEventType.KEY_EVENT && keyEvent.bKeyDown != BOOL.FALSE;
		}

		internal static bool IsMouseScroll(InputRecord ir)
		{
			var mouseEvent = ir.MouseEvent;

			var mouseWheel = mouseEvent.dwEventFlags is MouseEventFlags.MOUSE_WHEELED
				                 or MouseEventFlags.MOUSE_HWHEELED;

			return ir.EventType == ConsoleEventType.MOUSE_EVENT &&
			       mouseEvent.dwEventFlags != MouseEventFlags.MOUSE_MOVED && mouseWheel;
		}

		private static bool IsMouseEvent(InputRecord ir)
		{
			var mouseEvent = ir.MouseEvent;

			return ir.EventType == ConsoleEventType.MOUSE_EVENT &&
			       mouseEvent.dwEventFlags != MouseEventFlags.MOUSE_MOVED || IsMouseScroll(ir);
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

		internal static ConsoleKeyInfo GetKeyInfoFromRecord(InputRecord ir)
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

		public static bool InputAvailable
		{
			get
			{
				if (!OperatingSystem.IsWindows()) {
					return Console.KeyAvailable;
				}

				while (true) {

					if (!Win32.PeekConsoleInput(_stdIn, out InputRecord ir, 1, out uint numEventsRead)) {
						throw new Win32Exception();
					}

					//Debug.WriteLine(ir);

					if (numEventsRead == 0) {
						return false;
					}

					// Skip non key-down && mod key events.

					if (!IsMouseEvent(ir) && (!IsKeyDownEvent(ir) || IsModKey(ir))) {
						var rg = new InputRecord[1];

						if (!Win32.ReadConsoleInput(_stdIn, rg, 1, out numEventsRead))
							throw new Win32Exception();
					}
					else {
						return true;
					}
				}
			}
		}

		[SupportedOSPlatform(OS_WINDOWS)]
		internal static InputRecord ReadInputRecord()
		{
			var record = new InputRecord[1];

			if (!Win32.ReadConsoleInput(_stdIn, record, 1, out uint lpNumberOfEventsRead)) {
				throw new Win32Exception();
			}

			var read = record[0];

			return read;

		}

		[SupportedOSPlatform(OS_WINDOWS)]
		internal static void CloseNative()
		{
			Win32.SetConsoleMode(_stdIn, _oldMode);
			_stdIn   = IntPtr.Zero;
			_stdOut  = IntPtr.Zero;
			_oldMode = 0;
		}

		[SupportedOSPlatform(OS_WINDOWS)]
		internal static void InitNative()
		{
			_stdOut = Win32.GetStdHandle(StandardHandle.STD_OUTPUT_HANDLE);
			_stdIn  = Win32.GetStdHandle(StandardHandle.STD_INPUT_HANDLE);

			if (!Win32.GetConsoleMode(_stdIn, out ConsoleModes mode)) {
				throw new Win32Exception();
			}

			_oldMode = mode;

			mode |= ConsoleModes.ENABLE_MOUSE_INPUT;
			mode &= ~ConsoleModes.ENABLE_QUICK_EDIT_MODE;
			mode |= ConsoleModes.ENABLE_EXTENDED_FLAGS;

			if (!Win32.SetConsoleMode(_stdIn, mode)) {
				throw new Win32Exception();
			}
		}

		[SupportedOSPlatform(OS_WINDOWS)]
		private static void ScrollNative(int nRows)
		{
			var csbiInfo   = new ConsoleScreenBufferInfo();
			var srctWindow = new SmallRect(0, 0, 0, 0);

			// Get the current screen buffer window position.

			if (!Win32.GetConsoleScreenBufferInfo(_stdOut, csbiInfo)) {
				throw new Win32Exception();
			}

			// Check whether the window is too close to the screen buffer top

			if (csbiInfo.srWindow.Top >= nRows) {
				srctWindow.Top    = (short) -nRows; // move top up
				srctWindow.Bottom = (short) -nRows; // move bottom up
				srctWindow.Left   = 0;              // no change
				srctWindow.Right  = 0;              // no change

				if (!Win32.SetConsoleWindowInfo(_stdOut, false, srctWindow)) {
					throw new Win32Exception();
				}

			}
		}
	}
}