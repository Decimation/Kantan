using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using static Kantan.Internal.Common;

// ReSharper disable UnusedMember.Global

#nullable enable
namespace Kantan.Diagnostics
{
	public static class Log
	{
		[Conditional(TRACE_COND)]
		public static void WriteLine(string msg, string? category = null, string? src = null)
		{
			var sb = new StringBuilder();


			if (src is { }) {
				sb.Append($"[{src}] ");
			}

			sb.Append(msg);
			
			Trace.WriteLine(sb.ToString(), category);
		}
	}
}