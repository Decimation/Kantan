using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Kantan.OS.Structures;

/// <summary>
/// Defines the coordinates of the upper left and lower right corners of
/// a rectangle.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SmallRect
{
	public short left;
	public short top;
	public short right;
	public short bottom;

	/// <summary>
	/// Creates a new instance of the SmallRect structure.
	/// </summary>
	/// <param name="mLeft">Column position of top left corner.</param>
	/// <param name="mTop">Row position of the top left corner.</param>
	/// <param name="mRight">Column position of the bottom right corner.</param>
	/// <param name="mBottom">Row position of the bottom right corner.</param>
	public SmallRect(short mLeft, short mTop, short mRight, short mBottom)
	{
		left   = mLeft;
		top    = mTop;
		right  = mRight;
		bottom = mBottom;
	}

	/// <summary>
	/// Gets or sets the column position of the top left corner of a rectangle.
	/// </summary>
	public short Left
	{
		get { return left; }
		set { left = value; }
	}

	/// <summary>
	/// Gets or sets the row position of the top left corner of a rectangle.
	/// </summary>
	public short Top
	{
		get { return top; }
		set { top = value; }
	}

	/// <summary>
	/// Gets or sets the column position of the bottom right corner of a rectangle.
	/// </summary>
	public short Right
	{
		get { return right; }
		set { right = value; }
	}

	/// <summary>
	/// Gets or sets the row position of the bottom right corner of a rectangle.
	/// </summary>
	public short Bottom
	{
		get { return bottom; }
		set { bottom = value; }
	}

	/// <summary>
	/// Gets or sets the width of a rectangle.  When setting the width, the
	/// column position of the bottom right corner is adjusted.
	/// </summary>
	public short Width
	{
		get { return (short) (right - left + 1); }
		set { right = (short) (left + value - 1); }
	}

	/// <summary>
	/// Gets or sets the height of a rectangle.  When setting the height, the
	/// row position of the bottom right corner is adjusted.
	/// </summary>
	public short Height
	{
		get { return (short) (bottom - top + 1); }
		set { bottom = (short) (top + value - 1); }
	}
}