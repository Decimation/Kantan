using System;
using System.IO;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

[assembly: InternalsVisibleTo("Novus")]
[assembly: InternalsVisibleTo("Kantan.Net")]
[assembly: InternalsVisibleTo("Test")]

namespace Kantan.Internal
{
	/// <summary>
	/// Internal library common utilities, values, etc.
	/// </summary>
	internal static class Common
	{
		internal static readonly Random RandomInstance = new();

		internal const string DEBUG_COND = "DEBUG";

		internal const string TRACE_COND = "TRACE";

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

		public const string OS_WINDOWS = "windows";
	}
}