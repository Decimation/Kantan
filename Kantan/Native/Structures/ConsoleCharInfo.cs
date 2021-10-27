using System;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace Kantan.Native.Structures
{
	/// <summary>
	/// Specifies a Unicode or ASCII character and its attributes.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct ConsoleCharInfo
	{
		[FieldOffset(0)]
		private char cUnicodeChar;

		[FieldOffset(0)]
		private byte bAsciiChar;

		[FieldOffset(2)]
		private ConsoleCharAttribute attr;

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
		/// Gets or sets the Unicode character represented by this ConsoleCharInfo structure.
		/// </summary>
		public char UnicodeChar
		{
			get { return cUnicodeChar; }
			set { cUnicodeChar = value; }
		}

		/// <summary>
		/// Gets or sets the ASCII character represented by this ConsoleCharInfo structure.
		/// </summary>
		public byte AsciiChar
		{
			get { return bAsciiChar; }
			set { bAsciiChar = value; }
		}

		/// <summary>
		/// Gets or sets the attributes for this character.
		/// </summary>
		public ConsoleCharAttribute Attribute
		{
			get { return attr; }
			set { attr = value; }
		}

		/// <summary>
		/// Gets or sets the foreground color attribute.
		/// </summary>
		public ConsoleColor Foreground
		{
			get { return attr.Foreground; }
			set { attr.Foreground = value; }
		}

		/// <summary>
		/// Gets or sets the background color attribute.
		/// </summary>
		public ConsoleColor Background
		{
			get { return attr.Background; }
			set { attr.Background = value; }
		}
	}
}