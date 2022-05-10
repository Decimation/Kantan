using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Flurl.Http;

namespace Kantan.Net.Content.Resolvers;

public interface IFileTypeResolver : IDisposable
{
	string Resolve(Stream m);

	public static IFileTypeResolver Default { get; set; } = new MagicResolver();
}

