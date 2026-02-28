using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kantan.Numeric;

public record struct SizeVariadic<T> where T : struct, INumber<T>
{
	public T? Width { get; set; }

	public T? Height { get; set; }

	[MNNW(true, nameof(Width.Value), nameof(Height.Value))]
	public readonly bool IsDimensional => HasWidth && HasHeight;

	[MNNW(true, nameof(Height))]
	private readonly bool HasHeight => Height.HasValue;

	[MNNW(true, nameof(Width))]
	private readonly bool HasWidth => Width.HasValue;

	public SizeVariadic(T? width, T? height)
	{
		Width  = width;
		Height = height;
	}

	public readonly Size ToSize()
	{
		var wv = Width.GetValueOrDefault();
		var hv = Height.GetValueOrDefault();

		int iwv = default;
		int ihv = default;

		if (typeof(T) != typeof(int)) {
			Trace.WriteLine($"{typeof(T)} is not int!");
		}

		if (T.IsInteger(wv)) {

			iwv = Unsafe.As<T, int>(ref wv);
			ihv = Unsafe.As<T, int>(ref hv);
		}

		//todo

		return new Size(iwv, ihv);
	}

}
