#nullable disable
#pragma warning disable SYSLIB0014 // Type or member is obsolete

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using static Kantan.Diagnostics.LogCategories;

#pragma warning disable CS0618

// ReSharper disable LoopCanBeConvertedToQuery

#pragma warning disable 8602

// ReSharper disable UnusedMember.Global

namespace Kantan.Net;

public static class WebUtilities
{
	public static void OpenUrl(string url)
	{
		// https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
		// url must start with a protocol i.e. http://

		try {
			Process.Start(url);
		}
		catch {
			// hack because of this: https://github.com/dotnet/corefx/issues/10361
			if (OperatingSystem.IsWindows()) {
				url = url.Replace("&", "^&");

				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
				{
					CreateNoWindow = true
				});
			}
			else {
				throw;
			}
		}
	}

	

	#region Shortcuts

	public static IHtmlDocument GetHtmlDocument(string url)
	{
		string html   = GetString(url);
		var    parser = new HtmlParser();

		var document = parser.ParseDocument(html);
		return document;
	}

	public static string GetFile(string url, string folder)
	{
		string    fileName = Path.GetFileName(url);
		using var client   = new HttpClient();

		client.DefaultRequestHeaders.Add("User-Agent", "Other");
		string dir = Path.Combine(folder, fileName);
		client.DownloadFile(url, dir);

		return dir;

	}

	public static Stream GetStream(string url)
	{
		using var wc = new WebClient();
		return wc.OpenRead(url);
	}

	public static byte[] GetData(string url)
	{
		using var h = new HttpClient();
		return h.DownloadData(url);
	}

	public static string GetString(string url)
	{
		using var h = new HttpClient();
		return h.DownloadString(url);
	}

	#endregion


	#region HttpClient

	public static string DownloadString(this HttpClient client, string url)
	{
		var task = client.GetStringAsync(url);
		task.Wait();
		return task.Result;
	}

	public static Stream GetStream(this HttpClient client, string url)
	{
		var task = client.GetStreamAsync(url);
		task.Wait();
		return task.Result;
	}

	public static byte[] DownloadData(this HttpClient client, string url)
	{
		var task = client.GetByteArrayAsync(url);
		task.Wait();
		return task.Result;
	}

	public static string DownloadFile(this HttpClient client, string url)
	{
		var fname = Path.GetFileName(url);
		var tmp   = Path.Combine(Path.GetTempPath(), fname);

		return client.DownloadFile(url, tmp);
	}

	public static string DownloadFile(this HttpClient client, string url, string output)
	{
		var bytes = client.DownloadData(url);

		File.WriteAllBytes(output, bytes);

		return output;
	}

	#endregion
}
