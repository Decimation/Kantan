using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using static Kantan.Internal.Common;
// ReSharper disable UnusedMember.Global

#nullable enable
namespace Kantan.Diagnostics
{
	public static class Log
	{
		[Conditional(TRACE_COND)]
		public static void WriteLine(string msg, string category, [CallerMemberName] string? caller = null)
		{
			Trace.WriteLine($"({caller}): {msg}", category);
		}


		[Conditional(TRACE_COND)]
		[StringFormatMethod(STRING_FORMAT_ARG)]
		public static void WriteLine(string msg, params object[] args)
		{
			Trace.WriteLine(String.Format(msg, args));
		}
	}
}