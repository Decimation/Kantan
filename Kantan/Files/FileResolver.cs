using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Kantan.Files
{
	public class FileResolver : IFileTypeResolver
	{
		public void Dispose() { }

		public string Resolve(byte[] rg)
		{
			return ResolveAsync(new MemoryStream(rg)).Result; //todo
		}

		public async Task<string> ResolveAsync(Stream m)
		{

			// IFlurlResponse res = await url.GetAsync();

			try {

				var hdr = new byte[0xFF];
				var i2  = await m.ReadAsync(hdr, 0, hdr.Length);

				string s = Path.GetTempFileName();

				await File.WriteAllBytesAsync(s, hdr);

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

				var sz = await proc.StandardOutput.ReadToEndAsync();

				await proc.WaitForExitAsync();
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