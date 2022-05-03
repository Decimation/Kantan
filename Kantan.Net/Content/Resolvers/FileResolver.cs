using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kantan.Net.Properties;
using Kantan.Utilities;

namespace Kantan.Net.Content.Resolvers
{
	public class FileResolver : IHttpTypeResolver
	{
		#region Implementation of IDisposable

		public void Dispose()
		{
			
		}

		#endregion

		#region Implementation of IHttpTypeResolver

		public string Resolve(Stream m)
		{
			// IFlurlResponse res = await url.GetAsync();

			try
			{

				byte[] hdr = new Byte[0xFF];
				m.Read(hdr, 0, hdr.Length);

				string s = Path.GetTempFileName();

				File.WriteAllBytes(s, hdr);

				//@"C:\msys64\usr\bin\file.exe"
				// var name   = SearchInPath("file.exe");

				var args = $"{s} -i";

				var output = ProcessHelper.GetProcessOutput(Resources.EXE_FILE, args);

				File.Delete(s);

				return output;
			}
			catch (Exception)
			{
				return null;
			}
		}

		#endregion
	}
}
