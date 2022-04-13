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
using Kantan.Net.Content.Resolvers;
using Kantan.Net.Properties;

// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable ConditionIsAlwaysTrueOrFalse
#pragma warning disable CA2255
[assembly:InternalsVisibleTo("Test")]
namespace Kantan.Net;

public static class KantanNetInit
{
	public const string NAME = "Kantan.Net";

	[ModuleInitializer]
	public static void Setup()
	{
		Trace.WriteLine($"[{NAME}]: init");
		var assemblyLocation = typeof(KantanNetInit).GetTypeInfo().Assembly.Location;
		
	}

	public static void Close()
	{
		IHttpTypeResolver.Default.Dispose();
		HttpUtilities.Client.Dispose();
		//todo...
	}
}
