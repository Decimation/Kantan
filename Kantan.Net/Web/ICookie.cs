// Author: Deci | Project: Kantan.Net | Name: IBrowserCookie.cs
// Date: $File.CreatedYear-$File.CreatedMonth-22 @ 0:24:43

using System.Net;
using Flurl.Http;
using JetBrains.Annotations;
using Kantan.Monad;
using Microsoft.Data.Sqlite;

namespace Kantan.Net.Web;

public interface ICookie
{

	public Cookie AsCookie();

	public FlurlCookie AsFlurlCookie([CBN] string originUrl = null);

	public string Name { get; }


}