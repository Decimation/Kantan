using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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


	public HttpClient Client { get; private set; }

	public string Endpoint { get; }

	public GraphQLClient(string endpoint)
	{
		Endpoint = endpoint;

		Client = new HttpClient()
		{
			BaseAddress = new Uri(Endpoint),
		};

	}

	public dynamic Execute(string query, object variables = null, Dictionary<string, string> additionalHeaders = null,
	                       int timeout = -1)
	{
		Client = new HttpClient()
		{
			BaseAddress = new Uri(Endpoint),
		};

		Client.Timeout = timeout == -1 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeout);

		var request = new HttpRequestMessage(HttpMethod.Post, "/")
			{ };


		if (additionalHeaders is { Count: > 0 }) {
			foreach ((string key, string value) in additionalHeaders) {
				request.Headers.Add(key, value);
			}
		}

		var obj = new
		{
			query     = query,
			variables = variables
		};

		request.Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8,
		                                    "application/json");


		var response = Client.Send(request);
		var task     = response.Content.ReadAsStringAsync();
		task.Wait(Client.Timeout);


		return JObject.Parse(task.Result);
	}

	public void Dispose()
	{
		Client.Dispose();
		GC.SuppressFinalize(this);
	}
}