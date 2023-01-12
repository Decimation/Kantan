using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Kantan.Net;

public abstract class BaseCookieReader : IDisposable
{
	public SqliteConnection Connection { get; }

	public FileInfo File { get; }

	public BaseCookieReader(string c)
	{
		Connection = new SqliteConnection(c);
		File = new FileInfo(c);
	}

	public async Task OpenAsync()
	{
		await Connection.OpenAsync();
	}

	public abstract Task<List<KeyValuePair<string, object>>> ReadHostAsync(string host);

	public void Dispose()
	{
		Connection?.Dispose();
	}
}