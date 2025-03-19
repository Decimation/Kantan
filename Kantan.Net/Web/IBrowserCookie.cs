// Author: Deci | Project: Kantan.Net | Name: IBrowserCookie.cs
// Date: $File.CreatedYear-$File.CreatedMonth-22 @ 0:24:43

using System.Net;
using Flurl.Http;

namespace Kantan.Net.Web;

public interface IBrowserCookie
{

	public Cookie AsCookie();

	public FlurlCookie AsFlurlCookie();

	public string Name { get; }

}