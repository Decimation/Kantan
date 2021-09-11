#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Kantan.Internal;
using Kantan.Native.Structures;
using Kantan.Text;
using Kantan.Threading;
using Kantan.Utilities;

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
		///     <see cref="ConsoleManager.MAX_DISPLAY_OPTIONS" />
		/// </remarks>
		public IList<ConsoleOption> Options { get; init; }

		public bool SelectMultiple { get; init; }

		public string? Header { get; set; }

		public string? Status { get; set; }

		public string? Description { get; set; }

		public string? Subtitle { get; set; }

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
		/// <item><description><see cref="ConsoleManager.NC_GLOBAL_EXIT_KEY"/> is pressed</description></item>
		/// </list>
		/// </remarks>
		public ConsoleOutputResult Read()
		{
			var task = ReadAsync();
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
		/// <item><description><see cref="ConsoleManager.NC_GLOBAL_EXIT_KEY"/> is pressed</description></item>
		/// </list>
		/// </remarks>
		public async Task<ConsoleOutputResult> ReadAsync()
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
			ConsoleManager.OptionPositions.Clear();

			do {

				DisplayDialog(output);

				var task = Task.Run(() =>
				{
					// Block until input is entered.
					_ReadInput:
					int prevCount = Options.Count;

					while (!ConsoleManager.InputAvailable) {

						bool refresh =
							AtomicHelper.Exchange(ref ConsoleManager._status, ConsoleManager.ConsoleStatus.Ok) ==
							ConsoleManager.ConsoleStatus.Refresh;

						// Refresh buffer if collection was updated

						int currentCount = Options.Count;

						if (refresh || prevCount != currentCount) {
							DisplayDialog(output);
							prevCount = currentCount;
						}
					}

					InputRecord ir = ConsoleManager.ReadInputRecord();

					ConsoleKeyInfo cki2;

					switch (ir.EventType) {

						case ConsoleEventType.KEY_EVENT:
							// Key was read

							cki2 = ConsoleManager.GetKeyInfoFromRecord(ir);
							var dragAndDropFile = ConsoleManager.TryReadFile(cki2);

							if (!String.IsNullOrWhiteSpace(dragAndDropFile)) {

								//Debug.WriteLine($">> {dragAndDropFile}");
								output.DragAndDrop = dragAndDropFile;
								return;
							}

							break;
						case ConsoleEventType.MOUSE_EVENT when InputRecord.IsMouseScroll(ir):

							bool scrollDown = ir.MouseEvent.dwButtonState.HasFlag(ButtonState.SCROLL_DOWN);

							var increment =
								scrollDown ? ConsoleManager.ScrollIncrement : -ConsoleManager.ScrollIncrement;

							bool b = ConsoleManager.Scroll(increment);

							goto _ReadInput;
						case ConsoleEventType.MOUSE_EVENT:
							// Mouse was read
							var (x, y) = (ir.MouseEvent.dwMousePosition.X, ir.MouseEvent.dwMousePosition.Y);

							// var vs = ReadXY(Console.BufferWidth, 0, y);
							// Debug.WriteLine($"{vs}");

							if (ConsoleManager.OptionPositions.ContainsKey(y)) {
								var option  = ConsoleManager.OptionPositions[y];
								var indexOf = Options.IndexOf(option);
								var c       = GetDisplayOptionFromIndex(indexOf);

								var me = ir.MouseEvent;

								// note: KeyChar argument is slightly inaccurate (case insensitive; always uppercase)
								cki2 = ConsoleManager.GetKeyInfo(c, c, me.dwControlKeyState);

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
						var function = Functions[cki.Key];
						function();
					}
				}

				switch (cki.Key) {
					case ConsoleManager.NC_GLOBAL_REFRESH_KEY:
						ConsoleManager.Refresh();
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

					object? funcResult = fn();

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

			} while (cki.Key != ConsoleManager.NC_GLOBAL_EXIT_KEY);

			_Return:
			return output;
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


		/// <summary>
		///     Display dialog
		/// </summary>
		private void DisplayDialog(ConsoleOutputResult output)
		{
			// todo: atomic write operations (i.e., instead of incremental)

			Console.Clear();

			var t = Console.CursorTop;
			// Console.SetCursorPosition(0,0);
			var sb = new StringBuilder();

			if (Header != null) {
				ConsoleManager.Write(false, Header.AddColor(ConsoleManager.ColorHeader));
				sb.Append(Header.AddColor(ConsoleManager.ColorHeader));
			}

			if (Subtitle != null) {

				string subStr = ConsoleManager.FormatString(Strings.Constants.CHEVRON, Subtitle, false)
				                              .AddColor(ConsoleManager.ColorOptions);
				// string subStr = FormatString(null, Subtitle, true).AddColor(ColorOptions);

				ConsoleManager.Write(true, subStr);
				Console.WriteLine();
				sb.AppendLine(subStr).AppendLine();
			}

			int clamp = Math.Clamp(Options.Count, 0, MAX_DISPLAY_OPTIONS);

			for (int i = 0; i < clamp; i++) {
				ConsoleOption option = Options[i];

				//string delim = Strings.GetUnicodeBoxPipe(clamp, i);
				char c = GetDisplayOptionFromIndex(i);

				var sb1 = new StringBuilder();
				sb1.Append($"[{c}]: ");

				string sb11 = option.FormatOption();
				sb1.Append(sb11);
				string s = ConsoleManager.FormatString(null, sb1.ToString());


				var top = Console.CursorTop;
				ConsoleManager.OptionPositions[top] = option;
				// t+=MeasureRows(s)+(i-1);

				// Debug.WriteLine($"{top} | {((Strings.MeasureRows(sb.ToString())))} | {Strings.MeasureRows(s)} | {i}");

				// Debug.WriteLine(Console.CursorTop);
				sb.Append(s);
				ConsoleManager.Write(false, s);


				// Write(false, s);
				/*sb.Append(s);
				var rows      = Strings.MeasureRows(sb.ToString())+sb.ToString().Count(c=>c=='\n');
				var key = Console.CursorTop+rows;
				OptionPositions[key] = option;*/
			}

			Console.WriteLine();
			sb.AppendLine();

			if (Status != null) {
				ConsoleManager.Write(Status);
				sb.AppendLine(Status);
			}

			if (Description != null) {
				Console.WriteLine();

				ConsoleManager.Write(Description);
				sb.AppendLine().AppendLine(Description);
			}

			// Show options

			if (SelectMultiple) {
				Console.WriteLine();

				sb.AppendLine();

				string optionsStr = $"{Strings.Constants.CHEVRON} {output.Output.QuickJoin()}"
					.AddColor(ConsoleManager.ColorOptions);

				ConsoleManager.Write(true, optionsStr);

				sb.AppendLine(optionsStr).AppendLine();

				Console.WriteLine();

				ConsoleManager.Write(
					$"Press {ConsoleManager.NC_GLOBAL_EXIT_KEY.ToString().AddHighlight()} to save selected values.");

				sb.AppendLine(
					$"Press {ConsoleManager.NC_GLOBAL_EXIT_KEY.ToString().AddHighlight()} to save selected values.");
			}
			// Console.Write(sb);
		}

		private const int  MAX_OPTION_N        = 10;
		private const char OPTION_LETTER_START = 'A';
		private const int  MAX_DISPLAY_OPTIONS = 36;
	}
}