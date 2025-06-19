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

public sealed class FirefoxCookiesDatabaseReader : BaseCookiesDatabaseReader
{

	// public FirefoxCookieReader(string c) : base(c) { }

	/// <example>
	/// <see cref="FindCookieFile"/> <see cref="FileInfo.FullName"/>
	/// </example>
	public FirefoxCookiesDatabaseReader([NN] string f) : base(f) { }

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

	public async Task<IList<ICookie>> ReadCookiesAsync(string host, bool wildcard = true)
	{
		var           list = new List<ICookie>();
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

		Func<SqliteDataReader, ICookie> func = static r => new FirefoxCookie(r);

		await foreach (var c in ReadToEndAsync(cmd, func)) {
			list.Add(c);
		}

		return list;

	}

	public override Task<IList<ICookie>> ReadCookiesAsync()
	{
		return ReadCookiesAsync("%");
	}

}