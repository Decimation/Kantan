using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Kantan.Net;

public sealed class FirefoxCookieReader : BaseCookieReader
{
	public override async Task<List<KeyValuePair<string, object>>> ReadHostAsync(string host)
	{
		var dict = new List<KeyValuePair<string, object>>();
		var cmd  = Connection.CreateCommand();

		cmd.CommandText = """
				SELECT name,value
				FROM moz_cookies
				WHERE host like $host
				""";

		cmd.Parameters.AddWithValue("$host", host);

		await using var reader = await cmd.ExecuteReaderAsync();

		while (await reader.ReadAsync()) {
			var name  = reader.GetString(0);
			var value = reader.GetValue(1);

			dict.Add(new(name, value));
		}

		return dict;

	}

	public FirefoxCookieReader(string c) : base(c) { }

	public FirefoxCookieReader() : base($"Data Source={FindCookieFile().FullName}") { }

	public static FileInfo FindCookieFile()
	{
		//Data Source=C:\\Users\\Deci\\AppData\\Roaming\\Mozilla\\Firefox\\Profiles\\
		var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var dirs    = Directory.EnumerateDirectories(Path.Combine(roaming, "Mozilla", "Firefox", "Profiles"));
		var dir     = dirs.First(d => d.Contains("default"));
		var files   = Directory.EnumerateFiles(dir, "cookies.sqlite", SearchOption.TopDirectoryOnly);
		var file    = files.First();
		return new FileInfo(file);
	}
}