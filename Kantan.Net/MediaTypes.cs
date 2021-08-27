using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Kantan.Diagnostics;
using Newtonsoft.Json;
using RestSharp;

// ReSharper disable InconsistentNaming

// ReSharper disable IdentifierTypo
#pragma warning disable 8602
#pragma warning disable 8604
#pragma warning disable 8625
#pragma warning disable 8618
#pragma warning disable IDE0059

// ReSharper disable UnusedMember.Global
#nullable enable
using MA = System.Runtime.InteropServices.MarshalAsAttribute;
using UT = System.Runtime.InteropServices.UnmanagedType;

namespace Kantan.Net
{
	/// <summary>
	/// Media types, MIME types, etc.
	/// </summary>
	public static class MediaTypes
	{
		/*
		 * type/subtype
		 * type/subtype;parameter=value
		 */


		private const char DELIM = '/';

		private const int TYPE_I = 0;

		private const int SUBTYPE_I = 1;

		

		/// <summary>
		/// Whether the MIME <paramref name="mime"/> is of type <paramref name="type"/>
		/// </summary>
		public static bool IsTypeEqual(string mime, MediaType type) =>
			IsTypeEqual(mime, Enum.GetName(type));

		/// <summary>
		/// Whether the MIME <paramref name="mime"/> is of type <paramref name="type"/>
		/// </summary>
		public static bool IsTypeEqual(string mime, string type) =>
			GetTypeComponent(mime).StartsWith(type, StringComparison.InvariantCultureIgnoreCase);


		/*
		 * https://github.com/khellang/MimeTypes/blob/master/src/MimeTypes/MimeTypeFunctions.ttinclude
		 */

		public static string GetTypeComponent(string mime) => mime.Split(DELIM)[TYPE_I];

		public static string GetSubTypeComponent(string mime)
		{
			// NOTE: doesn't handle parameters
			return mime.Split(DELIM)[SUBTYPE_I];
		}

		[DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
		private static extern int FindMimeFromData(IntPtr pBC, [MA(UT.LPWStr)] string pwzUrl,
		                                           [MA(UT.LPArray, ArraySubType = UT.I1, SizeParamIndex = 3)]
		                                           byte[] pBuffer, int cbSize,
		                                           [MA(UT.LPWStr)] string pwzMimeProposed,
		                                           int dwMimeFlags, out IntPtr ppwzMimeOut, int dwReserved);

		public static string ResolveFromData(string url) => ResolveFromData(WebUtilities.GetStream(url));

		public static string ResolveFromData(Stream s)
		{
			using var ms = s as MemoryStream;

			const int BLOCK_SIZE = 256;

			byte[] rg = new byte[BLOCK_SIZE];

			int c = ms.Read(rg, 0, BLOCK_SIZE);

			string m = ResolveFromData(rg);

			return m;
		}

		public static string ResolveFromData(byte[] dataBytes, string? mimeProposed = null)
		{
			//https://stackoverflow.com/questions/2826808/how-to-identify-the-extension-type-of-the-file-using-c/2826884#2826884
			//https://stackoverflow.com/questions/18358548/urlmon-dll-findmimefromdata-works-perfectly-on-64bit-desktop-console-but-gener
			//https://stackoverflow.com/questions/11547654/determine-the-file-type-using-c-sharp
			//https://github.com/GetoXs/MimeDetect/blob/master/src/Winista.MimeDetect/URLMONMimeDetect/urlmonMimeDetect.cs

			Guard.AssertArgumentNotNull(dataBytes, nameof(dataBytes));

			string mimeRet = String.Empty;

			if (!String.IsNullOrEmpty(mimeProposed)) {
				//suggestPtr = Marshal.StringToCoTaskMemUni(mimeProposed); // for your experiments ;-)
				mimeRet = mimeProposed;
			}

			int ret = FindMimeFromData(IntPtr.Zero, null, dataBytes, dataBytes.Length,
			                           mimeProposed, 0, out var outPtr, 0);

			if (ret == 0 && outPtr != IntPtr.Zero) {
				string str = Marshal.PtrToStringUni(outPtr)!;

				Marshal.FreeHGlobal(outPtr);

				return str;
			}

			return mimeRet;
		}

		private const string DB_JSON_URL = "https://cdn.jsdelivr.net/gh/jshttp/mime-db@master/db.json";

		private static Dictionary<string, MediaTypeInfo> GetDatabase()
		{
			using var client = new WebClient();

			string json = client.DownloadString(new Uri(DB_JSON_URL));

			var mimeTypes = JsonConvert.DeserializeObject<Dictionary<string, MediaTypeInfo>>(json)!;

			return mimeTypes;
		}

		public static MediaTypeInfo GetInfo(string mime) => Database.Value[mime];

		public static IEnumerable<string> GetExtensions(string mime)
		{
			mime = mime.ToLower();

			return Database.Value.Where(kp => kp.Key == mime).SelectMany(kp => kp.Value.Extensions);
		}

		private static Lazy<Dictionary<string, MediaTypeInfo>> Database { get; } = new(GetDatabase);


		static MediaTypes() { }
	}

	public enum MediaType
	{
		Image,
		Video,
		Audio,
		Text,
		Application
	}

	public sealed class MediaTypeInfo
	{
		public MediaTypeInfo()
		{
			Extensions = new List<string>();
		}

		public string Source { get; set; }

		public List<string> Extensions { get; }

		public bool Compressible { get; set; }

		public string Charset { get; set; }
	}
}