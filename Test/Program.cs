using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Cli;
using Kantan.Internal;
using Kantan.Net;
using Kantan.Utilities;
using RestSharp;

// ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060, CS1998

namespace Test
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			/*var dialog = new NConsoleDialog()
			{
				Subtitle = "a\nb\nc",
				Functions = new Action[]
				{
					() =>
					{
						Console.WriteLine("g");

					},
				},
				Options = NConsoleOption.FromArray(new[] {1, 2, 3})
			};

			dialog.Read();*/

			var i = "https://litter.catbox.moe/tnneye.jpg";

			var jpg2 = "https://ascii2d.net/search/url/"+i;

			var jpg = "https://yandex.com/images/search?rpt=imageview&url=https://i.imgur.com/u92FZ6P.png";
			var w   =Network.GetHttpResponse(jpg, HttpMethod.Get, 5000);
			Console.WriteLine(w);
			Console.WriteLine(w.StatusCode);
			Console.WriteLine(w.Headers.Location);
			Console.WriteLine(w.TrailingHeaders.Location);
			foreach (KeyValuePair<string, IEnumerable<string>> header in w.Headers) {
				Console.WriteLine(header);
			}

			var v = AsyncHelpers.RunSync(() =>
			{
				return w.Content.ReadAsStringAsync();

			});
			//Console.WriteLine(v);
			Console.WriteLine(w.Content.ReadAsStringAsync().Result);
			
		}
	}
}