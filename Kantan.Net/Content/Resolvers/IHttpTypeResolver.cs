using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Flurl.Http;

namespace Kantan.Net.Content.Resolvers;

public interface IHttpTypeResolver : IDisposable
{
	string Resolve(Stream m);

	public static IHttpTypeResolver Default { get; set; } = new UrlmonResolver();
}

