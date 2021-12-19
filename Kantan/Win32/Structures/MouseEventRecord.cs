using System;
using System.Diagnostics;

// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming

namespace Kantan.Win32.Structures;

[DebuggerDisplay("{dwMousePosition.X}, {dwMousePosition.Y}")]
public struct MouseEventRecord
{
	public Coord           dwMousePosition;
	public ButtonState     dwButtonState;
	public ControlKeyState dwControlKeyState;
	public MouseEventFlags dwEventFlags;


	/// <inheritdoc />
	public override string ToString()
	{
		return $"{nameof(dwMousePosition)}: {dwMousePosition}, \n" +
		       $"\t{nameof(dwButtonState)}: {dwButtonState}, \n" +
		       $"\t{nameof(dwControlKeyState)}: {dwControlKeyState}, \n" +
		       $"\t{nameof(dwEventFlags)}: {dwEventFlags}\n";
	}
}

[Flags]
public enum ControlKeyState
{
	RightAltPressed  = 0x0001,
	LeftAltPressed   = 0x0002,
	RightCtrlPressed = 0x0004,
	LeftCtrlPressed  = 0x0008,
	ShiftPressed     = 0x0010,
	NumLockOn        = 0x0020,
	ScrollLockOn     = 0x0040,
	CapsLockOn       = 0x0080,
	EnhancedKey      = 0x0100
}

public enum ButtonState
{
	FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001,
	FROM_LEFT_2ND_BUTTON_PRESSED = 0x0004,
	FROM_LEFT_3RD_BUTTON_PRESSED = 0x0008,
	FROM_LEFT_4TH_BUTTON_PRESSED = 0x0010,
	RIGHTMOST_BUTTON_PRESSED     = 0x0002,

	/// <summary>
	///     For mouse wheel events, if this flag is set, the wheel was scrolled down.
	///     If cleared, the wheel was scrolled up.
	///     This is not officially documented.
	/// </summary>
	SCROLL_DOWN = unchecked((int) 0xFF000000)
}

public enum MouseEventFlags
{
	DOUBLE_CLICK   = 0x0002,
	MOUSE_HWHEELED = 0x0008,
	MOUSE_MOVED    = 0x0001,
	MOUSE_WHEELED  = 0x0004,

	/// <summary>
	/// A mouse button was pressed or released
	/// </summary>
	MOUSE_BUTTON = 0,
}