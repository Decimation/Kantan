using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;

namespace Kantan.Net.Web;

public sealed class FirefoxCookieReader : BaseCookieReader
{

	// public FirefoxCookieReader(string c) : base(c) { }

	/// <example>
	/// <see cref="FindCookieFile"/> <see cref="FileInfo.FullName"/>
	/// </example>
	public FirefoxCookieReader([NN] string f) : base(f) { }

	// public FirefoxCookieReader() : this(FindCookieFile()?.FullName) { }

	[CBN]
	public static FileInfo FindCookieFile()
	{
		var cf = FindCookieFiles();

		var infos = cf as FileInfo[] ?? cf.ToArray();

		if (!infos.Any()) {
			return null;
		}

		return infos.OrderByDescending(x => x.LastWriteTime)
			.FirstOrDefault(x => x.DirectoryName.Contains("default"));

	}

	public static IEnumerable<FileInfo> FindCookieFiles()
	{
		//Data Source=C:\\Users\\Deci\\AppData\\Roaming\\Mozilla\\Firefox\\Profiles\\

		var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var dirs    = Directory.EnumerateDirectories(Path.Combine(roaming, "Mozilla", "Firefox", "Profiles"));

		foreach (string dir in dirs) {
			var files = Directory.EnumerateFiles(dir, "cookies.sqlite", SearchOption.TopDirectoryOnly);

			foreach (string file in files) {
				yield return new FileInfo(file);
			}
		}

		// var dir   = dirs.First(d => d.Contains("default"));
		// var files = Directory.EnumerateFiles(dir, "cookies.sqlite", SearchOption.TopDirectoryOnly);
		// var file  = files.First();
		// return new FileInfo(file);
	}

	public async Task<IList<IBrowserCookie>> ReadCookiesAsync(string host, bool wildcard = true)
	{
		var           list = new List<IBrowserCookie>();
		SqliteCommand cmd  = Connection.CreateCommand();

		cmd.CommandText = """
		                  SELECT *
		                  FROM moz_cookies
		                  WHERE host like $host
		                  """;

		if (wildcard) {
			host = $"%{host}%";
		}

		cmd.Parameters.AddWithValue("$host", host);

		await foreach (var c in ReadToEndAsync(cmd, r => new FirefoxCookie(r))) {
			list.Add(c);
		}

		return list;

	}

	public override Task<IList<IBrowserCookie>> ReadCookiesAsync()
	{
		return ReadCookiesAsync("%");
	}

}