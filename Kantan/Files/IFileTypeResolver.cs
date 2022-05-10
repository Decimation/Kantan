using System;
using System.IO;

namespace Kantan.Files;

public interface IFileTypeResolver : IDisposable
{
	public enum FileTypeStyle
	{
		None,
		Mime,
		Extension
	}

	string Resolve(Stream m, FileTypeStyle f = FileTypeStyle.Mime);

	public static IFileTypeResolver Default { get; set; } = new UrlmonResolver(); //todo
}