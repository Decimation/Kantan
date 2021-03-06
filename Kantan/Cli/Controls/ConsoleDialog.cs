global using SC = Kantan.Text.Strings.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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


	public ConsoleOption this[string s]
	{
		get => GetOption(s);
	}

	[CanBeNull]
	public ConsoleOption GetOption(string name)
	{
		return Options.FirstOrDefault(x => x.Name == name);
	}

	public void Insert(Index i, ConsoleOption option) => Options.Insert(i.Value, option);

	public void Insert(ConsoleOption x, ConsoleOption option) => Insert(Options.IndexOf(x), option);

	public void Insert(string name, ConsoleOption option) => Insert(this[name], option);

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
	public async Task<ConsoleOutputResult> ReadInputAsync(CancellationToken? c = null)
	{
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
		bool skipDisplay = false;

		do {

			if (!skipDisplay) {
				Display(output);
			}
			else {
				skipDisplay = false;
			}

			var token = c ?? CancellationToken.None;

			var inputTask = Task.Run(() => InputTask(output, token), token);

			if (inputTask.IsCanceled || token.IsCancellationRequested) {
				return output;
			}

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
			}

			// Handle special keys
			

			switch (cki.Key) {
				case NC_GLOBAL_REFRESH_KEY:
					Refresh();
					break;
				case <= ConsoleKey.F12 and >= ConsoleKey.F1:
					int i = cki.Key - ConsoleKey.F1;

					if (Functions is { } && Functions.ContainsKey(cki.Key))
					{
						Action function = Functions[cki.Key];
						function();
					}
					break;
				case >= ConsoleKey.VolumeMute and <= ConsoleKey.MediaPlay:
					Debug.WriteLine($"Ignoring key {cki.Key}");
					skipDisplay = true;
					// continue;
					break;
				default:
					// skipDisplay = true;
					break;
			}

			// KeyChar can't be used as modifiers are not applicable
			char keyChar = (char) (int) cki.Key;

			if (!Char.IsLetterOrDigit(keyChar)) {
				// skipDisplay = true;
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

	private void InputTask(ConsoleOutputResult output, CancellationToken? t = null)
	{
		// Block until input is entered.

		_ReadInput:
		int prevCount = Options.Count;

		while (!ConsoleManager.InputAvailable) {
			bool refresh      = ExchangeStatus(ConsoleStatus.Ok) == ConsoleStatus.Refresh;
			int  currentCount = Options.Count;

			// Refresh buffer if collection was updated

			if (t is { IsCancellationRequested: true }) {
				// break;
				output.Cancelled = true;
				return;
			}

			if ((refresh || prevCount != currentCount)) {
				Display(output);
				prevCount = currentCount;
			}

		}


		InputRecord      ir = ConsoleManager.ReadInput();
		ConsoleKeyInfo   cki;
		MouseEventRecord me = ir.MouseEvent;

		switch (ir.EventType) {

			case InputEventType.KEY_EVENT:
				// Key was read

				cki = ir.ToConsoleKeyInfo();
				string dragAndDropFile = TryReadFile(cki);

				if (!String.IsNullOrWhiteSpace(dragAndDropFile)) {

					output.DragAndDrop = dragAndDropFile;
					return;
				}


				break;
			case InputEventType.MOUSE_EVENT when ir.IsMouseScroll:
				// Mouse scroll was read
				bool scrollDown = me.dwButtonState.HasFlag(ButtonState.SCROLL_DOWN);
				int  increment  = scrollDown ? ConsoleManager.ScrollIncrement : -ConsoleManager.ScrollIncrement;

				ConsoleManager.Scroll(increment);

				goto _ReadInput;
			case InputEventType.MOUSE_EVENT:
				// Mouse was read

				bool click = false;

				var (x, y) = (me.dwMousePosition.X, me.dwMousePosition.Y);

				if (ConsoleManager.History.TryPeek(out var ir2)) {
					if (ir2.MouseEvent.dwButtonState == ButtonState.FROM_LEFT_1ST_BUTTON_PRESSED) {
						click = true;
					}
				}


				if (click) {
					// output.Status.SkipNext = true;
					goto _ReadInput;
				}

				if (m_optionPositions.ContainsKey(y)) {
					var  option  = m_optionPositions[y];
					int  indexOf = Options.IndexOf(option);
					char c       = ConsoleOption.GetDisplayOptionFromIndex(indexOf);


					// note: KeyChar argument is slightly inaccurate (case insensitive; always uppercase)
					cki = ConsoleManager.GetKeyInfo(c, c, me.dwControlKeyState);

					HighlightClick(y, x);

				}
				else {
					output.Status.SkipNext = true;
					goto default;
				}

				break;

			default:
				cki = default;
				break;
		}


#if DEBUG
		var debugStr = ir.EventType switch
		{
			InputEventType.MOUSE_EVENT => ir.MouseEvent.ToString(),
			InputEventType.KEY_EVENT   => ir.KeyEvent.ToString(),

			_ => String.Empty,
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

	private unsafe ConsoleStatus ExchangeStatus(ConsoleStatus s)
	{
		var ptr    = (int*) Unsafe.AsPointer(ref m_status);
		var status = (ConsoleStatus) Interlocked.Exchange(ref Unsafe.AsRef<int>(ptr), (int) s);

		return status;
	}

	private static void HighlightClick(ushort y, ushort x)
	{
		const char SPACE = (char) 32;

		var bufferLine = ConsoleManager.ReadBufferLine(y).Trim(SPACE);

		// Debug.WriteLine($"highlight ({x}, {y})", nameof(ConsoleDialog));

		ConsoleManager.Highlight(ConsoleManager.HighlightAttribute, bufferLine.Length, 0, y);
		Thread.Sleep(TimeSpan.FromMilliseconds(50));
	}


	/// <summary>
	///     Display dialog
	/// </summary>
	private void Display(ConsoleOutputResult output)
	{
		if (output.Status.SkipNext) {
			// Avoid unnecessary display update

			output.Status.SkipNext = false;
			return;
		}

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
	private static string TryReadFile(ConsoleKeyInfo cki)
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
		ExchangeStatus(ConsoleStatus.Refresh);
	}


	private void EnsureDescription()
	{
		if (Description is not { }) {
			Description = String.Empty;
		}

		if (!Description.EndsWith('\n')) {
			Description += '\n';
		}

	}

	public ConsoleDialog AddDescriptions(params string[] strings)
	{
		foreach (string s in strings) {
			AddDescription(s);
		}

		return this;
	}

	public ConsoleDialog AddDescription(string s, Color? c = null)
	{
		EnsureDescription();

		if (c.HasValue) {
			s = s.AddColor(c.Value);
		}

		Description += s;
		return this;

	}

	public ConsoleDialog AddDescription(Dictionary<string, string> s, Color? c = null)
	{
		return AddDescription(Strings.GetMapString(s, c));

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
}