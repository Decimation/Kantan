using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Kantan.Native.Structures
{
	[DebuggerDisplay("EventType: {EventType}")]
	[StructLayout(LayoutKind.Explicit)]
	internal struct InputRecord
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
}