global using MN = System.Diagnostics.CodeAnalysis.MaybeNullAttribute;
global using MNNW = System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute;
global using UT = System.Runtime.InteropServices.UnmanagedType;
global using MA = System.Runtime.InteropServices.MarshalAsAttribute;
global using KNI = Kantan.Net.KantanNetInit;
global using MURV = JetBrains.Annotations.MustUseReturnValueAttribute;
global using CBN = JetBrains.Annotations.CanBeNullAttribute;
global using NN = System.Diagnostics.CodeAnalysis.NotNullAttribute;
global using VP = JetBrains.Annotations.ValueProviderAttribute;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Kantan.Net.Properties;
using Kantan.Net.Utilities;
using Microsoft.Extensions.Logging;
// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable ConditionIsAlwaysTrueOrFalse
#pragma warning disable CA2255
[assembly: InternalsVisibleTo("Test")]

namespace Kantan.Net;

public static class KantanNetInit
{
	public const string NAME = "Kantan.Net";

	public static readonly Assembly Assembly = typeof(KNI).GetTypeInfo().Assembly;

	static KantanNetInit()
	{
		// RuntimeHelpers.RunClassConstructor(typeof(HttpUtilities).TypeHandle);
		// Setup();
	}

	[ModuleInitializer]
	public static void Setup()
	{
		Trace.WriteLine($"[{NAME}]: init");

	}

	public static void Close()
	{
		//todo...
	}
}