using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.Data.Sqlite;

namespace Kantan.Net.Web;

public abstract class BaseCookieReader : IDisposable
{

	public SqliteConnection Connection { get; }

	public FileInfo File { get; }

	public BaseCookieReader([NN] string c, bool ds = true)
	{
		Requires.NotNull(c);

		Connection = new SqliteConnection(ds ? $"Data Source={c}" : c);
		File       = new FileInfo(c);
	}

	public Task OpenAsync()
	{
		return Connection.OpenAsync();
	}

	public abstract Task<List<IBrowserCookie>> ReadCookiesAsync();

	public void Dispose()
	{
		Connection?.Dispose();
	}

	

	public static async IAsyncEnumerable<IBrowserCookie> ReadToEndAsync(
		SqliteCommand cmd, Func<SqliteDataReader, IBrowserCookie> list)
	{
		//todo

		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync();

		while (await reader.ReadAsync()) {

			yield return list(reader);
		}
	}

}