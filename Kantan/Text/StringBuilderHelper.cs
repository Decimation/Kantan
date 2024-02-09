#nullable enable
using System;
using System.Collections;
using System.Drawing;
using System.Text;
using Kantan.Utilities;

// ReSharper disable UnusedMember.Global

namespace Kantan.Text;
#if OTHER

public static class StringBuilderHelper
{
	public static StringBuilder Append(this StringBuilder sb, string name, object? val,
	                                   string? valStr = null, bool newLine = true, 
	                                   Color? nameColor = null)
	{
		if (val == null) {
			goto ret;
		}

		// Patterns are so epic

		switch (val) {
			case IList { Count: 0 }:
			case string s when String.IsNullOrWhiteSpace(s):
				return sb;

			default:
			{
				valStr ??= val.ToString();

				if (nameColor.HasValue) {
					name = name.AddColor(nameColor.Value);
				}

				string fs = $"{name}: {valStr}".Truncate(Console.BufferWidth);

				if (newLine) {
					fs += "\n";
				}

				sb.Append(fs);
				break;
			}
		}
		ret:
		return sb;
	}
}
#endif
