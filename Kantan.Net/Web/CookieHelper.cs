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
}