﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Kantan.Utilities;

// ReSharper disable UnusedVariable

// ReSharper disable InconsistentNaming

#nullable enable

// ReSharper disable UnusedMember.Global
// ReSharper disable ParameterTypeCanBeEnumerable.Global
// ReSharper disable StringCompareToIsCultureSpecific
#pragma warning disable IDE0059
namespace Kantan.Text;


public static class Pastel
{

	//https://github.com/silkfire/Pastel/blob/master/src/ConsoleExtensions.cs

	private const string FORMAT_STRING_START   = "\u001b[{0};2;";
	private const string FORMAT_STRING_COLOR   = "{1};{2};{3}m";
	private const string FORMAT_STRING_CONTENT = "{4}";

	private static readonly string Reset     = $"\x1b[{EmbeddedResources.Seq_Reset}m";
	private static readonly string Underline = $"\x1b[{EmbeddedResources.Seq_Underline}m";
	private static readonly string Negative  = $"\x1b[{EmbeddedResources.Seq_Negative}m";

	public static bool Enabled { get; set; }

	private static readonly string FORMAT_STRING_FULL = FORMAT_STRING_START + FORMAT_STRING_COLOR
	                                                                        + FORMAT_STRING_CONTENT
	                                                                        + Reset;

	private static readonly ReadOnlyDictionary<ColorPlane, string> PlaneFormatModifiers =
		new(new Dictionary<ColorPlane, string>
		{
			[ColorPlane.Foreground] = "38",
			[ColorPlane.Background] = "48"
		});

	private static readonly Regex CloseNestedPastelStringRegex1 =
		new($"({Reset.Replace("[", @"\[")})+", RegexOptions.Compiled);

	private static readonly Regex CloseNestedPastelStringRegex2 = new(
		$"(?<!^)(?<!{Reset.Replace("[", @"\[")})(?<!{String.Format($"{FORMAT_STRING_START.Replace("[", @"\[")}{FORMAT_STRING_COLOR}", new[] { $"(?:{PlaneFormatModifiers[ColorPlane.Foreground]}|{PlaneFormatModifiers[ColorPlane.Background]})" }.Concat(Enumerable.Repeat(@"\d{1,3}", 3)).Cast<object>().ToArray())})(?:{String.Format(FORMAT_STRING_START.Replace("[", @"\["), $"(?:{PlaneFormatModifiers[ColorPlane.Foreground]}|{PlaneFormatModifiers[ColorPlane.Background]})")})"
		, RegexOptions.Compiled);

	private static readonly ReadOnlyDictionary<ColorPlane, Regex> CloseNestedPastelStringRegex3 =
		new(new Dictionary<ColorPlane, Regex>
		{
			[ColorPlane.Foreground] =
				new(
					$"(?:{Reset.Replace("[", @"\[")})(?!{String.Format(FORMAT_STRING_START.Replace("[", @"\["), PlaneFormatModifiers[ColorPlane.Foreground])})(?!$)",
					RegexOptions.Compiled),
			[ColorPlane.Background] =
				new(
					$"(?:{Reset.Replace("[", @"\[")})(?!{String.Format(FORMAT_STRING_START.Replace("[", @"\["), PlaneFormatModifiers[ColorPlane.Background])})(?!$)",
					RegexOptions.Compiled)
		});

	private static readonly Func<string, int> ParseHexColor =
		hc => Int32.Parse(hc.Replace("#", ""), NumberStyles.HexNumber);

	private static readonly Func<string, Color, ColorPlane, string> ColorFormat = (i, c, p) =>
		String.Format(FORMAT_STRING_FULL, PlaneFormatModifiers[p], c.R, c.G, c.B, CloseNestedPastelStrings(i, c, p));

	private static readonly Func<string, string, ColorPlane, string> ColorHexFormat =
		(i, c, p) => ColorFormat(i, Color.FromArgb(ParseHexColor(c)), p);

	private static readonly ColorFormatFunction NoColorOutputFormat = (i, _) => i;

	private static readonly HexColorFormatFunction NoHexColorOutputFormat = (i, _) => i;

	private static readonly ColorFormatFunction
		ForegroundColorFormat = (i, c) => ColorFormat(i, c, ColorPlane.Foreground);

	private static readonly HexColorFormatFunction ForegroundHexColorFormat =
		(i, c) => ColorHexFormat(i, c, ColorPlane.Foreground);

	private static readonly ColorFormatFunction
		BackgroundColorFormat = (i, c) => ColorFormat(i, c, ColorPlane.Background);

	private static readonly HexColorFormatFunction BackgroundHexColorFormat =
		(i, c) => ColorHexFormat(i, c, ColorPlane.Background);

	private static readonly ReadOnlyDictionary<bool, ReadOnlyDictionary<ColorPlane, ColorFormatFunction>>
		ColorFormatFunctions
			= new(
				new Dictionary<bool, ReadOnlyDictionary<ColorPlane, ColorFormatFunction>>
				{
					[false] = new(
						new Dictionary<ColorPlane, ColorFormatFunction>
						{
							[ColorPlane.Foreground] = NoColorOutputFormat,
							[ColorPlane.Background] = NoColorOutputFormat
						}),
					[true] = new(
						new Dictionary<ColorPlane, ColorFormatFunction>
						{
							[ColorPlane.Foreground] = ForegroundColorFormat,
							[ColorPlane.Background] = BackgroundColorFormat
						})
				});

	private static readonly ReadOnlyDictionary<bool, ReadOnlyDictionary<ColorPlane, HexColorFormatFunction>>
		HexColorFormatFunctions = new(
			new Dictionary<bool, ReadOnlyDictionary<ColorPlane, HexColorFormatFunction>>
			{
				[false] = new(
					new Dictionary<ColorPlane, HexColorFormatFunction>
					{
						[ColorPlane.Foreground] = NoHexColorOutputFormat,
						[ColorPlane.Background] = NoHexColorOutputFormat
					}),
				[true] = new(
					new Dictionary<ColorPlane, HexColorFormatFunction>
					{
						[ColorPlane.Foreground] = ForegroundHexColorFormat,
						[ColorPlane.Background] = BackgroundHexColorFormat
					})
			});

	static Pastel()
	{

		/*if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			var iStdOut = ConsoleManager.Win32.GetStdHandle(ConsoleManager.StandardHandle.STD_OUTPUT_HANDLE);

			bool enable = ConsoleManager.Win32.GetConsoleMode(iStdOut, out var outConsoleMode)
			              && ConsoleManager.Win32.SetConsoleMode(
				              iStdOut, outConsoleMode | ConsoleManager.ConsoleModes.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
		}*/

		Enabled = Environment.GetEnvironmentVariable("NO_COLOR") == null;
	}

	private static string CloseNestedPastelStrings(string input, Color color, ColorPlane colorPlane)
	{
		string closedString = CloseNestedPastelStringRegex1.Replace(input, Reset);

		closedString = CloseNestedPastelStringRegex2.Replace(closedString, $"{Reset}$0");

		closedString = CloseNestedPastelStringRegex3[colorPlane].Replace(closedString,
		                                                                 $"$0{String.Format($"{FORMAT_STRING_START}" + $"{FORMAT_STRING_COLOR}", PlaneFormatModifiers[colorPlane], color.R, color.G, color.B)}");

		return closedString;
	}

	private delegate string ColorFormatFunction(string input, Color color);

	private delegate string HexColorFormatFunction(string input, string hexColor);

	private enum ColorPlane : byte
	{

		Foreground,
		Background

	}

	/// <summary>
	///     Returns a string wrapped in an ANSI foreground color code using the specified color.
	/// </summary>
	/// <param name="input">The string to color.</param>
	/// <param name="color">The color to use on the specified string.</param>
	public static string AddColor(this string input, Color color)
	{
		return ColorFormatFunctions[Enabled][ColorPlane.Foreground](input, color);
	}

	/// <summary>
	///     Returns a string wrapped in an ANSI foreground color code using the specified color.
	/// </summary>
	/// <param name="input">The string to color.</param>
	/// <param name="hexColor">The color to use on the specified string.
	///     <para>Supported format: [#]RRGGBB.</para>
	/// </param>
	public static string AddColor(this string input, string hexColor)
	{
		return HexColorFormatFunctions[Enabled][ColorPlane.Foreground](input, hexColor);
	}

	/// <summary>
	///     Returns a string wrapped in an ANSI background color code using the specified color.
	/// </summary>
	/// <param name="input">The string to color.</param>
	/// <param name="color">The color to use on the specified string.</param>
	public static string AddColorBG(this string input, Color color)
	{
		return ColorFormatFunctions[Enabled][ColorPlane.Background](input, color);
	}

	/// <summary>
	///     Returns a string wrapped in an ANSI background color code using the specified color.
	/// </summary>
	/// <param name="input">The string to color.</param>
	/// <param name="hexColor">The color to use on the specified string.
	///     <para>Supported format: [#]RRGGBB.</para>
	/// </param>
	public static string AddColorBG(this string input, string hexColor)
	{
		return HexColorFormatFunctions[Enabled][ColorPlane.Background](input, hexColor);
	}

	public static string AddHighlight(this string s, Color? c = null)
	{
		c ??= Console.BackgroundColor.ToColor();
		var f = c.Value.Invert();
		return s.AddColorBG(f).AddColor(c.Value);
	}

	public enum PastelDecoration
	{

		Underline,
		Negative,
		Bold

	}

	public static string AddDecoration(this string s, PastelDecoration d)
	{
		string? dec = d switch
		{
			PastelDecoration.Underline => EmbeddedResources.Seq_Underline,
			PastelDecoration.Negative  => EmbeddedResources.Seq_Negative,
			PastelDecoration.Bold      => EmbeddedResources.Seq_Bold,
			_                    => null,
		};

		return $"\x1b[{dec}m{s}{Reset}";
	}

	public static string Remove(string s)
	{
		return Regex.Replace(s, "(\x9B|\x1B\\[)[0-?]*[ -/]*[@-~]", String.Empty);
	}

}
