using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kantan.Cli.Controls;
using Kantan.Native;
using Kantan.Native.Structures;
using Kantan.Text;
using Kantan.Threading;
using Kantan.Utilities;
using static Kantan.Diagnostics.LogCategories;
using static Kantan.Internal.Common;
using static Kantan.Text.Strings;

// ReSharper disable IdentifierTypo

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable SuggestVarOrType_SimpleTypes

// ReSharper disable SuggestVarOrType_Elsewhere

#pragma warning disable 8601

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

#pragma warning disable 8602, CA1416, CS8604, IDE0059
#nullable enable
#pragma warning disable IDE0051 // Remove unused private members

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
		internal static readonly Color ColorHeader  = Color.Red;
		internal static readonly Color ColorOptions = Color.Aquamarine;
		internal static readonly Color ColorError   = Color.Red;

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

		public static void Init()
		{
			//Console.OutputEncoding = Encoding.Unicode;
		}

		#region IO

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

		public static ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);

		/// <summary>
		///     Determines whether the console buffer contains a file directory that was
		///     input via drag-and-drop.
		/// </summary>
		/// <param name="cki">First character in the buffer</param>
		/// <returns>A valid file directory if the buffer contains one; otherwise, <c>null</c></returns>
		/// <remarks>
		///     This is done heuristically by checking if the first character <paramref name="cki" /> is either a quote or the
		///     primary disk letter. If so, then the rest of the buffer is read until the current sequence is a
		/// string resembling a valid file path.
		/// </remarks>
		internal static string? TryReadFile(ConsoleKeyInfo cki)
		{
			const char quote = '\"';

			var sb = new StringBuilder();

			char keyChar = cki.KeyChar;

			var driveLetters = DriveInfo.GetDrives().Select(x => x.Name.First()).ToArray();

			if (keyChar == quote || driveLetters.Any(e => e == keyChar)) {
				sb.Append(keyChar);

				do {
					ConsoleKeyInfo cki2 = ReadKey(true);

					if (cki2.Key == NC_GLOBAL_EXIT_KEY) {
						return null;
					}

					keyChar = cki2.KeyChar;
					sb.Append(keyChar);

					if (File.Exists(sb.ToString())) {
						break;
					}
				} while (keyChar != quote);

			}

			return sb.ToString().Trim(quote);
		}

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

		#region

		internal static readonly Dictionary<int, ConsoleOption> OptionPositions = new();

		internal static readonly Dictionary<int, ConsoleOptionFunction> OtherPositions = new();

		public static int ScrollIncrement { get; set; } = 3;

		/// <summary>
		///     Exits <see cref="ConsoleDialog.ReadInput" />
		/// </summary>
		public const ConsoleKey NC_GLOBAL_EXIT_KEY = ConsoleKey.Escape;

		/// <summary>
		///     <see cref="Refresh" />
		/// </summary>
		public const ConsoleKey NC_GLOBAL_REFRESH_KEY = ConsoleKey.F5;

		internal static readonly ConsoleKeyInfo ExitKeyInfo =
			new((char) NC_GLOBAL_EXIT_KEY, NC_GLOBAL_EXIT_KEY, false, false, false);

		private const char OPTION_N = 'N';

		private const char OPTION_Y = 'Y';

		/// <summary>
		///     Interface status
		/// </summary>
		internal static ConsoleStatus _status;

		internal enum ConsoleStatus
		{
			/// <summary>
			///     Signals to reload interface
			/// </summary>
			Refresh,

			/// <summary>
			///     Signals to continue displaying current interface
			/// </summary>
			Ok,
		}

		internal enum ConsoleSignal
		{
			Exit
		}

		public static void Refresh() => AtomicHelper.Exchange(ref _status, ConsoleStatus.Refresh);

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

		#endregion

		#region Write

		public static void Print(params object[] args)
		{
			Console.WriteLine(args.QuickJoin());
		}

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

		public static bool Scroll(int increment)
		{
			var newTop = increment + Console.WindowTop;

			var b = !(newTop > Console.BufferHeight) &&
			        !(newTop < 0);

			if (b) {
				// note: Still seems to throw an exception when attempting to scrolling past end ???
				Console.SetWindowPosition(0, newTop);
			}

			return b;
		}

		/// <summary>
		///     Attempts to resize the console window
		/// </summary>
		/// <returns><c>true</c> if the operation succeeded</returns>
		public static bool Resize(int cww, int cwh)
		{
			bool canResize = Console.LargestWindowWidth >= cww &&
			                 Console.LargestWindowHeight >= cwh;

			if (canResize) {
				Console.SetWindowSize(cww, cwh);
			}

			return canResize;
		}

		public static bool InputAvailable
		{
			get
			{
				while (true) {

					if (!Win32.PeekConsoleInput(_stdIn, out InputRecord ir, 1, out uint numEventsRead)) {
						throw new Win32Exception();
					}

					//Debug.WriteLine(ir);

					if (numEventsRead == 0) {
						return false;
					}

					// Skip non key-down && mod key events.

					if (!ir.IsMouseButtonEvent() && (!ir.IsKeyDownEvent() || ir.IsModKey())) {
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

		internal static bool         _click;
		private static  IntPtr       _stdIn;
		private static  IntPtr       _stdOut;
		private static  ConsoleModes _oldMode;


		internal static InputRecord[] _inputBuffer = new InputRecord[1];


		internal static InputRecord ReadInputRecord()
		{
			var record = new InputRecord[1];

			if (!Win32.ReadConsoleInput(_stdIn, record, 1, out uint lpNumberOfEventsRead)) {
				throw new Win32Exception();
			}

			var read = record[0];

			var c = Math.Clamp((int) lpNumberOfEventsRead, 0, _inputBuffer.Length);
			Array.Copy(record, 0, _inputBuffer, 0, c);

			return read;

		}

		/// <summary>
		/// Reads characters from the screen buffer, starting at the given position.
		/// </summary>
		/// <param name="nChars">The number of characters to read.</param>
		/// <param name="x">Column position of the first character to read.</param>
		/// <param name="y">Row position of the first character to read.</param>
		/// <returns>A string containing the characters read from the screen buffer.</returns>
		public static string ReadXY(int nChars, int x, int y)
		{
			char[] buff      = new char[nChars];
			int    charsRead = 0;

			if (!Win32.ReadConsoleOutputCharacter(_stdOut, buff, nChars, new Coord((ushort) x, (ushort) y),
			                                      ref charsRead)) {
				throw new Win32Exception();
			}

			return new string(buff, 0, charsRead);
		}

		/// <summary>
		/// Writes characters to the buffer at the given position.
		/// The cursor position is not updated.
		/// </summary>
		/// <param name="text">The string to be output.</param>
		/// <param name="x">Column position of the starting location.</param>
		/// <param name="y">Row position of the starting location.</param>
		/// <returns></returns>
		public static int WriteXY(string text, int x, int y) => WriteXY(text.ToCharArray(), text.Length, x, y);

		/// <summary>
		/// Writes characters from a character array to the screen buffer at the given cursor position.
		/// </summary>
		/// <param name="text">An array containing the characters to be written.</param>
		/// <param name="nChars">The number of characters to be written.</param>
		/// <param name="x">Column position in which to write the first character.</param>
		/// <param name="y">Row position in which to write the first character.</param>
		/// <returns>Returns the number of characters written.</returns>
		public static int WriteXY(char[] text, int nChars, int x, int y)
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

		internal static ConsoleKeyInfo GetKeyInfoFromRecord(InputRecord ir)
		{

			// We did NOT have a previous keystroke with repeated characters:
			while (true) {

				var keyCode = (VirtualKey) ir.KeyEvent.wVirtualKeyCode;

				// First check for non-keyboard events & discard them. Generally we tap into only KeyDown events and ignore the KeyUp events
				// but it is possible that we are dealing with a Alt+NumPad unicode key sequence, the final unicode char is revealed only when
				// the Alt key is released (i.e when the sequence is complete). To avoid noise, when the Alt key is down, we should eat up
				// any intermediate key strokes (from NumPad) that collectively forms the Unicode character.

				if (!ir.IsKeyDownEvent()) {
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
					if (ir.IsModKey()) {
						continue;
					}
				}

				// When Alt is down, it is possible that we are in the middle of a Alt+NumPad unicode sequence.
				// Escape any intermediate NumPad keys whether NumLock is on or not (notepad behavior)
				var key = (ConsoleKey) keyCode;

				if (ir.IsAltKeyDown() && key is >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9
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

		private static void CloseNative()
		{
			Win32.SetConsoleMode(_stdIn, _oldMode);
			_stdIn   = IntPtr.Zero;
			_stdOut  = IntPtr.Zero;
			_oldMode = 0;
		}

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