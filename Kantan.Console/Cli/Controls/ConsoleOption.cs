﻿#nullable disable
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
using Kantan.Model;
using Kantan.Text;
using Color = System.Drawing.Color;

// ReSharper disable SuggestVarOrType_DeconstructionDeclarations

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Global

namespace Kantan.Console.Cli.Controls;

[CanBeNull]
public delegate object ConsoleOptionFunction();

/// <summary>
///     Represents an interactive console/shell option
/// </summary>
[Obsolete]
public class ConsoleOption
{
	public ConsoleOption() { }

	/// <summary>
	///     Display name
	/// </summary>
	[MaybeNull]
	public virtual string Name { get; set; }

	/// <summary>
	///     Function to execute when selected
	/// </summary>
	public virtual ConsoleOptionFunction Function
	{
		get => Functions[NC_FN_MAIN];
		set => Functions[NC_FN_MAIN] = value;
	}

	public Dictionary<ConsoleModifiers, ConsoleOptionFunction> Functions { get; init; } = new()
	{
		//[0] = () => { return null; },

	};

	[MaybeNull]
	public Func<ConsoleOption, string> UpdateOption { get; set; }

	public void Update()
	{
		Name = UpdateOption?.Invoke(this);
	}

	[Pure]
	public string GetDataString()
	{
		var sb = new StringBuilder();

		foreach (var (key, value) in Data) {
			switch (value) {
				case null:
					continue;
				case ConsoleOption option:
					sb.Append(option.GetDataString());
					break;
				default:
					break;
			}

		}

		return sb.ToString();
	}

	#region Implementation of IMap

	public Dictionary<string, object> Data
	{
		get => IMap.ToMap(this);
	}

	#endregion

	internal static char GetDisplayOptionFromIndex(int i)
	{
		if (i < MAX_OPTION_N) {
			return Char.Parse(i.ToString());
		}

		int d = OPTION_LETTER_START + (i - MAX_OPTION_N);

		return (char) d;
	}

	internal static int GetIndexFromDisplayOption(char c)
	{
		if (Char.IsNumber(c)) {
			return (int) Char.GetNumericValue(c);
		}

		if (Char.IsLetter(c)) {
			c = Char.ToUpper(c);
			return MAX_OPTION_N + (c - OPTION_LETTER_START);
		}

		return KantanInit.INVALID;
	}

	[Pure]
	internal string GetConsoleString(int i)
	{
		var  sb = new StringBuilder();
		char c  = GetDisplayOptionFromIndex(i);

		string name = Name;

		sb.Append($"[{c}]: ");

		if (name != null) {
			sb.Append($"{name} ");
		}

		if (Data.Any()) {

			sb.AppendLine();

			sb.Append($"{Strings.Indent(GetDataString())}");
		}

		if (!sb.ToString().EndsWith(SC.NEW_LINE)) {
			sb.AppendLine();
		}

		string f = ConsoleManager.FormatString(null, sb.ToString());

		return f;
	}

	public const ConsoleModifiers NC_FN_MAIN  = 0;
	public const ConsoleModifiers NC_FN_ALT   = ConsoleModifiers.Alt;
	public const ConsoleModifiers NC_FN_CTRL  = ConsoleModifiers.Control;
	public const ConsoleModifiers NC_FN_SHIFT = ConsoleModifiers.Shift;
	public const ConsoleModifiers NC_FN_COMBO = NC_FN_CTRL | NC_FN_ALT;

	public const int  MAX_OPTION_N        = 10;
	public const char OPTION_LETTER_START = 'A';
	public const int  MAX_DISPLAY_OPTIONS = 36;

	#region From

	public static ConsoleOption[] FromArray<T>(T[] values) => FromArray(values, arg => arg.ToString());

	public static ConsoleOption[] FromArray<T>(IList<T> values, Func<T, string> getName)
	{
		var rg = new ConsoleOption[values.Count];

		for (int i = 0; i < rg.Length; i++) {
			var option = values[i];

			string name = getName(option);

			rg[i] = new ConsoleOption
			{
				Name         = name,
				Function     = () => option,
				UpdateOption = (t) => t.Name = getName(option)
			};

		}

		return rg;
	}

	public static ConsoleOption[] FromEnum<TEnum>() where TEnum : Enum
	{
		var options = (TEnum[]) Enum.GetValues(typeof(TEnum));

		return FromArray(options, e => Enum.GetName(typeof(TEnum), e) ??
		                               throw new InvalidOperationException());
	}

	#endregion
}