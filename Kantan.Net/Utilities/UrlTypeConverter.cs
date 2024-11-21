using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flurl;

namespace Kantan.Net.Utilities;

public class UrlTypeConverter : JsonConverter<Url>
{

	public override Url Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.GetString();
	}

	public override void Write(Utf8JsonWriter writer, Url value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}

}