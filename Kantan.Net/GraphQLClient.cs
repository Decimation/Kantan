using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Utilities;
using JsonSerializer = System.Text.Json.JsonSerializer;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAnonymousTypePropertyName
#pragma warning disable IDE0037

namespace Kantan.Net;

/// <summary>
/// Simple GraphQL client
/// </summary>
public class GraphQLClient : IDisposable
{
	/*
	 * Adapted from https://github.com/latheesan-k/simple-graphql-client
	 */

	/*
	 * Adapted from https://github.com/latheesan-k/simple-graphql-client
	 */

	public FlurlClient Client { get; }

	public string Endpoint { get; }

	public GraphQLClient(string endpoint)
	{
		Endpoint = endpoint;

		Client = new FlurlClient()
		{
			BaseUrl = Endpoint,
			Settings =
				{ }
		};

	}

	public async Task<JsonNode> ExecuteAsync(string query, object variables = null,
	                                        Dictionary<string, string> additionalHeaders = null,
	                                        int timeout = -1)
	{

		var t = timeout == -1 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeout);

		var r = new FlurlRequest()
		{
			Settings =
			{
				Timeout = t,
				AllowedHttpStatusRange = "*"
			},
			Client = Client,
			Url    = Endpoint,
			Verb   = HttpMethod.Post
		};

		if (additionalHeaders is { Count: > 0 }) {
			foreach ((string key, string value) in additionalHeaders) {
				r.Headers.Add(key, value);
			}
		}

		var obj = new
		{
			query     = query,
			variables = variables
		};

		// var c = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

		var response = await r.PostJsonAsync(obj);
		var task     = await response.GetStringAsync();

		return JsonNode.Parse(task);
	}

	public void Dispose()
	{
		Client.Dispose();
		GC.SuppressFinalize(this);
	}
}