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

public abstract class BaseCookiesDatabaseReader : IDisposable
{

	public SqliteConnection Connection { get; }

	public FileInfo File { get; }

	public BaseCookiesDatabaseReader([NN] string c, bool ds = true)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(c);

		Connection = new SqliteConnection(ds ? $"Data Source={c}" : c);
		File       = new FileInfo(c);
	}

	/*
	public Task OpenAsync()
	{
		return Connection.OpenAsync();
	}
	*/

	public abstract Task<IList<ICookie>> ReadCookiesAsync();

	public void Dispose()
	{
		Connection?.Dispose();
	}

	

	public static async IAsyncEnumerable<ICookie> ReadToEndAsync(SqliteCommand cmd, Func<SqliteDataReader, ICookie> list)
	{
		//todo

		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync();

		while (await reader.ReadAsync()) {

			yield return list(reader);
		}
	}

}