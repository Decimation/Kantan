using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Utilities;

public static class ProcessHelper
{
	public static string GetProcessOutput(string fileName, string args = null)
	{
		Process proc = GetProcess(fileName, args);

		proc.Start();

		var sz = proc.StandardOutput.ReadToEnd();

		proc.WaitForExit();
		return sz;
	}

	public static Process GetProcess(string fileName, string args)
	{
		using var proc = new Process()
		{
			StartInfo =
			{
				FileName  = fileName,
				Arguments = args ?? String.Empty,

				RedirectStandardInput  = false,
				RedirectStandardOutput = true,
				RedirectStandardError  = true,

				UseShellExecute = false,
				WindowStyle     = ProcessWindowStyle.Hidden,
			},

			EnableRaisingEvents = true,
		};

		return proc;
	}
}