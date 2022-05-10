using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kantan.Net.Properties;
using Kantan.Utilities;

namespace Kantan.Net.Content.Resolvers
{
	public class FileResolver : IFileTypeResolver
	{
		#region Implementation of IDisposable

		public void Dispose() { }

		#endregion

		#region Implementation of IFileTypeResolver

		public string Resolve(Stream m)
		{
			// IFlurlResponse res = await url.GetAsync();

			try {

				var hdr = new byte[0xFF];
				m.Read(hdr, 0, hdr.Length);

				string s = Path.GetTempFileName();

				File.WriteAllBytes(s, hdr);

				//@"C:\msys64\usr\bin\file.exe"
				// var name   = SearchInPath("file.exe");

				var args = $"{s} -i";

				var proc1 = new Process()
				{
					StartInfo =
					{
						FileName  = Resources.EXE_FILE,
						Arguments = args,

						RedirectStandardInput  = false,
						RedirectStandardOutput = true,
						RedirectStandardError  = true,

						UseShellExecute = false,
						// WindowStyle     = ProcessWindowStyle.Hidden,
					},

					EnableRaisingEvents = true,
				};
				using Process proc = proc1;

				proc.Start();

				var sz = proc.StandardOutput.ReadToEnd();

				proc.WaitForExit();
				var output = (string) sz;

				File.Delete(s);

				return output;
			}
			catch (Exception) {
				return null;
			}
		}

		#endregion
	}
}