using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable ConditionIsAlwaysTrueOrFalse
#pragma warning disable CA2255
namespace Kantan.Net;

internal static class Init
{
	internal const string NAME = "Kantan.Net";

	[ModuleInitializer]
	internal static void Setup()
	{
		Trace.WriteLine($"[{NAME}]: init");
	}
}
