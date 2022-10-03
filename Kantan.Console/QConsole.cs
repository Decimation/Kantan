using System.Diagnostics;
using Kantan.Console.Cli;
using Kantan.Console.Cli.Controls;
using static Kantan.Console.Cli.ConsoleManager;

namespace Kantan.Console
{
	public static class QConsole
	{
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
		public static async Task<ConsoleOutputResult> ReadInputAsync(CancellationToken? c = null,
		                                                      Action<ConsoleOutputResult> display = null)
		{
			var output = new ConsoleOutputResult();

			/*
			 * Handle input
			 */

			ConsoleKeyInfo cki;

			Init();

			m_optionPositions.Clear();
			bool skipDisplay = false;

			do {

				if (!skipDisplay) {
					display(output);
				}
				else {
					skipDisplay = false;
				}

				var token = c ?? CancellationToken.None;

				var inputTask = Task.Run(() => InputTask(output, token, display), token);

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

						if (Functions is { } && Functions.ContainsKey(cki.Key)) {
							Action function = Functions[cki.Key];
							function();
						}

						break;
					case >= ConsoleKey.VolumeMute and <= ConsoleKey.MediaPlay:
						Debug.WriteLine($"Ignoring key {cki.Key}");
						skipDisplay = true;
						// continue;
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

		private static ConsoleStatus _status;

		public static void Refresh()
		{
			ConsoleManager.ExchangeStatus(ConsoleManager.ConsoleStatus.Refresh, ref _status);
		}

		private static void InputTask(ConsoleOutputResult output, CancellationToken? t = null,
		                       Action<ConsoleOutputResult> display = null, Func<int, int, bool> onClick = null)
		{
			// Block until input is entered.

			_ReadInput:
			int prevCount = Options.Count;

			while (!InputAvailable) {
				bool refresh = ExchangeStatus(ConsoleStatus.Ok, ref _status) ==
				               ConsoleStatus.Refresh;

				int currentCount = Options.Count;

				// Refresh buffer if collection was updated

				if (t is { IsCancellationRequested: true }) {
					// break;
					output.Cancelled = true;
					return;
				}

				if ((refresh || prevCount != currentCount)) {
					display(output);
					prevCount = currentCount;
				}

			}

			InputRecord      ir = ReadInput();
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
					int  increment  = scrollDown ? ScrollIncrement : -ScrollIncrement;

					Scroll(increment);

					goto _ReadInput;
				case InputEventType.MOUSE_EVENT:
					// Mouse was read

					bool click = false;

					var (x, y) = (me.dwMousePosition.X, me.dwMousePosition.Y);

					if (History.TryPeek(out var ir2)) {
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
						cki = GetKeyInfo(c, c, me.dwControlKeyState);

						onClick(y, x);

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

		/// <summary>
		///     The index of an <see cref="ConsoleOption" /> corresponds to its key, which, when pressed,
		///     executes the <see cref="ConsoleOption.Function" /> with the appropriate modifiers
		/// </summary>
		/// <remarks>
		///     <see cref="ConsoleOption.MAX_DISPLAY_OPTIONS" />
		/// </remarks>
		public static IList<ConsoleOption> Options { get; } = new List<ConsoleOption>();

		/// <summary>
		///     <c>F*</c> keys
		/// </summary>
		public static Dictionary<ConsoleKey, Action> Functions { get; set; } = new();

		private static readonly Dictionary<int, ConsoleOption> m_optionPositions = new();

		public static bool SelectMultiple { get; set; }
	}
}