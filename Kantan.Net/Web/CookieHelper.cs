using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Kantan.Net.Web;

public static class CookieHelper
{

	public static IEnumerable<Cookie> ToCookies(Dictionary<string, string> kv, string uri)
	{
		return kv.Select(c => new Cookie(c.Key, c.Value, domain: new Uri(uri).Host, path: "/"));
	}

	/// <summary>
	/// Parses cookies encoded in <a href="https://curl.haxx.se/rfc/cookie_spec.html">Curl format</a>
	/// </summary>
	public static Cookie ParseCookie(string s)
	{
		var spl = s.Split('\t');

		string domain = spl[0],
		       flag   = spl[1],
		       path   = spl[2],
		       secure = spl[3],
		       expir  = spl[4],
		       name   = spl[5],
		       value  = spl[6];

		var ck = new Cookie(name, value, path, domain)
		{
			Expires = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expir)).DateTime,
			Secure  = Boolean.Parse(secure)
		};

		return ck;
	}

}