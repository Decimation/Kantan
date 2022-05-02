using System;
using System.Diagnostics;
using Kantan.Utilities;

namespace Kantan.Net.Adapters;

public static class CliAdapters
{
	public static string gallery_dl_resolve(string args)
	{
		return ProcessHelper.GetProcessOutput("gallery-dl", args);
	}
}