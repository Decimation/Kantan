using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kantan.Utilities;

namespace Kantan.FileTypes;

public interface IFileTypeResolver : IDisposable
{
	public IEnumerable<FileType> Resolve(byte[] rg);

	public async Task<IEnumerable<FileType>> ResolveAsync(Stream m)
	{
		return Resolve(await m.ReadHeaderAsync());
	}

	public IEnumerable<FileType> Resolve(Stream m)
	{
		return Resolve(m.ReadHeader());
	}

	public static IFileTypeResolver Default { get; set; } = UrlmonResolver.Value; //todo
}