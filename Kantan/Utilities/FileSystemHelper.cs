using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Kantan.Utilities
{
	public static class FileSystemHelper
	{
		public static string SearchInPath(string s)
		{
			var variable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);

			string[] path = variable.Split(';');

			foreach (string directory in path) {
				if (Directory.Exists(directory)) {
					foreach (string file in Directory.EnumerateFiles(directory)) {
						if (Path.GetFileName(file) == s) {
							//rg.Add(file);
							return file;
						}
					}
				}
			}

			return null;
		}
	}
}