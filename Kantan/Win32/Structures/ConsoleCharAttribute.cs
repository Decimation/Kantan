using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Kantan.Win32.Structures;

/// <summary>
/// Helper class that simplifies working with foreground and background colors.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ConsoleCharAttribute
{
	public short attr;

	/// <summary>
	/// Creates a new instance of the ConsoleCharAttribute structure
	/// </summary>
	/// <param name="fg">The foreground color.</param>
	/// <param name="bg">The background color.</param>
	public ConsoleCharAttribute(ConsoleColor fg, ConsoleColor bg)
	{
		attr = (short) (((ushort) bg << 4) | (ushort) fg);
	}

	/// <summary>
	/// Creates a new instance of the ConsoleCharAttribute structure.
	/// </summary>
	/// <param name="wAttr">The combined foreground/background attribute.</param>
	public ConsoleCharAttribute(short wAttr)
	{
		attr = wAttr;
	}

	/// <summary>
	/// Gets or sets the foreground color attribute.
	/// </summary>
	public ConsoleColor Foreground
	{
		get => (ConsoleColor) (attr & 0x0f);
		set => attr = (short) ((attr & 0xfff0) | (ushort) value);
	}

	/// <summary>
	/// Gets or sets the background color attribute.
	/// </summary>
	public ConsoleColor Background
	{
		get => (ConsoleColor) ((attr >> 4) & 0x0f);
		set => attr = (short) ((attr & 0xff0f) | ((ushort) value << 4));
	}

		
}