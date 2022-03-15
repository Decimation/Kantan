using System.IO;

namespace Kantan.Net.Media.Resolvers;

public interface IMediaTypeResolver
{
	string Resolve(Stream m);
}