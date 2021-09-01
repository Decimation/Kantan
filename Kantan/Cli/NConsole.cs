using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kantan.Text;
using Kantan.Threading;
using Kantan.Utilities;
using Microsoft.Win32.SafeHandles;
using static Kantan.Internal.Common;
using Console = System.Console;

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

namespace Kantan.Cli
{
	/// <summary>
	///     Extended console.
	/// </summary>
	/// <list type="bullet">
	///     <item>
	///         <description>
	///             <see cref="NConsole" />
	///         </description>
	///     </item>
	///     <item>
	///         <description>
	///             <see cref="NConsoleOption" />
	///         </description>
	///     </item>
	///     <item>
	///         <description>
	///             <see cref="NConsoleDialog" />
	///         </description>
	///     </item>
	///     <item>
	///         <description>
	///             <see cref="NConsoleProgress" />
	///         </description>
	///     </item>
	/// </list>
	public static class NConsole
	{
		private static readonly Color ColorHeader  = Color.Red;
		private static readonly Color ColorOptions = Color.Aquamarine;
		private static readonly Color ColorError   = Color.Red;

		public static int BufferLimit { get; set; } = Console.BufferWidth - 10;

		private static TimeSpan PauseTime { get; set; } = TimeSpan.FromSeconds(1);

		/*
		 * https://github.com/Decimation/SimpleCore/blob/2d6009cfc498de07d5f507192c3cbe1983ff1a11/SimpleCore.Cli/NConsole.cs
		 * https://gist.github.com/ZacharyPatten/798ed612d692a560bdd529367b6a7dbd
		 * https://github.com/ZacharyPatten/Towel
		 */

		/*
		 * TODO: this design isn't great
		 */

		public static void Init()
		{
			//Console.OutputEncoding = Encoding.Unicode;
			//ListenThread.Start();

			//ThreadPool.QueueUserWorkItem(KeyWatch);
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

		#region Write

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

		public static void ClearCurrentLine()
		{
			Console.SetCursorPosition(0, Console.CursorTop - 1);

			int top = Console.CursorTop;
			Console.SetCursorPosition(0, Console.CursorTop);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(0, top);
		}

		public static void ClearLastLine()
		{
			var top = Console.CursorTop;
			Console.SetCursorPosition(0, top - 1);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(0, top);
		}

		#endregion

		#region IO

		#region Keys

		/// <summary>
		///     Exits <see cref="ReadInput" />
		/// </summary>
		public const ConsoleKey NC_GLOBAL_EXIT_KEY = ConsoleKey.Escape;

		/// <summary>
		///     <see cref="Refresh" />
		/// </summary>
		public const ConsoleKey NC_GLOBAL_REFRESH_KEY = ConsoleKey.F5;

		#endregion Keys


		#region Display/formatting

		/// <summary>
		///     Root formatting function.
		/// </summary>
		[StringFormatMethod(STRING_FORMAT_ARG)]
		public static string FormatString(string delim, string msg, bool repDelim = true)
		{
			string[] split = msg.Split(StringConstants.NativeNewLine);

			bool d1 = false;

			for (int i = 0; i < split.Length; i++) {
				string a = StringConstants.SPACE + split[i];

				string b;

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
					c += StringConstants.ELLIPSES;
				}

				split[i] = c;
			}

			return String.Join(StringConstants.NativeNewLine, split);
		}

		private static string FormatOption(NConsoleOption option, int i)
		{
			var  sb = new StringBuilder();
			char c  = GetDisplayOptionFromIndex(i);

			string? name = option.Name;

			if (option.Color.HasValue) {
				name = name.AddColor(option.Color.Value);
			}

			sb.Append($"[{c}]: ");

			if (name != null) {
				sb.Append($"{name} ");
			}

			if (option.Data != null) {

				sb.AppendLine();

				sb.Append($"{Strings.Indent(Strings.OutlineString(option.Data))}");
			}

			if (!sb.ToString().EndsWith(StringConstants.NativeNewLine)) {
				sb.AppendLine();
			}

			string f = FormatString(StringConstants.ASTERISK.ToString(), sb.ToString());


			return f;
		}

		private static char GetDisplayOptionFromIndex(int i)
		{
			if (i < MAX_OPTION_N) {
				return Char.Parse(i.ToString());
			}

			int d = OPTION_LETTER_START + (i - MAX_OPTION_N);

			return (char) d;
		}

		private static int GetIndexFromDisplayOption(char c)
		{
			if (Char.IsNumber(c)) {
				return (int) Char.GetNumericValue(c);
			}

			if (Char.IsLetter(c)) {
				c = Char.ToUpper(c);
				return MAX_OPTION_N + (c - OPTION_LETTER_START);
			}

			return INVALID;
		}

		public static Dictionary<int, NConsoleOption> pos = new();

		/// <summary>
		///     Display dialog
		/// </summary>
		private static void DisplayDialog(NConsoleDialog dialog, HashSet<object> selectedOptions)
		{
			Console.Clear();

			if (dialog.Header != null) {
				Write(false, dialog.Header.AddColor(ColorHeader));
			}

			if (dialog.Subtitle != null) {

				string subStr = FormatString(StringConstants.CHEVRON, dialog.Subtitle, false).AddColor(ColorOptions);

				Write(true, subStr);
				Console.WriteLine();
			}

			int clamp = Math.Clamp(dialog.Options.Count, 0, MAX_DISPLAY_OPTIONS);

			for (int i = 0; i < clamp; i++) {
				NConsoleOption? option = dialog.Options[i];

				string s = FormatOption(option, i);

				Write(false, s);

				//todo
				pos[Console.CursorTop - 1] = option;
			}

			Console.WriteLine();

			if (dialog.Status != null) {
				Write(dialog.Status);
			}

			if (dialog.Description != null) {
				Console.WriteLine();

				Write(dialog.Description);
			}

			// Show options
			if (dialog.SelectMultiple) {
				Console.WriteLine();

				string optionsStr = $"{StringConstants.CHEVRON} {selectedOptions.QuickJoin()}".AddColor(ColorOptions);

				Write(true, optionsStr);
			}

			if (dialog.SelectMultiple) {
				Console.WriteLine();
				Write($"Press {NC_GLOBAL_EXIT_KEY.ToString().AddUnderline()} to save selected values.");
			}
		}

		#endregion

		/// <summary>
		///     Handles user input and options
		/// </summary>
		/// <remarks>Returns when:
		/// <list type="number">
		/// <item><description><see cref="NConsoleOption.Function" /> returns a non-<c>null</c> value</description></item>
		/// <item><description>A file path is dragged-and-dropped</description></item>
		/// <item><description><see cref="NC_GLOBAL_EXIT_KEY"/> is pressed</description></item>
		/// </list>
		/// </remarks>
		public static HashSet<object> ReadInput(NConsoleDialog dialog)
		{
			Task<HashSet<object>> task = ReadInputAsync(dialog);
			task.Wait();
			return task.Result;
		}

		/// <summary>
		///     Handles user input and options
		/// </summary>
		/// <remarks>Returns when:
		/// <list type="number">
		/// <item><description><see cref="NConsoleOption.Function" /> returns a non-<c>null</c> value</description></item>
		/// <item><description>A file path is dragged-and-dropped</description></item>
		/// <item><description><see cref="NC_GLOBAL_EXIT_KEY"/> is pressed</description></item>
		/// </list>
		/// </remarks>
		public static async Task<HashSet<object>> ReadInputAsync(NConsoleDialog dialog)
		{
			var selectedOptions = new HashSet<object>();

			/*
			 * Handle input
			 */

			ConsoleKeyInfo cki;

			NConsoleOption? click           = null;
			string?         dragAndDropFile = null;
			bool            k               = false;

			pos.Clear();

			do {
				DisplayDialog(dialog, selectedOptions);

				var t = Task.Run<ConsoleKeyInfo?>(() =>
				{
					// Block until input is entered.

					int prevCount = dialog.Options.Count;


					while (!Console.KeyAvailable) {

						bool refresh = AtomicHelper.Exchange(ref Status, ConsoleStatus.Ok) == ConsoleStatus.Refresh;

						// Refresh buffer if collection was updated

						int currentCount = dialog.Options.Count;

						if (refresh || prevCount != currentCount) {

							DisplayDialog(dialog, selectedOptions);
							prevCount = currentCount;
						}

						var v = NativeMethods.Run();

						if (!v.Equals(default) && v.EventType == NativeMethods.MOUSE_EVENT &&
						    v.MouseEvent.dwButtonState == 0x1) {
							Debug.WriteLine("click");

							if (NConsole.pos.ContainsKey(v.MouseEvent.dwMousePosition.Y)) {
								var po = NConsole.pos[v.MouseEvent.dwMousePosition.Y];
								click = po;
								k     = false;
								break;
							}
						}

						else {
							k = true;
						}

					}

					ConsoleKeyInfo cki2;

					if (click is { } && !k) {
						var i = GetDisplayOptionFromIndex(dialog.Options.IndexOf(click));

						cki2 = new ConsoleKeyInfo(i, (ConsoleKey) i, false, false, false) { };
					}
					else {
						// Key was read

						cki2 = Console.ReadKey(true);

						dragAndDropFile = ListenForFile(cki2);

						if (!String.IsNullOrWhiteSpace(dragAndDropFile)) {

							Debug.WriteLine($">> {dragAndDropFile}");
							return null;
						}
					}


					//Debug.WriteLine($"{cki2.Key} | {cki2.KeyChar}");

					return cki2;
				});

				await t;

				// File path was input via drag-and-drop
				if (!t.Result.HasValue) {
					return new HashSet<object> { dragAndDropFile };
				}

				// Key was read

				cki = t.Result.Value;

				// Handle special keys

				if (cki.Key is <= ConsoleKey.F12 and >= ConsoleKey.F1) {
					int i = cki.Key - ConsoleKey.F1;

					if (dialog.Functions is { } && dialog.Functions.ContainsKey(cki.Key)) {
						/*if (dialog.Functions.Length > i && i >= 0) {
							dialog.Functions[i]();
						}*/
						dialog.Functions[cki.Key]();
					}
				}

				switch (cki.Key) {
					case NC_GLOBAL_REFRESH_KEY:
						Refresh();
						break;
				}

				// KeyChar can't be used as modifiers are not applicable
				char keyChar = (char) (int) cki.Key;

				if (!Char.IsLetterOrDigit(keyChar)) {
					continue;
				}

				ConsoleModifiers modifiers = cki.Modifiers;

				// Handle option

				int idx = GetIndexFromDisplayOption(keyChar);

				if (idx < dialog.Options.Count && idx >= 0) {
					var option = dialog.Options[idx];

					if (!option.Functions.ContainsKey(modifiers)) {
						continue;
					}

					var fn = option.Functions[modifiers];

					object? funcResult = fn();

					if (funcResult != null) {
						//
						if (dialog.SelectMultiple) {
							selectedOptions.Add(funcResult);
						}
						else {
							return new HashSet<object> { funcResult };
						}
					}
				}
			} while (cki.Key != NC_GLOBAL_EXIT_KEY);

			return selectedOptions;
		}

		public static string ReadLine(string? prompt = null, Predicate<string>? invalid = null,
		                              string? errPrompt = null)
		{
			invalid ??= String.IsNullOrWhiteSpace;

			string? input;
			bool    isInvalid;

			do {
				//https://stackoverflow.com/questions/8946808/can-console-clear-be-used-to-only-clear-a-line-instead-of-whole-console

				Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");

				if (prompt != null) {
					string str = $">> {prompt}: ".AddColor(ColorOptions);

					Console.Write(str);
				}

				input     = Console.ReadLine();
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
			Write($"{StringConstants.ASTERISK} {String.Format(msg, args)} ({OPTION_Y}/{OPTION_N}): ");

			char key = Char.ToUpper(Console.ReadKey().KeyChar);

			Console.WriteLine();

			return key switch
			{
				OPTION_N => false,
				OPTION_Y => true,
				_        => ReadConfirmation(msg, args)
			};
		}

		public static void Refresh() => AtomicHelper.Exchange(ref Status, ConsoleStatus.Refresh);

		public static void WaitForInput()
		{
			Console.WriteLine();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}

		public static void WaitForTimeSpan(TimeSpan span) => Thread.Sleep(span);

		public static void WaitForSecond() => WaitForTimeSpan(PauseTime);

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
		private static string? ListenForFile(ConsoleKeyInfo cki)
		{
			const char quote = '\"';

			var sb = new StringBuilder();

			char keyChar = cki.KeyChar;

			var driveLetters = DriveInfo.GetDrives().Select(x => x.Name.First()).ToArray();

			if (keyChar == quote || driveLetters.Any(e => e == keyChar)) {
				sb.Append(keyChar);

				do {
					ConsoleKeyInfo cki2 = Console.ReadKey(true);

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

		#region Status

		/// <summary>
		///     Interface status
		/// </summary>
		private static ConsoleStatus Status;

		private enum ConsoleStatus
		{
			/// <summary>
			///     Signals to reload interface
			/// </summary>
			Refresh,

			/// <summary>
			///     Signals to continue displaying current interface
			/// </summary>
			Ok
		}

		#endregion

		#region Options

		public const char OPTION_N = 'N';

		public const char OPTION_Y = 'Y';

		private const int MAX_OPTION_N = 10;

		private const char OPTION_LETTER_START = 'A';

		public const int MAX_DISPLAY_OPTIONS = 36;

		#endregion Options

		#endregion IO

		/*
		 * https://github.com/sindresorhus/cli-spinners/blob/main/spinners.json
		 * https://github.com/sindresorhus/cli-spinners
		 * https://www.npmjs.com/package/cli-spinners
		 */

		public static void Print(params object[] args)
		{
			Console.WriteLine(args.QuickJoin());
		}
	}


	// TODO
	internal static class NativeMethods
	{
		internal static IntPtr _getStdHandle;
		

		internal static INPUT_RECORD Run()
		{
			_getStdHandle = GetStdHandle(STD_INPUT_HANDLE);
			int mode = 0;

			if (!(GetConsoleMode(_getStdHandle, ref mode))) {
				throw new Win32Exception();
			}
			

			mode |= ENABLE_MOUSE_INPUT;
			mode &= ~ENABLE_QUICK_EDIT_MODE;
			mode |= ENABLE_EXTENDED_FLAGS;

			if (!(SetConsoleMode(_getStdHandle, mode))) {
				throw new Win32Exception();
			}

			var  record    = new INPUT_RECORD();
			uint recordLen = 0;

			if (!(ReadConsoleInput(_getStdHandle, ref record, 1, ref recordLen))) {
				throw new Win32Exception();
			}

			//Console.SetCursorPosition(0, 0);

			switch (record.EventType) {
				case MOUSE_EVENT:
				{
					/*Console.WriteLine("Mouse event");

						Console.WriteLine($"    X ...............:   {record.MouseEvent.dwMousePosition.X,4:0}  ");

						Console.WriteLine($"    Y ...............:   {record.MouseEvent.dwMousePosition.Y,4:0}  ");

						Console.WriteLine($"    dwButtonState ...: 0x{record.MouseEvent.dwButtonState:X4}  ");

						Console.WriteLine($"    dwControlKeyState: 0x{record.MouseEvent.dwControlKeyState:X4}  ");

						Console.WriteLine($"    dwEventFlags ....: 0x{record.MouseEvent.dwEventFlags:X4}  ");*/


					goto ret;

				}
					break;

				/*case KEY_EVENT:
					{
						/*Console.WriteLine("Key event  ");
						Console.WriteLine($"    bKeyDown  .......:  {record.KeyEvent.bKeyDown,5}  ");

						Console.WriteLine($"    wRepeatCount ....:   {record.KeyEvent.wRepeatCount,4:0}  ");

						Console.WriteLine($"    wVirtualKeyCode .:   {record.KeyEvent.wVirtualKeyCode,4:0}  ");

						Console.WriteLine($"    uChar ...........:      {record.KeyEvent.UnicodeChar}  ");

						Console.WriteLine($"    dwControlKeyState: 0x{record.KeyEvent.dwControlKeyState:X4}  ");#1#

						if (record.KeyEvent.wVirtualKeyCode == (int) ConsoleKey.Escape) { return; }
					}
						break;*/
			}

			ret:
			WriteConsoleInput(_getStdHandle, new[] { record }, 1, out recordLen);
			return record;

		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern bool WriteConsoleInput(IntPtr hConsoleInput, INPUT_RECORD[] lpBuffer, uint nLength,
		                                             out uint lpNumberOfEventsWritten);

		public const int STD_INPUT_HANDLE = -10;

		public const int ENABLE_MOUSE_INPUT     = 0x0010;
		public const int ENABLE_QUICK_EDIT_MODE = 0x0040;
		public const int ENABLE_EXTENDED_FLAGS  = 0x0080;

		public const int KEY_EVENT   = 1;
		public const int MOUSE_EVENT = 2;


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

		[DebuggerDisplay("{dwMousePosition.X}, {dwMousePosition.Y}")]
		internal struct MOUSE_EVENT_RECORD
		{
			public COORD dwMousePosition;
			public int   dwButtonState;
			public int   dwControlKeyState;
			public int   dwEventFlags;
		}

		[DebuggerDisplay("{X}, {Y}")]
		internal struct COORD
		{
			public ushort X;
			public ushort Y;
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


		public class ConsoleHandle : SafeHandleMinusOneIsInvalid
		{
			public ConsoleHandle() : base(false) { }

			protected override bool ReleaseHandle()
			{
				return true; //releasing console handle is not our business
			}
		}


		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, ref int lpMode);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ReadConsoleInput(IntPtr hConsoleInput, ref INPUT_RECORD lpBuffer,
		                                             uint nLength, ref uint lpNumberOfEventsRead);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);
	}
}