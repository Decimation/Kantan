// Author: Deci | Project: SmartImage.Lib | Name: SmartHttpListener.cs
// Date: 2024/11/21 @ 03:11:34

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;

namespace Kantan.Net;

// using Funcs = Dictionary<string, Func<byte[], string>>;
// using Funcs2 = Dictionary<string, SmartHttpListener.RequestDataCallback>;
using RouteCallbackMap = Dictionary<string, SmartHttpListener.HandleRequestCallback>;

// From https://github.com/chrishonselaar/ProtoPad/blob/master/ServiceDiscovery/SmartHttpListener.cs

public sealed class SmartHttpListener : IDisposable
{

	static SmartHttpListener()
	{
		Client = (FlurlClient) FlurlHttp.Clients.GetOrAdd(nameof(SmartHttpListener), null, builder =>
		{
			// builder.Settings.Redirects.ForwardAuthorizationHeader = true;
			// builder.Settings.Redirects.AllowSecureToInsecure      = true;

			builder.Settings.AllowedHttpStatusRange = "*";
			builder.AllowAnyHttpStatus();

			builder.OnError(f =>
			{
				f.ExceptionHandled = true;
				return;
			});

		});
	}

	public HttpListener Listener { get; }

	private const int ChunkSize = 1024;

	public RouteCallbackMap RequestHandlers { get; }

	public static FlurlClient Client { get; }

	public Encoding Encoding { get; }

	// public delegate Task<string> RequestDataCallback(byte[] buf);

	public delegate Task<string> HandleRequestCallback(HttpListenerRequest buf);

	public SmartHttpListener(RouteCallbackMap requestHandlers, int port)
		: this(requestHandlers, $"http://*:{port}/") { }

	public SmartHttpListener(RouteCallbackMap requestHandlers, [CBN] string uriPrefix)
	{
		if (String.IsNullOrWhiteSpace(uriPrefix)) {
			throw new ArgumentNullException(nameof(uriPrefix));
		}

		RequestHandlers = requestHandlers;

		Listener = new HttpListener()
		{
			TimeoutManager =
			{
				IdleConnection = Timeout.InfiniteTimeSpan,
			},
		};


		Listener.Prefixes.Add(uriPrefix);
		Encoding = Encoding.UTF8;

		// Start();

		// Debug.WriteLine("ProtoPad HTTP Server started");
	}


	public async Task StartAsync(CancellationToken ct = default)
	{
		if (!Listener.IsListening) {
			Listener.Start();

			// Listener.BeginGetContext(HandleRequest, Listener);

			while (Listener.IsListening) {
				var ctx = await Listener.GetContextAsync();

				var res = await HandleRequestAsync(ctx, ct);

				if (ct.IsCancellationRequested) {
					break;
				}
			}
		}
	}

	private async Task<bool> HandleRequestAsync(HttpListenerContext context, CancellationToken ct = default)
	{

		// var bytesRead = state.Stream.Length;

		/*var responseData = new byte[context.Request.ContentLength64];
		var cb           = await context.Request.InputStream.ReadAsync(responseData, 0, responseData.Length, ct);*/

		var data = context.Request;

		// var responseData = state.ResultBuffer;

		foreach (var requestHandler in RequestHandlers) {


			var requestUrl = data.Url;

			if (requestUrl != null && !requestUrl.PathAndQuery.Contains(requestHandler.Key))
				continue;

			var responseValue = await requestHandler.Value(data);
			var responseBytes = Encoding.GetBytes(responseValue);

			context.Response.ContentType     = MediaTypeNames.Text.Plain;
			context.Response.StatusCode      = (int) HttpStatusCode.OK;
			context.Response.ContentLength64 = responseBytes.Length;
			await context.Response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length, ct);

			context.Response.OutputStream.Dispose();

			// state.Dispose();
			context.Response.Close();

			return true;
		}

		return true;
	}


	[return: MN]
	public static async Task<IFlurlResponse> SendCustomCommandAsync(string ipAddress, string command,
	                                                                CancellationToken ct = default)
	{
		try {
			string s = $"{ipAddress}/{command}";

			var res = await Client.Request(s)
				          .GetAsync(cancellationToken: ct);
			return res;
		}
		catch {
			return null;
		}
	}

	[return: MN]
	public static async Task<IFlurlResponse> SendPostRequestAsync(string ipAddress, byte[] byteArray, string command,
	                                                              CancellationToken ct = default)
	{
		var s       = $"{ipAddress}/{command}";
		var request = Client.Request(s);
		request.Content.Headers.ContentLength = byteArray.Length;
		request.Content.Headers.ContentType   = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.FormUrlEncoded);
		request.Content                       = new ByteArrayContent(byteArray);

		try {
			var response = await request.SendAsync(HttpMethod.Post, cancellationToken: ct);

			return response;
		}
		catch (Exception) {
			return null;
		}
	}

	public void Dispose()
	{
		Listener?.Close();
	}

#if ASYNC_OLD
	private sealed class HttpResponseState : IDisposable
	{

		public HttpResponseState(HttpListenerRequest request, HttpListenerResponse response)
		{
			Request = request;
			Response = response;
			Stream = Request.InputStream;
			Buffer = new byte[ChunkSize];
			ResultBuffer = new ConcurrentBag<byte[]>();

			Trace.WriteLine(
				$"Alloc {nameof(HttpResponseState)} :: {Request.ContentLength64} {Response.ContentLength64}");
		}

		public Stream Stream { get; }

		public byte[] Buffer { get; }

		public ConcurrentBag<byte[]> ResultBuffer { get; }

		// public readonly ArrayPool<byte> ResultBuffer = ArrayPool<byte>.Create();

		public HttpListenerRequest Request { get; }

		public HttpListenerResponse Response { get; }

		public void Dispose()
		{
			Stream?.Dispose();
			((IDisposable) Response)?.Dispose();
			ResultBuffer.Clear();
		}

	}
	private void Start()
	{
		if (!Listener.IsListening) {
			Listener.Start();
			Listener.BeginGetContext(HandleRequest, Listener);
		}
	}


	private void Callback(IAsyncResult ar)
	{
		var state = (HttpResponseState) ar.AsyncState;

		if (state == null) {
			return;
		}

		var bytesRead = state.Stream.EndRead(ar);

		if (bytesRead > 0) {
			var buffer = new byte[bytesRead];
			Buffer.BlockCopy(state.Buffer, 0, buffer, 0, bytesRead);
			state.ResultBuffer.Add(buffer);
			state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, Callback, state);
		}
		else {
			state.Stream.Dispose();
			var responseData = state.ResultBuffer.SelectMany(static x => x).ToArray();

			// var responseData = state.ResultBuffer;

			foreach (var requestHandler in m_requestHandlers) {


				var requestUrl = state.Request.Url;

				if (requestUrl != null && !requestUrl.PathAndQuery.Contains(requestHandler.Key))
					continue;

				var responseValue = requestHandler.Value(responseData);
				var responseBytes = Encoding.UTF8.GetBytes(responseValue);

				state.Response.ContentType = MediaTypeNames.Text.Plain;
				state.Response.StatusCode = (int) HttpStatusCode.OK;
				state.Response.ContentLength64 = responseBytes.Length;
				state.Response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
				state.Response.OutputStream.Close();
				return;
			}
		}
	}

	private void HandleRequest(IAsyncResult result)
	{
		var context = Listener.EndGetContext(result);
		Listener.BeginGetContext(HandleRequest, Listener);

		var state = new HttpResponseState(context.Request, context.Response)
			{ };

		context.Request.InputStream.BeginRead(state.Buffer, 0, state.Buffer.Length, Callback, state);
	}
#endif

}