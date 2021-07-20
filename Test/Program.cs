using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Internal;
using Kantan.Net;
using RestSharp;

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

			Console.WriteLine(Common.AppFolder);

			Console.WriteLine(Network.GetExternalIP());
			var response = Network.GetResponse(png, 1000);
			Console.WriteLine(response);
			Network.DumpResponse(response);
			Console.WriteLine(Network.GetFinalRedirect("https://en.wikipedia.org/wiki/Boolean_algebra?wprov=svl"));
			Console.WriteLine(MediaTypes.GetMediaType(png));
			Console.WriteLine(Network.GetFinalRedirect(s));
			Console.WriteLine(MediaTypes.IsUrlType(png, MediaTypes.Text));
		}
	}
}
