using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Kantan.Internal;
using Kantan.Native;
using Kantan.Native.Structures;
using Kantan.Text;
using Kantan.Threading;
using Kantan.Utilities;
// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault

// ReSharper disable PossibleInvalidOperationException
// ReSharper disable ConditionIsAlwaysTrueOrFalse

// ReSharper disable EmptyConstructor

// ReSharper disable UnusedMember.Global

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Kantan.Cli.Controls
{
	/// <summary>
	///     Displayed in following sequence:
	///     <list type="number">
	///         <item>
	///             <description>
	///                 <see cref="Header" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Subtitle" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Options" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Status" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Description" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	public class ConsoleDialog
	{
		public ConsoleDialog() { }

		/*
		 * Header
		 * Subtitle
		 * Options
		 * Status
		 * Description
		 */


		/// <summary>
		///     The index of an <see cref="ConsoleOption" /> corresponds to its key, which, when pressed,
		///     executes the <see cref="ConsoleOption.Function" /> with the appropriate modifiers
		/// </summary>
		/// <remarks>
		///     <see cref="MAX_DISPLAY_OPTIONS" />
		/// </remarks>
		public IList<ConsoleOption> Options { get; init; }

		public bool SelectMultiple { get; init; }

		[CanBeNull]
		public string Header { get; set; }

		[CanBeNull]
		public string Status { get; set; }

		[CanBeNull]
		public string Description { get; set; }

		[CanBeNull]
		public string Subtitle { get; set; }

		/// <summary>
		///     <c>F*</c> keys
		/// </summary>
		public Dictionary<ConsoleKey, Action> Functions { get; set; } = new();

		/// <summary>
		///     Handles user input and options
		/// </summary>
		/// <remarks>Returns when:
		/// <list type="number">
		/// <item><description><see cref="ConsoleOption.Function" /> returns a non-<c>null</c> value</description></item>
		/// <item><description>A file path is dragged-and-dropped</description></item>
		/// <item><description><see cref="NC_GLOBAL_EXIT_KEY"/> is pressed</description></item>
		/// </list>
		/// </remarks>
		public ConsoleOutputResult ReadInput()
		{
			var task = ReadInputAsync();
			task.Wait();
			return task.Result;
		}

		/// <summary>
		///     Handles user input and options
		/// </summary>
		/// <remarks>Returns when:
		/// <list type="number">
		/// <item><description><see cref="ConsoleOption.Function" /> returns a non-<c>null</c> value</description></item>
		/// <item><description>A file path is dragged-and-dropped</description></item>
		/// <item><description><see cref="NC_GLOBAL_EXIT_KEY"/> is pressed</description></item>
		/// </list>
		/// </remarks>
		public async Task<ConsoleOutputResult> ReadInputAsync()
		{
			//var selectedOptions = new HashSet<object>();
			var output = new ConsoleOutputResult
			{
				SelectMultiple = SelectMultiple,
			};

			/*
			 * Handle input
			 */

			ConsoleKeyInfo cki;

			ConsoleManager.InitNative();
			m_optionPositions.Clear();

			do {

				Display(output);

				var task = Task.Run(() =>
				{
					// Block until input is entered.
					_ReadInput:
					int prevCount = Options.Count;

					while (!ConsoleManager.InputAvailable) {

						bool refresh =
							AtomicHelper.Exchange(ref m_status, ConsoleStatus.Ok) ==
							ConsoleStatus.Refresh;

						// Refresh buffer if collection was updated

						int currentCount = Options.Count;

						if (refresh || prevCount != currentCount) {
							Display(output);
							prevCount = currentCount;
						}
					}

					InputRecord ir = ConsoleManager.ReadInputRecord();

					ConsoleKeyInfo cki2;

					MouseEventRecord me = ir.MouseEvent;

					switch (ir.EventType) {

						case ConsoleEventType.KEY_EVENT:
							// Key was read

							cki2 = ConsoleManager.GetKeyInfoFromRecord(ir);
							var dragAndDropFile = TryReadFile(cki2);

							if (!String.IsNullOrWhiteSpace(dragAndDropFile)) {

								Debug.WriteLine($">> {dragAndDropFile}");
								output.DragAndDrop = dragAndDropFile;
								return;
							}

							break;
						case ConsoleEventType.MOUSE_EVENT when ConsoleManager.IsMouseScroll(ir):

							bool scrollDown = me.dwButtonState.HasFlag(ButtonState.SCROLL_DOWN);

							var increment =
								scrollDown ? ConsoleManager.ScrollIncrement : -ConsoleManager.ScrollIncrement;

							bool b = ConsoleManager.Scroll(increment);

							goto _ReadInput;
						case ConsoleEventType.MOUSE_EVENT:
							// Mouse was read
							var (x, y) = (me.dwMousePosition.X, me.dwMousePosition.Y);

							// var vs = ReadXY(Console.BufferWidth, 0, y);
							// Debug.WriteLine($"{vs}");

							// hack: wtf
							if (me.dwButtonState == ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
								ConsoleManager._click = true;
							}

							if (ConsoleManager._click && me.dwButtonState == 0) {
								goto default;
							}

							if (m_optionPositions.ContainsKey(y)) {
								var  option  = m_optionPositions[y];
								int  indexOf = Options.IndexOf(option);
								char c       = GetDisplayOptionFromIndex(indexOf);

								// note: KeyChar argument is slightly inaccurate (case insensitive; always uppercase)
								cki2 = ConsoleManager.GetKeyInfo(c, c, me.dwControlKeyState);
								// Thread.Sleep(1000);
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
					goto _Return;
				}

				if (output.Key.HasValue) {
					cki = output.Key.Value;
				}
				else {
					throw new InvalidOperationException();
				}

				Debug.WriteLine($"{nameof(ConsoleManager)}: ({cki.Key} {(int) cki.Key}) " +
				                $"| ({cki.KeyChar} {(int) cki.KeyChar})", LogCategories.C_DEBUG);

				// Handle special keys

				if (cki.Key is <= ConsoleKey.F12 and >= ConsoleKey.F1) {
					int i = cki.Key - ConsoleKey.F1;

					if (Functions is { } && Functions.ContainsKey(cki.Key)) {
						Action function = Functions[cki.Key];
						function();
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

				if (idx < Options.Count && idx >= 0) {
					var option = Options[idx];

					if (!option.Functions.ContainsKey(modifiers)) {
						continue;
					}

					var fn = option.Functions[modifiers];

					object funcResult = fn();

					if (funcResult != null) {
						//
						if (SelectMultiple) {
							output.Output.Add(funcResult);
						}
						else {
							output.Output = new HashSet<object> { funcResult };
							goto _Return;
						}
					}
				}

			} while (cki.Key != NC_GLOBAL_EXIT_KEY);

			_Return:
			return output;
		}

		/// <summary>
		///     Display dialog
		/// </summary>
		private void Display(ConsoleOutputResult output)
		{
			Display();

			// Show options

			if (SelectMultiple) {
				Console.WriteLine();

				// sb.AppendLine();

				string optionsStr = $"{Strings.Constants.CHEVRON} {output.Output.QuickJoin()}"
					.AddColor(ConsoleManager.ColorOptions);

				ConsoleManager.Write(true, optionsStr);

				// sb.AppendLine(optionsStr).AppendLine();

				Console.WriteLine();

				ConsoleManager.Write(
					$"Press {NC_GLOBAL_EXIT_KEY.ToString().AddHighlight()} to save selected values.");

				// sb.AppendLine($"Press {NC_GLOBAL_EXIT_KEY.ToString().AddHighlight()} to save selected values.");
			}
			// Console.Write(sb);
		}

		public void Display(bool clear = true)
		{
			// todo: atomic write operations (i.e., instead of incremental)

			if (clear) {
				Console.Clear();
			}

			// var t = Console.CursorTop;
			// Console.SetCursorPosition(0,0);
			// var sb = new StringBuilder();

			if (Header != null) {
				ConsoleManager.Write(false, Header.AddColor(ConsoleManager.ColorHeader));
				// sb.Append(Header.AddColor(ConsoleManager.ColorHeader));
			}

			if (Subtitle != null) {

				string subStr = ConsoleManager.FormatString(Strings.Constants.CHEVRON, Subtitle, false)
				                              .AddColor(ConsoleManager.ColorOptions);

				// string subStr = FormatString(null, Subtitle, true).AddColor(ColorOptions);

				ConsoleManager.Write(true, subStr);
				Console.WriteLine();
				// sb.AppendLine(subStr).AppendLine();
			}

			int clamp = Math.Clamp(Options.Count, 0, MAX_DISPLAY_OPTIONS);

			for (int i = 0; i < clamp; i++) {
				ConsoleOption option = Options[i];

				//string delim = Strings.GetUnicodeBoxPipe(clamp, i);

				string s = FormatOption(option, i);

				var top = Console.CursorTop;
				m_optionPositions[top] = option;

				// t+=MeasureRows(s)+(i-1);
				// Debug.WriteLine($"{top} | {((Strings.MeasureRows(sb.ToString())))} | {Strings.MeasureRows(s)} | {Strings.MeasureRows(sb.ToString())-Strings.MeasureRows(s)-i} | {i} |");
				// Debug.WriteLine(Console.CursorTop);
				// sb.Append(s);
				ConsoleManager.Write(false, s);


				// Write(false, s);
				/*sb.Append(s);
				var rows      = Strings.MeasureRows(sb.ToString())+sb.ToString().Count(c=>c=='\n');
				var key = Console.CursorTop+rows;
				OptionPositions[key] = option;*/
			}

			Console.WriteLine();
			// sb.AppendLine();

			if (Status != null) {
				ConsoleManager.Write(Status);
				// sb.AppendLine(Status);
			}

			if (Description != null) {
				Console.WriteLine();

				ConsoleManager.Write(Description);
				// sb.AppendLine().AppendLine(Description);
			}
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

			return Common.INVALID;
		}

		private static string FormatOption(ConsoleOption option, int i)
		{
			var  sb = new StringBuilder();
			char c  = GetDisplayOptionFromIndex(i);

			string name = option.Name;


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

			if (!sb.ToString().EndsWith(Strings.Constants.NEW_LINE)) {
				sb.AppendLine();
			}

			string f = ConsoleManager.FormatString(null, sb.ToString());

			return f;
		}

		private const int  MAX_OPTION_N        = 10;
		private const char OPTION_LETTER_START = 'A';
		private const int  MAX_DISPLAY_OPTIONS = 36;

		/// <summary>
		///     Exits <see cref="ConsoleDialog.ReadInput" />
		/// </summary>
		public const ConsoleKey NC_GLOBAL_EXIT_KEY = ConsoleKey.Escape;

		/// <summary>
		///     <see cref="Refresh" />
		/// </summary>
		public const ConsoleKey NC_GLOBAL_REFRESH_KEY = ConsoleKey.F5;

		private readonly Dictionary<int, ConsoleOption> m_optionPositions = new();

		/// <summary>
		///     Interface status
		/// </summary>
		private ConsoleStatus m_status;

		private enum ConsoleStatus
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

		public void Refresh() => AtomicHelper.Exchange(ref m_status, ConsoleStatus.Refresh);

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
		internal static string TryReadFile(ConsoleKeyInfo cki)
		{
			const char quote = '\"';

			var sb = new StringBuilder();

			char keyChar = cki.KeyChar;

			var driveLetters = DriveInfo.GetDrives().Select(x => x.Name.First()).ToArray();

			if (keyChar == quote || driveLetters.Any(e => e == keyChar)) {
				sb.Append(keyChar);

				do {
					ConsoleKeyInfo cki2 = ConsoleManager.ReadKey(true);

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

		public override int GetHashCode()
		{
			var h = new HashCode();

			IEnumerable<int> hx = Options.Select(o => o.GetHashCode());

			foreach (int i in hx) {
				h.Add(i);
			}

			h.Add(Status?.GetHashCode());
			h.Add(Description?.GetHashCode());
			h.Add(Header?.GetHashCode());
			h.Add(Subtitle?.GetHashCode());

			return h.ToHashCode();
		}
	}
}