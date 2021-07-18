using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Net;

namespace Test
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			var png = "https://i.pximg.net/img-original/img/2019/10/23/08/48/13/77437257_p0.png";
			var s   = "https://image4.uhdpaper.com/wallpaper/azur-lane-atago-anime-girl-uhdpaper.com-4K-4.1734.jpg";
			var s2  = "https://i.imgur.com/QtCausw.png";

			Network.DumpResponse(Network.GetMetaResponse(s));
			Console.WriteLine(Network.IsType(png,"image"));
			Console.WriteLine(Network.IsType(s, "image"));
			Console.WriteLine(Network.IsType(s2, "image"));


		}
	}
}
