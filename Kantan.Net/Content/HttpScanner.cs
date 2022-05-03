#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using JetBrains.Annotations;
using Kantan.Diagnostics;
using Kantan.Net.Content.Resolvers;
using Kantan.Net.Properties;
using Kantan.Net.Utilities;
using Kantan.Utilities;

#endregion

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMember.Local

namespace Kantan.Net.Content;

// +++ H1
// ++ H2
// + H3
// - H4
// -- H5
// --- H6

/// <summary>
///     <a href="https://mimesniff.spec.whatwg.org/">Implements <em>MIME</em></a>
/// </summary>
public static class HttpScanner
{
	private const int RSRC_HEADER_LEN = 1445;

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#sniffing-a-mislabeled-binary-resource">7.2</a>
	/// </remarks>
	public static string IsBinaryResource(byte[] input)
	{
		int l = input.Length;

		byte[] seq1a = { 0xFE, 0xFF };
		byte[] seq1b = { 0xFF, 0xFE };
		byte[] seq2  = { 0xEF, 0xBB, 0xBF };

		switch (l) {
			case >= 2 when input.SequenceEqual(seq1a) || input.SequenceEqual(seq1b):
				return HttpType.MT_TEXT_PLAIN;
			case >= 3 when input.SequenceEqual(seq2):
				return HttpType.MT_TEXT_PLAIN;

		}

		if (!input.Any(IsBinaryDataByte)) {
			return HttpType.MT_TEXT_PLAIN;
		}

		return HttpType.MT_APPLICATION_OCTET_STREAM;
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#terminology">3</a>
	/// </remarks>
	public static bool IsBinaryDataByte(byte b)
	{
		return b is >= 0x00 and <= 0x08 or 0x0B or >= 0x0E and <= 0x1A or >= 0x1C and <= 0x1F;
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#reading-the-resource-header">5.2</a>
	/// </remarks>
	public static async Task<byte[]> ReadResourceHeader(Stream m)
	{
		int d = checked((int) m.Length);
		int l = d >= RSRC_HEADER_LEN ? RSRC_HEADER_LEN : d;

		byte[] data = new byte[l];

		int l2 = await m.ReadAsync(data, 0, l);

		return data;
	}

	public static bool CheckPattern(byte[] input, HttpType s, ISet<byte> ignored = null)
	{
		return CheckPattern(input, s.Pattern, s.Mask, ignored);
	}

	/// <remarks>
	///     <a href="https://mimesniff.spec.whatwg.org/#matching-a-mime-type-pattern">6</a>
	/// </remarks>
	public static bool CheckPattern(byte[] input, byte[] pattern, byte[] mask, ISet<byte> ignored = null)
	{
		Require.Assert(pattern.Length == mask.Length);

		ignored ??= Enumerable.Empty<byte>().ToHashSet();

		if (input.Length < pattern.Length) return false;

		int s = 0;

		while (s < input.Length) {
			if (!ignored.Contains(input[s])) break;

			s++;
		}

		int p = 0;

		while (p < pattern.Length) {
			int md = input[s] & mask[p];

			if (md != pattern[p]) {
				return false;
			}

			s++;
			p++;
		}

		return true;
	}


	public enum AltScannerType
	{
		Magic,
		File
	}

	[ItemCanBeNull]
	public static async Task<string> ScanAltAsync(string url, TimeSpan? ts = default,
	                                              AltScannerType m = AltScannerType.Magic)
	{
		ts ??= TimeSpan.FromMilliseconds(HttpUtilities.Timeout);
		

		switch (m) {

			case AltScannerType.Magic:
				// IFlurlResponse res    = await url.GetAsync();
				// var stream = await url.WithAutoRedirect(true).WithTimeout(ts.Value).AllowAnyHttpStatus().GetStreamAsync();


				try {
					var res = await url.WithAutoRedirect(true)
					                   .WithTimeout(ts.Value)
					                   .AllowAnyHttpStatus().GetAsync();

					var stream = await res.GetStreamAsync();
					
					var output1 = MagicResolver.Instance.Resolve(stream);

					// var rs      = new HttpResource() { Url = url, Stream = stream, Response = res, ResolvedTypes = { new HttpType(){ } }};

					return output1;


				}
				catch (Exception) {
					return null;
				}
			case AltScannerType.File:

				// IFlurlResponse res = await url.GetAsync();

				try {
					byte[] buf = await url.GetBytesAsync();

					byte[] hdr = buf[0..0xFF];

					string s = Path.GetTempFileName();

					await File.WriteAllBytesAsync(s, hdr);

					//@"C:\msys64\usr\bin\file.exe"
					// var name   = SearchInPath("file.exe");

					var args = $"{s} -i";

					var output = ProcessHelper.GetProcessOutput(Resources.EXE_FILE, args);

					File.Delete(s);

					return output;
				}
				catch (Exception) {
					return null;
				}
			default:
				return null;
		}

	}

	public static async Task<HttpResource[]> ScanAsync(string url, HttpResourceFilter filter = null)
	{
		filter ??= HttpResourceFilter.Default;

		List<string> urls = await filter.Extract(url);

		HttpResource[] hr = await Task.WhenAll(urls.Select(async Task<HttpResource>(s1) =>
		{
			HttpResource rsrc = await HttpResource.GetAsync(s1);
			rsrc?.Resolve();

			return rsrc;
		}));

		hr = hr.Where(x => x is { IsBinary: true }
			       /*&& x.Stream.Length >= filter.MinimumSize
			       && filter.TypeBlacklist.Contains(x.ComputedType)*/)
		       .ToArray();

		return hr;
	}
}