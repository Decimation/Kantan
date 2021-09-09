#nullable enable
using System;
using System.Collections;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace Kantan.Text
{
	public static class StringBuilderHelper
	{
		public static StringBuilder Append(this StringBuilder sb, string name, object? val, string? valStr = null,
		                                   bool newLine = true)
		{
			if (val != null) {

				// Patterns are so epic

				switch (val) {
					case IList { Count: 0 }:
					case string s when String.IsNullOrWhiteSpace(s):
						return sb;

					default:
					{
						valStr ??= val.ToString();

						string fs = $"{name}: {valStr}".Truncate();

						if (newLine) {
							fs += "\n";
						}

						sb.Append(fs);
						break;
					}
				}

			}

			return sb;
		}
	}
}