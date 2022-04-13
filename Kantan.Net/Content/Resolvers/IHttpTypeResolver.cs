using System;
using System.IO;

namespace Kantan.Net.Content.Resolvers;

public interface IHttpTypeResolver : IDisposable
{
	string Resolve(Stream m);

	public static IHttpTypeResolver Default { get; set; } = new UrlmonResolver();
}