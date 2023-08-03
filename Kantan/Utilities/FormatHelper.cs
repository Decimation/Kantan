using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Utilities;

public static class FormatHelper
{
	public static string FormatBytes(long bytes)
	{

		if (bytes == 0) {
			return "0 " + Suffixes[0];
		}

		int    magnitude    = (int) Math.Floor(Math.Log(bytes, 1024));
		double adjustedSize = bytes / Math.Pow(1024, magnitude);

		return string.Format("{0:n2} {1}", adjustedSize, Suffixes[magnitude]);
	}

	private static HexFormatter Hex { get; } = new();

	public sealed class HexFormatter : ICustomFormatter
	{
		public string Format(string fmt, object arg, IFormatProvider formatProvider)
		{
			fmt ??= FMT_P;

			fmt = fmt.ToUpper(CultureInfo.InvariantCulture);
			string hexStr;

			if (arg is IFormattable f) {
				hexStr = f.ToString(HEX_FORMAT_SPECIFIER, null);
			}
			else {
				throw new NotImplementedException();
			}

			var sb = new StringBuilder();

			switch (fmt) {
				case FMT_P:
					sb.Append(HEX_PREFIX);
					goto case FMT_X;
				case FMT_X:
					sb.Append(hexStr);
					break;
				default:
					return arg.ToString();
			}

			return sb.ToString();

		}

		public const string HEX_FORMAT_SPECIFIER = "X";

		public const string HEX_PREFIX = "0x";

		public const string FMT_X = "X";
		public const string FMT_P = "P";
	}

	public static string ToHexString<T>(T t, string s = HexFormatter.FMT_P)
		=> Hex.Format(s, t, CultureInfo.CurrentCulture);

	private static readonly string[] Suffixes = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
}