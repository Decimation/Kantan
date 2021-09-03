using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kantan.Native;
using Kantan.Text;
using Kantan.Threading;
using Kantan.Utilities;
using static Kantan.Internal.Common;

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

		private static void ClearCurrentLineCR()
		{
			Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
		}

		public static void WriteOnLine(string s, int newTop)
		{
			(int left, int top) = Console.GetCursorPosition();
			ClearLine(newTop);
			Console.WriteLine(s);
			Console.SetCursorPosition(left, top);
		}

		public static void ClearLine(int newTop)
		{
			(int left, int top) = Console.GetCursorPosition();
			Console.SetCursorPosition(0, newTop);
			Console.Write(new string(' ', Console.BufferWidth));
			Console.SetCursorPosition(left, top);

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

		#region Display/formatting

		/// <summary>
		///     Root formatting function.
		/// </summary>
		[StringFormatMethod(STRING_FORMAT_ARG)]
		public static string FormatString(string? delim, string msg, bool repDelim = true)
		{
			string[] split = msg.Split(StringConstants.NativeNewLine);

			bool d1 = false;
			bool dx = delim is null;

			for (int i = 0; i < split.Length; i++) {
				string a = StringConstants.SPACE + split[i];

				string b;

				if (dx) {
					if (i == 0 && split.Length == 2) {
						delim = StringConstants.Horizontal;
					}
					// fixme: WHAT THE FUCK
					else delim = Strings.GetUnicodeBoxPipe(split, i);
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

			if (option.ColorBG.HasValue) {
				name = name.AddColorBG(option.ColorBG.Value);
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

			string f = FormatString(null, sb.ToString(), true);


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

		private static readonly Dictionary<int, NConsoleOption> OptionPositions = new();

		/// <summary>
		///     Display dialog
		/// </summary>
		private static void DisplayDialog(NConsoleDialog dialog, ConsoleOutputResult output)
		{
			Console.Clear();

			if (dialog.Header != null) {
				Write(false, dialog.Header.AddColor(ColorHeader));
			}

			if (dialog.Subtitle != null) {

				string subStr = FormatString(StringConstants.CHEVRON, dialog.Subtitle, false).AddColor(ColorOptions);
				// string subStr = FormatString(null, dialog.Subtitle, true).AddColor(ColorOptions);

				Write(true, subStr);
				Console.WriteLine();
			}

			int clamp = Math.Clamp(dialog.Options.Count, 0, MAX_DISPLAY_OPTIONS);

			for (int i = 0; i < clamp; i++) {
				NConsoleOption? option = dialog.Options[i];

				//string delim = Strings.GetUnicodeBoxPipe(clamp, i);

				string s = FormatOption(option, i);

				OptionPositions[Console.CursorTop] = option;

				Write(false, s);

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

				string optionsStr = $"{StringConstants.CHEVRON} {output.Output.QuickJoin()}"
					.AddColor(ColorOptions);

				Write(true, optionsStr);

				Console.WriteLine();
				Write($"Press {NC_GLOBAL_EXIT_KEY.ToString().AddHighlight()} to save selected values.");
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
		public static async Task<ConsoleOutputResult> ReadInputAsync(NConsoleDialog dialog)
		{
			//var selectedOptions = new HashSet<object>();

			var output = new ConsoleOutputResult { };

			/*
			 * Handle input
			 */

			ConsoleKeyInfo cki;

			ConsoleInterop.Init();
			OptionPositions.Clear();

			do {

				DisplayDialog(dialog, output);

				var task = Task.Run(() =>
				{
					// Block until input is entered.

					int prevCount = dialog.Options.Count;

					while (!ConsoleInterop.InputAvailable) {

						bool refresh = AtomicHelper.Exchange(ref Status, ConsoleStatus.Ok) == ConsoleStatus.Refresh;

						// Refresh buffer if collection was updated

						int currentCount = dialog.Options.Count;

						if (refresh || prevCount != currentCount) {
							DisplayDialog(dialog, output);
							prevCount = currentCount;
						}
					}

					InputRecord ir = ConsoleInterop.ReadInput();

					ConsoleKeyInfo cki2;

					switch (ir.EventType) {

						case ConsoleEventType.KEY_EVENT:
							// Key was read

							cki2 = ConsoleInterop.GetKeyInfoFromRecord(ir);

							var dragAndDropFile = TryReadFile(cki2);

							if (!String.IsNullOrWhiteSpace(dragAndDropFile)) {

								//Debug.WriteLine($">> {dragAndDropFile}");
								output.DragAndDrop = dragAndDropFile;
								return;
							}

							break;
						case ConsoleEventType.MOUSE_EVENT:
							// Mouse was read
							var y = ir.MouseEvent.dwMousePosition.Y;

							if (OptionPositions.ContainsKey(y)) {
								var option  = OptionPositions[y];
								var indexOf = dialog.Options.IndexOf(option);
								var c       = GetDisplayOptionFromIndex(indexOf);

								var me = ir.MouseEvent;

								// note: KeyChar argument is slightly inaccurate
								cki2 = ConsoleInterop.GetKeyInfo(c, c, me.dwControlKeyState);


								// Highlight clicked option
								/*option.Color   = Color.Black;
								option.ColorBG = Color.Yellow;
								DisplayDialog(dialog, output);
								option.Color   = null;
								option.ColorBG = null;
								Thread.Sleep(150);*/
							}
							else {
								goto default;
							}

							break;

						default:
							cki2 = default;
							break;
					}

					//packet.Input = cki2;

					output.Key = cki2;
				});

				await task;

				// Input was read

				// File path was input via drag-and-drop
				if (output.DragAndDrop != null) {
					goto ret;

				}

				if (output.Key.HasValue) {
					cki = output.Key.Value;
				}
				else {
					throw new InvalidOperationException();
				}

				//Debug.WriteLine($"{cki.Key} {cki.KeyChar} | {(int) cki.Key} {(int) cki.KeyChar}");


				// Handle special keys

				if (cki.Key is <= ConsoleKey.F12 and >= ConsoleKey.F1) {
					int i = cki.Key - ConsoleKey.F1;

					if (dialog.Functions is { } && dialog.Functions.ContainsKey(cki.Key)) {
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
							output.Output.Add(funcResult);
						}
						else {
							output.Output = new HashSet<object>() { funcResult };
							goto ret;
						}
					}
				}

			} while (cki.Key != NC_GLOBAL_EXIT_KEY);


			ret:
			return output;
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
		public static ConsoleOutputResult ReadInput(NConsoleDialog dialog)
		{
			var task = ReadInputAsync(dialog);
			task.Wait();
			return task.Result;
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
		private static string? TryReadFile(ConsoleKeyInfo cki)
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

		#region Wait

		public static void WaitForInput()
		{
			Console.WriteLine();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey();
		}

		public static void WaitForTimeSpan(TimeSpan span) => Thread.Sleep(span);

		public static void WaitForSecond() => WaitForTimeSpan(PauseTime);

		#endregion

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
	}

	public class ConsoleOutputResult
	{
		public HashSet<object> Output { get; internal set; }

		public bool SelectMultiple { get; internal set; }

		public string? DragAndDrop { get; internal set; }

		public ConsoleKeyInfo? Key { get; internal set; }

		public ConsoleOutputResult()
		{
			Output = new HashSet<object>();
		}
	}
}