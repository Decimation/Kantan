using System.IO;

namespace Kantan.Net.Media.Resolvers;

public interface IMediaTypeResolver
{
	string Resolve(Stream m);

	public static IMediaTypeResolver Default { get; set; } = new UrlmonResolver();
}