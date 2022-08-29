using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.FileTypes;

public sealed class FastResolver : IFileTypeResolver
{
	#region Implementation of IDisposable

	public void Dispose()
	{
			
	}

	#endregion

	#region Implementation of IFileTypeResolver

	public IEnumerable<FileType> Resolve(byte[] rg)
	{
		return FileType.Resolve(rg);
	}

	public async Task<IEnumerable<FileType>> ResolveAsync(Stream m)
	{
		return await FileType.ResolveAsync(m);
	}

	public IEnumerable<FileType> Resolve(Stream m)
	{
		return FileType.Resolve(m);
	}

	#endregion
}