global using KNI = Kantan.Net.KantanNetInit;
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

	public static readonly LoggerFactory LoggerFactory = new() { };

	public static readonly ILogger Logger = LoggerFactory.CreateLogger($"{nameof(NAME)}");

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
		IHttpTypeResolver.Default.Dispose();
		HttpUtilities.Client.Dispose();
		LoggerFactory.Dispose();
		//todo...
	}
}