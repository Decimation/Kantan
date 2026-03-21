// Author: Deci | Project: Kantan.Console | Name: ElementUtility.cs
// Date: 2026/03/20 @ 23:03:58

using System;
using Kantan.Text;
// ReSharper disable InconsistentNaming

namespace Kantan.Console;

public static class ElementUtility
{

	public const string NA_STR = "-";

	public static readonly SText Txt_Empty = new(String.Empty);

	public static readonly SText Txt_NA = new(NA_STR);

	public static readonly SText Txt_Rad = new(Strings.Constants.RAD_SIGN.ToString());

	public static readonly SText Txt_Mul = new(Strings.Constants.MUL_SIGN.ToString());

}