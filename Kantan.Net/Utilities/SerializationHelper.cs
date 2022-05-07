using System.Json;

// ReSharper disable UnusedMember.Global

namespace Kantan.Net.Utilities;

public static class SerializationHelper
{
	public static JsonValue TryGetKeyValue(this JsonValue value, string k)
	{
		return value.ContainsKey(k) ? value[k] : null;
	}
}