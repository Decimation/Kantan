﻿global using KI = Kantan.KantanInit;
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
namespace Kantan;

public static class KantanInit
{
	public const string NAME = "Kantan";

	[ModuleInitializer]
	public static void Setup()
	{
		Trace.WriteLine($"[{NAME}]: init");
	}

	public static void Close() { }
}