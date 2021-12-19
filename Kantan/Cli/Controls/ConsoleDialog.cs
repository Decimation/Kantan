global using SC = Kantan.Text.Strings.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Kantan.Internal;
using Kantan.OS.Structures;
using Kantan.Text;
using Kantan.Utilities;

// ReSharper disable SuggestVarOrType_DeconstructionDeclarations

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault
// ReSharper disable PossibleInvalidOperationException
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable EmptyConstructor
// ReSharper disable UnusedMember.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Kantan.Cli.Controls;

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
	///     <see cref="ConsoleOption.MAX_DISPLAY_OPTIONS" />
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

	private readonly Dictionary<int, ConsoleOption> m_optionPositions = new();

	/// <summary>
	///     Interface status
	/// </summary>
	private ConsoleStatus m_status;


	private static readonly string SelectMultiplePrompt =
		$"Press {NC_GLOBAL_EXIT_KEY.ToString().AddHighlight()} to save selected values.";

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

		ConsoleManager.Init();

		m_optionPositions.Clear();

		do {

			Display(output);

			var inputTask = Task.Run(() => InputTask(output));

			await inputTask;

			// Input was read

			// File path was input via drag-and-drop
			if (output.DragAndDrop != null) {
				goto _Return;
			}

			if (output.Key.HasValue) {
				cki = output.Key.Value;
			}
			else {
				cki = default;
				continue;

				// throw new InvalidOperationException();
			}

			/*Debug.WriteLine($"{nameof(ConsoleManager)}: ({cki.Key} {(int) cki.Key}) " +
			                $"| ({cki.KeyChar} {(int) cki.KeyChar})", LogCategories.C_DEBUG);*/

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

			int idx = ConsoleOption.GetIndexFromDisplayOption(keyChar);

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

	private void InputTask(ConsoleOutputResult output)
	{
		// Block until input is entered.

		_ReadInput:
		int prevCount = Options.Count;

		while (!ConsoleManager.InputAvailable) {
			unsafe {
				var ptr = (int*) Unsafe.AsPointer(ref m_status);

				bool refresh = Interlocked.Exchange(ref Unsafe.AsRef<int>(ptr), (int) ConsoleStatus.Ok) ==
				               (int) ConsoleStatus.Refresh;

				// Refresh buffer if collection was updated

				int currentCount = Options.Count;

				if ((refresh || prevCount != currentCount)) {
					Debug.WriteLine("update", nameof(ConsoleDialog));
					Display(output);
					prevCount = currentCount;
				}
			}

		}

		InputRecord ir = ConsoleManager.ReadInput();

		ConsoleKeyInfo cki;

		MouseEventRecord me = ir.MouseEvent;

		switch (ir.EventType) {

			case ConsoleEventType.KEY_EVENT:
				// Key was read

				cki = ir.ToConsoleKeyInfo();
				string dragAndDropFile = TryReadFile(cki);

				if (!String.IsNullOrWhiteSpace(dragAndDropFile)) {

					Debug.WriteLine($">> {dragAndDropFile}");
					output.DragAndDrop = dragAndDropFile;
					return;
				}

				break;
			case ConsoleEventType.MOUSE_EVENT when ir.IsMouseScroll:

				bool scrollDown = me.dwButtonState.HasFlag(ButtonState.SCROLL_DOWN);

				int increment = scrollDown ? ConsoleManager.ScrollIncrement : -ConsoleManager.ScrollIncrement;

				bool b = ConsoleManager.Scroll(increment);

				goto _ReadInput;
			case ConsoleEventType.MOUSE_EVENT:
				// Mouse was read

				var (x, y) = (me.dwMousePosition.X, me.dwMousePosition.Y);


				// hack: wtf

				//Debug.WriteLine($"{me}");

				if (me.dwButtonState == ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
					ConsoleManager._click = true;
				}

				if (ConsoleManager._click && me.dwButtonState == 0) {
					//ConsoleManager._click = false;
					goto default;
				}

				/*if (me.dwButtonState == 0 && ConsoleManager.m_prevRecord.dwButtonState == ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
					Debug.WriteLine("ignoring 2nd press");
					ConsoleManager.m_prevRecord = me;
					goto default;
					
				}*/


				if (m_optionPositions.ContainsKey(y)) {
					ConsoleOption option = m_optionPositions[y];

					int indexOf = Options.IndexOf(option);

					char c = ConsoleOption.GetDisplayOptionFromIndex(indexOf);

					// note: KeyChar argument is slightly inaccurate (case insensitive; always uppercase)
					cki = ConsoleManager.GetKeyInfo(c, c, me.dwControlKeyState);

					HighlightClick(y, x);
				}
				else {
					goto default;
				}

				break;

			default:
				cki = default;
				break;
		}

		//packet.Input = cki2;

#if DEBUG
		var debugStr = ir.EventType switch
		{
			ConsoleEventType.MOUSE_EVENT => ir.MouseEvent.ToString(),
			ConsoleEventType.KEY_EVENT   => ir.KeyEvent.ToString(),

			_ => string.Empty,
		};

		Debug.WriteLine($"{ir} | {(debugStr)}",
		                nameof(ConsoleDialog));
#endif


		if (cki == default) {
			output.Key = null;
		}
		else {
			output.Key = cki;
		}
	}

	private static void HighlightClick(ushort y, ushort x)
	{
		const char SPACE = (char) 32;

		var bufferLine = ConsoleManager.ReadBufferLine(y).Trim(SPACE);

		Debug.WriteLine($"click ({x}, {y})", nameof(ConsoleDialog));

		ConsoleManager.Highlight(ConsoleManager.HighlightAttribute, bufferLine.Length, 0, y);

		//ConsoleManager.WriteBufferLine(s1, y);

		Thread.Sleep(50);
	}


	/// <summary>
	///     Display dialog
	/// </summary>
	private void Display(ConsoleOutputResult output)
	{
		Debug.WriteLine($"{nameof(ConsoleDialog)}: Update display");

		Display();

		// Show options

		if (SelectMultiple) {
			ConsoleManager.NewLine();

			string optionsStr = $"{SC.CHEVRON} {output.Output.QuickJoin()}"
				.AddColor(ConsoleManager.ColorOptions);

			ConsoleManager.Write(true, optionsStr);

			ConsoleManager.NewLine();

			ConsoleManager.Write(SelectMultiplePrompt);

		}
	}

	public void Display(bool clear = true)
	{
		// todo: atomic write operations (i.e., instead of incremental)

		if (clear) {
			Console.Clear();
		}

		if (Header != null) {
			ConsoleManager.Write(false, Header.AddColor(ConsoleManager.ColorHeader));
		}

		if (Subtitle != null) {

			string subStr = ConsoleManager.FormatString(SC.CHEVRON, Subtitle, false)
			                              .AddColor(ConsoleManager.ColorOptions);

			ConsoleManager.Write(true, subStr);
			ConsoleManager.NewLine();
		}

		int clamp = Math.Clamp(Options.Count, 0, ConsoleOption.MAX_DISPLAY_OPTIONS);

		for (int i = 0; i < clamp; i++) {
			ConsoleOption option = Options[i];

			string s = option.GetConsoleString(i);

			var top = Console.CursorTop;
			m_optionPositions[top] = option;

			ConsoleManager.Write(false, s);

		}

		ConsoleManager.NewLine();

		if (Status != null) {
			ConsoleManager.Write(Status);
		}

		if (Description != null) {
			ConsoleManager.NewLine();

			ConsoleManager.Write(Description);
		}
	}


	/// <summary>
	///     Exits <see cref="ReadInput" />
	/// </summary>
	public const ConsoleKey NC_GLOBAL_EXIT_KEY = ConsoleKey.Escape;

	/// <summary>
	///     <see cref="Refresh" />
	/// </summary>
	public const ConsoleKey NC_GLOBAL_REFRESH_KEY = ConsoleKey.F5;

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
		const char QUOTE = '\"';

		var sb = new StringBuilder();

		char keyChar = cki.KeyChar;

		var driveLetters = DriveInfo.GetDrives().Select(x => x.Name.First()).ToArray();

		if (keyChar == QUOTE || driveLetters.Any(e => e == keyChar)) {
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
			} while (keyChar != QUOTE);

		}

		return sb.ToString().Trim(QUOTE);
	}

	public void Refresh()
	{
		unsafe {
			var ptr = (int*) Unsafe.AsPointer(ref m_status);
			Interlocked.Exchange(ref Unsafe.AsRef<int>(ptr), (int) ConsoleStatus.Refresh);
		}

	}

	public override int GetHashCode()
	{
		var hashCode = new HashCode();

		IEnumerable<int> hashes = Options.Select(o => o.GetHashCode());

		foreach (int value in hashes) {
			hashCode.Add(value);
		}

		hashCode.Add(Status?.GetHashCode());
		hashCode.Add(Description?.GetHashCode());
		hashCode.Add(Header?.GetHashCode());
		hashCode.Add(Subtitle?.GetHashCode());

		return hashCode.ToHashCode();
	}
}