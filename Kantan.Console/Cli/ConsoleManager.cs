
global using SConsole = global::System.Console;
global using AConsole = Spectre.Console.AnsiConsole;
using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kantan.Console.Cli.Controls;
using Kantan.Text;
using static Kantan.Text.Strings;
using Color = System.Drawing.Color;

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

#pragma warning disable 8602, CS8604, IDE0059, IDE0051
#nullable enable

namespace Kantan.Console.Cli;

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
///             <see cref="ConsoleManager.UI.ProgressIndicator" />
///         </description>
///     </item>
/// </list>
[Obsolete]
public static partial class ConsoleManager
{
	public static Color ColorHeader  { get; set; } = Color.Red;
	public static Color ColorOptions { get; set; } = Color.Aquamarine;
	public static Color ColorError   { get; set; } = Color.Red;

	public static int BufferLimit { get; set; } = SConsole.BufferWidth - 10;

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

	/// <summary>
	///     Root formatting function.
	/// </summary>
	[StringFormatMethod(KantanInit.STRING_FORMAT_ARG)]
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

	private static string GetUnicodeBoxPipe(IList<string> l, int i)
	{
		string delim;

		if (i == 0 && l.Count == 2) {
			delim = Constants.Horizontal;
			return delim;
		}

		if (l.Skip(i + 1).All(String.IsNullOrWhiteSpace)) {
			return Constants.BottomLeftCorner;
		}

		delim = l switch
		{
			{ Count: 1 } => Constants.Horizontal,
			{ Count: 2 } => i switch
			{
				0 => Constants.UpperLeftCorner,
				1 => Constants.BottomLeftCorner,
				_ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
			},
			_ => i switch
			{
				0   => Constants.UpperLeftCorner,
				> 0 => Constants.Vertical,
				_   => Constants.BottomLeftCorner,
			}
		};

		return delim;
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

				SConsole.Write(str);
			}

			input = SConsole.ReadLine();

			isInvalid = invalid(input);

			if (isInvalid) {
				errPrompt ??= "Invalid input";
				AConsole.WriteLine(errPrompt.AddColor(ColorError));
				Thread.Sleep(PauseTime);
				//SConsole.Write(new string('\r',7));
				/*ClearLastLine();
				SConsole.CursorTop--;
				*/

				ClearCurrentLine();
				SConsole.CursorTop--;

			}

		} while (isInvalid);

		return input;
	}

	[StringFormatMethod(KantanInit.STRING_FORMAT_ARG)]
	public static bool ReadConfirmation(string msg, params object[] args)
	{
		Write($"{Constants.ASTERISK} {String.Format(msg, args)} ({OPTION_Y}/{OPTION_N}): ");

		char key = Char.ToUpper(ReadKey().KeyChar);

		AConsole.WriteLine();

		return key switch
		{
			OPTION_N => false,
			OPTION_Y => true,
			_        => ReadConfirmation(msg, args)
		};
	}

	public static ConsoleKeyInfo ReadKey(bool intercept) => SConsole.ReadKey(intercept);

	public static ConsoleKeyInfo ReadKey() => ReadKey(false);

	#region Wait

	public static void WaitForInput()
	{
		AConsole.WriteLine();
		AConsole.WriteLine("Press any key to continue...");
		ReadKey();
	}

	public static void WaitForTimeSpan(TimeSpan span) => Thread.Sleep(span);

	public static void WaitForSecond() => WaitForTimeSpan(PauseTime);

	#endregion

	#endregion IO

	#region Write

	public static void Print(params object[] args) => AConsole.WriteLine(args.QuickJoin());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NewLine() => AConsole.WriteLine();

	/// <summary>
	///     Root write method.
	/// </summary>
	[StringFormatMethod(KantanInit.STRING_FORMAT_ARG)]
	public static void Write(bool newLine, string msg, params object[] args)
	{
		string fmt = String.Format(msg, args);

		if (newLine) {
			AConsole.WriteLine(fmt);
		}
		else {
			AConsole.Write(fmt);
		}
	}

	[StringFormatMethod(KantanInit.STRING_FORMAT_ARG)]
	public static void Write(string msg, params object[] args) => Write(true, msg, args);

	[StringFormatMethod(KantanInit.STRING_FORMAT_ARG)]
	public static void WriteOnCurrentLine(string msg, params object[] args)
	{
		msg = String.Format(msg, args);

		string clear = new('\b', msg.Length);
		SConsole.Write(clear);
		Write(msg);
	}

	public static void WriteOnLine(string s, int newTop)
	{
		(int left, int top) = SConsole.GetCursorPosition();
		ClearLine(newTop);
		AConsole.WriteLine(s);
		SConsole.SetCursorPosition(left, top);
	}

	#endregion

	#region Clear

	private static void ClearCurrentLineCR()
	{
		AConsole.Write("\r" + new string(' ', SConsole.WindowWidth - 1) + "\r");
	}

	public static void ClearLine(int newTop, int? top = null)
	{
		(int left, int top1) = SConsole.GetCursorPosition();

		top ??= top1;

		SConsole.SetCursorPosition(0, newTop);
		AConsole.Write(new string(' ', SConsole.BufferWidth));
		SConsole.SetCursorPosition(left, top.Value);

	}

	public static void ClearCurrentLine()
	{
		SConsole.SetCursorPosition(0, SConsole.CursorTop - 1);

		int top = SConsole.CursorTop;
		/*SConsole.SetCursorPosition(0, SConsole.CursorTop);
		SConsole.Write(new string(' ', SConsole.BufferWidth));
		SConsole.SetCursorPosition(0, top);*/
		ClearLine(SConsole.CursorTop, top);

	}

	public static void ClearLastLine()
	{
		/*var top = SConsole.CursorTop;
		SConsole.SetCursorPosition(0, top - 1);
		SConsole.Write(new string(' ', SConsole.BufferWidth));
		SConsole.SetCursorPosition(0, top);*/

		ClearLine(SConsole.CursorTop - 1);
	}

	#endregion

	/// <summary>
	///     Exits <see cref="ConsoleDialog.ReadInput" />
	/// </summary>
	public const ConsoleKey NC_GLOBAL_EXIT_KEY = ConsoleKey.Escape;
}