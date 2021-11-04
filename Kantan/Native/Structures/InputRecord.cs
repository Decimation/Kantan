using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

// ReSharper disable InconsistentNaming

namespace Kantan.Native.Structures
{
	[DebuggerDisplay("EventType: {EventType}")]
	[StructLayout(LayoutKind.Explicit)]
	public struct InputRecord
	{
		[FieldOffset(0)]
		public ConsoleEventType EventType;

		[FieldOffset(4)]
		public KeyEventRecord KeyEvent;

		[FieldOffset(4)]
		public MouseEventRecord MouseEvent;

		/// <summary>
		/// Window buffer size information if this is a window buffer size event.
		/// </summary>
		[FieldOffset(4)]
		public WindowBufferSizeEvent WindowBufferSizeEvent;

		/// <summary>
		/// Menu event information if this is a menu event.
		/// </summary>
		[FieldOffset(4)]
		public MenuEvent MenuEvent;

		/// <summary>
		/// Focus event information if this is a focus event.
		/// </summary>
		[FieldOffset(4)]
		public FocusEvent FocusEvent;
		
		public override string ToString()
		{
			return $"{nameof(EventType)}: {EventType}";
		}
	}

	public enum ConsoleEventType : short
	{
		FOCUS_EVENT              = 0x0010,
		KEY_EVENT                = 0x0001,
		MENU_EVENT               = 0x0008,
		MOUSE_EVENT              = 0x0002,
		WINDOW_BUFFER_SIZE_EVENT = 0x0004
	}
}