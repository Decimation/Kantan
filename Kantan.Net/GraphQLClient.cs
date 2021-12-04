using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Internal;
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


	private HttpClient m_client;

	private readonly string m_apiUrl;

	public GraphQLClient(string apiUrl)
	{
		m_apiUrl = apiUrl;

		ServicePointManager.SecurityProtocol =
			SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
	}

	public dynamic Execute(string query, object variables = null,
	                       Dictionary<string, string> additionalHeaders = null,
	                       int timeout = 0)
	{
		m_client = new HttpClient()
		{
			BaseAddress = new Uri(m_apiUrl),
		};

		m_client.Timeout = timeout == 0 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeout);

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

		var clone = ReflectionHelper.Clone(m_client);

		var response = m_client.Send(request);
		var task     = response.Content.ReadAsStringAsync();
		task.Wait(m_client.Timeout);

		/*request.ResetStatus();
		_client.ResetStatus();*/

		m_client = clone;

		return JObject.Parse(task.Result);
	}

	public void Dispose()
	{
		m_client.Dispose();
	}
}