#nullable disable
#pragma warning disable SYSLIB0014 // Type or member is obsolete

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using static Kantan.Diagnostics.LogCategories;

#pragma warning disable CS0618

// ReSharper disable LoopCanBeConvertedToQuery

#pragma warning disable 8602

// ReSharper disable UnusedMember.Global

namespace Kantan.Net;

public static class WebUtilities
{
	public static void OpenUrl(string url)
	{
		// https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
		// url must start with a protocol i.e. http://

		try {
			Process.Start(url);
		}
		catch {
			// hack because of this: https://github.com/dotnet/corefx/issues/10361
			if (OperatingSystem.IsWindows()) {
				url = url.Replace("&", "^&");

				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
				{
					CreateNoWindow = true
				});
			}
			else {
				throw;
			}
		}
	}
}
