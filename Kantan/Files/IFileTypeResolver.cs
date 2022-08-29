using System;
using System.IO;
using System.Threading.Tasks;
using Kantan.Utilities;

namespace Kantan.Files;

public interface IFileTypeResolver : IDisposable
{
	public string Resolve(byte[] rg);

	public async Task<string> ResolveAsync(Stream m)
	{
		return Resolve(await m.ReadHeaderAsync());
	}

	public string Resolve(Stream m)
	{
		return Resolve(m.ReadHeader());
	}

	public static IFileTypeResolver Default { get; set; } = new UrlmonResolver(); //todo
}