using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Kantan.OS.Structures;

/// <summary>
/// Specifies a Unicode or ASCII character and its attributes.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct ConsoleCharInfo
{
	[FieldOffset(0)]
	public char cUnicodeChar;

	[FieldOffset(0)]
	public byte bAsciiChar;

	[FieldOffset(2)]
	public ConsoleCharAttribute attr;

	/// <summary>
	/// Creates a new instance of the ConsoleCharInfo structure.
	/// </summary>
	/// <param name="uChar">The Unicode character.</param>
	/// <param name="attr">Character attributes.</param>
	public ConsoleCharInfo(char uChar, ConsoleCharAttribute attr)
	{
		bAsciiChar   = 0;
		cUnicodeChar = uChar;
		this.attr    = attr;
	}

	/// <summary>
	/// Creates a new instance of the ConsoleCharInfo structure.
	/// </summary>
	/// <param name="aChar">The ASCII character.</param>
	/// <param name="attr">Character attributes.</param>
	public ConsoleCharInfo(byte aChar, ConsoleCharAttribute attr)
	{
		cUnicodeChar = '\x0';
		bAsciiChar   = aChar;
		this.attr    = attr;
	}
		
		
		

	/// <summary>
	/// Gets or sets the foreground color attribute.
	/// </summary>
	public ConsoleColor Foreground
	{
		get => attr.Foreground;
		set => attr.Foreground = value;
	}

	/// <summary>
	/// Gets or sets the background color attribute.
	/// </summary>
	public ConsoleColor Background
	{
		get => attr.Background;
		set => attr.Background = value;
	}
}