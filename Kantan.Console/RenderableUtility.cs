global using CBN = JetBrains.Annotations.CanBeNullAttribute;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kantan.Console;

public static class RenderableUtility
{

	public static IRenderable AsRenderable<T>(T? val) where T : struct
		=> val.HasValue ? AsRenderable(val.Value) : ElementUtility.Txt_NA;

	public static IRenderable AsRenderable<T>(T val)
	{
		IRenderable renderable = val switch
		{
			IRenderable r => r,

			string sz when String.IsNullOrWhiteSpace(sz) => ElementUtility.Txt_NA,
			string sz                                    => new SText(Markup.Escape(sz)),
			bool b                                       => b.ToPrettyText(),
			null                                         => ElementUtility.Txt_NA,
			_                                            => new SText(val?.ToString())
		};
		return renderable;
	}

	public static SText ToPrettyText(this bool b) => b ? ElementUtility.Txt_Rad : ElementUtility.Txt_Mul;

}