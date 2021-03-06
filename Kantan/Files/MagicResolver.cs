#region

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Kantan.Utilities;

#endregion

namespace Kantan.Files;

/*
 * Adapted from https://github.com/hey-red/Mime
 */


public sealed class MagicResolver : IFileTypeResolver
{
	public const MagicOpenFlags MagicMimeFlags =
		MagicOpenFlags.MAGIC_ERROR |
		MagicOpenFlags.MAGIC_MIME_TYPE |
		MagicOpenFlags.MAGIC_NO_CHECK_COMPRESS |
		MagicOpenFlags.MAGIC_NO_CHECK_ELF |
		MagicOpenFlags.MAGIC_NO_CHECK_APPTYPE;

	public IntPtr Magic { get; }

	public static readonly MagicResolver Instance;


	static MagicResolver()
	{
		Instance = new MagicResolver();
	}

	public MagicResolver(string mgc = null)
	{
		mgc ??= GetMagicFile();

		Magic = MagicNative.magic_open(MagicMimeFlags);
		var rd = MagicNative.magic_load(Magic, mgc);

	}

	private static string GetMagicFile()
	{
		var mgc = Path.Combine(KantanInit.DataFolder, EmbeddedResources.F_MAGIC);

		if (!File.Exists(mgc)) {
			File.WriteAllBytes(mgc, EmbeddedResources.magic);
			Debug.WriteLine($"populating {mgc}");
		}

		Debug.WriteLine($"magic file: {mgc}");

		return mgc;
	}


	public string Resolve(Stream stream, IFileTypeResolver.FileTypeStyle f = IFileTypeResolver.FileTypeStyle.Mime)
	{
		if (f != IFileTypeResolver.FileTypeStyle.Mime) {
			throw new ArgumentOutOfRangeException(nameof(f));
		}

		var buf = (stream).ReadHeader();
		// var buf1 = stream.ReadHeaderAsync(FileType.RSRC_HEADER_LEN);
		// buf1.Wait();
		// var buf  = buf1.Result;

		var sz = MagicNative.magic_buffer(Magic, buf, buf.Length);
		return Marshal.PtrToStringAnsi(sz);
	}

	public void Dispose()
	{
		MagicNative.magic_close(Magic);
	}
}

