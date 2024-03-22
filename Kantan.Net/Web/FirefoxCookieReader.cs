using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Kantan.Net.Web;

public sealed class FirefoxCookieReader : BaseCookieReader
{

	public FirefoxCookieReader(string c) : base(c) { }

	public FirefoxCookieReader() : base($"Data Source={FindCookieFile().FullName}") { }

	public static FileInfo FindCookieFile()
	{
		var cf = FindCookieFiles();

		return cf.OrderByDescending(x => x.LastWriteTime)
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

	public override async Task<List<IBrowserCookie>> ReadCookiesAsync()
	{
		var           dict = new List<IBrowserCookie>();
		SqliteCommand cmd  = Connection.CreateCommand();
		cmd.CommandText = "select * from moz_cookies";

		/*cmd.CommandText = """
						  SELECT name,value
						  FROM moz_cookies
						  WHERE host like $host
						  """;

		cmd.Parameters.AddWithValue("$host", host);*/

		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync();

		while (await reader.ReadAsync()) {

			dict.Add(new FirefoxCookie(reader));
		}

		return dict;

	}

}