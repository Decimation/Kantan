using System;
using System.Diagnostics;
using System.IO;

namespace Kantan.Files
{
	public class FileResolver : IFileTypeResolver
	{
		public void Dispose() { }

		public string Resolve(Stream m, IFileTypeResolver.FileTypeStyle f = IFileTypeResolver.FileTypeStyle.Mime)
		{

			// IFlurlResponse res = await url.GetAsync();

			try {

				var hdr = new byte[0xFF];
				m.Read(hdr, 0, hdr.Length);

				string s = Path.GetTempFileName();

				File.WriteAllBytes(s, hdr);

				//@"C:\msys64\usr\bin\file.exe"
				// var name   = SearchInPath("file.exe");

				// var args = $"{s} -i";

				var args = $"{s}";

				var proc1 = new Process()
				{
					StartInfo =
					{
						FileName  = EmbeddedResources.EXE_FILE,
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
	}
}