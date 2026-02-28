using System;
using System.Collections.Generic;
using System.Drawing;

// ReSharper disable UnusedMember.Global

namespace Kantan.Utilities;

/// <summary>
/// Color utilities.
/// </summary>
/// <seealso cref="Color"/>
/// <seealso cref="KnownColor"/>
/// <seealso cref="ConsoleColor"/>
public static class ColorHelper
{
	public static readonly Color AbsoluteRed   = Color.FromArgb(Byte.MaxValue, Byte.MaxValue, 0, 0);
	public static readonly Color AbsoluteGreen = Color.FromArgb(Byte.MaxValue, 0, Byte.MaxValue, 0);
	public static readonly Color AbsoluteBlue  = Color.FromArgb(Byte.MaxValue, 0, 0, Byte.MaxValue);

	/// <param name="c">Color to correct.</param>
	extension(Color c)
	{

		public Color Invert()
		{
			return Color.FromArgb(byte.MaxValue - c.R, byte.MaxValue - c.G, byte.MaxValue - c.B);
		}

		/// <summary>
		/// Creates color with corrected brightness.
		/// </summary>
		/// <param name="factor">The brightness correction factor. Must be between -1 and 1. 
		/// Negative values produce darker colors.</param>
		/// <returns>
		/// Corrected <see cref="Color"/> structure.
		/// </returns>
		public Color ChangeBrightness(float factor)
		{
			// Adapted from https://gist.github.com/zihotki/09fc41d52981fb6f93a81ebf20b35cd5

			float red   = c.R;
			float green = c.G;
			float blue  = c.B;

			if (factor < 0) {
				factor = 1 + factor;

				red   *= factor;
				green *= factor;
				blue  *= factor;
			}
			else {
				red   = (Byte.MaxValue - red)   * factor + red;
				green = (Byte.MaxValue - green) * factor + green;
				blue  = (Byte.MaxValue - blue)  * factor + blue;
			}

			return Color.FromArgb(c.A, (int) red, (int) green, (int) blue);
		}

		public IEnumerable<Color> GetGradients(Color end, int steps)
		{
			// https://stackoverflow.com/questions/2011832/generate-color-gradient-in-c-sharp

			int stepA = ((end.A - c.A) / (steps - 1));
			int stepR = ((end.R - c.R) / (steps - 1));
			int stepG = ((end.G - c.G) / (steps - 1));
			int stepB = ((end.B - c.B) / (steps - 1));

			for (int i = 0; i < steps; i++) {

				int startA = c.A + (stepA * i);
				int startR = c.R + (stepR * i);
				int startG = c.G + (stepG * i);
				int startB = c.B + (stepB * i);

				yield return Color.FromArgb(startA, startR, startG, startB);
			}
		}

	}

	public static Color ToColor(this ConsoleColor c)
	{
		var i = (int) c;

		int a = ((i & 8) > 0) ? 2 : 1;
		int r = ((i & 4) > 0) ? 64 * a : 0;
		int g = ((i & 2) > 0) ? 64 * a : 0;
		int b = ((i & 1) > 0) ? 64 * a : 0;

		return Color.FromArgb(r, g, b);
	}

}