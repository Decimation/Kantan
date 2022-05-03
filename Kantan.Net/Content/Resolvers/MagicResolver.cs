#region

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Kantan.Net.Properties;
using Kantan.Utilities;

#endregion

namespace Kantan.Net.Content.Resolvers;

public sealed class MagicResolver : IHttpTypeResolver, IDisposable
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
		if (mgc == null) {
			var tmp = Path.Combine(Path.GetTempPath(), "magic_tmp.mgc");
			File.WriteAllBytes(tmp, Resources.magic);
			mgc = tmp;
			Debug.WriteLine($"temp magic file: {tmp}");
		}

		Magic = MagicNative.magic_open(MagicMimeFlags);
		var rd = MagicNative.magic_load(Magic, mgc);
	}

	
	public string Resolve(Stream stream)
	{
		var buf = (stream).GetHeaderBlock();
		var sz  = MagicNative.magic_buffer(Magic, buf, buf.Length);
		return Marshal.PtrToStringAnsi(sz);
	}

	public void Dispose()
	{
		MagicNative.magic_close(Magic);
	}
}