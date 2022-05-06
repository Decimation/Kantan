global using KI = Kantan.KantanInit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Novus")]
[assembly: InternalsVisibleTo("Kantan.Net")]
[assembly: InternalsVisibleTo("Test")]

// ReSharper disable InconsistentNaming
// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable ConditionIsAlwaysTrueOrFalse
#pragma warning disable CA2255
namespace Kantan;

public static class KantanInit
{
	public const string NAME = "Kantan";

	public static readonly string DataFolder =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), NAME);

	[ModuleInitializer]
	public static void Setup()
	{
		Trace.WriteLine($"[{NAME}]: init");
		Directory.CreateDirectory(DataFolder);
	}

	public static void Close() { }

	internal static readonly Random RandomInstance = new();

	internal const string DEBUG_COND        = "DEBUG";
	internal const string TRACE_COND        = "TRACE";
	internal const string STRING_FORMAT_ARG = "msg";

	/// <summary>
	/// Common integer value representing an invalid value, error, etc.
	/// </summary>
	internal const int INVALID = -1;

	/*private const string Kantan = "Kantan";

	internal static string AppFolder
	{
		get
		{
			var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			var appFolder     = Path.Combine(commonAppData, Kantan);

			if (!Directory.Exists(appFolder)) {
				Directory.CreateDirectory(appFolder);
			}

			return appFolder;
		}
	}*/
}