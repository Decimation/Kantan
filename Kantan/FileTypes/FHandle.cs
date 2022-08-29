using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.FileTypes
{
	public class FHandle : IDisposable
	{
		public Stream Stream { get; private set; }

		public string Value { get; private set;}

		private FHandle(Stream stream, string value)
		{
			Stream = stream;
			Value  = value;
		}

		[CBN]
		public static FHandle Alloc(string s)
		{
			if (File.Exists(s)) {
				
			}
			else {
				
			}
			return null;
		}

		#region IDisposable

		public void Dispose()
		{
			Stream?.Dispose();
		}

		#endregion
	}
}
