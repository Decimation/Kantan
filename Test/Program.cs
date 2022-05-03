using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using AngleSharp.Io.Network;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Util;
using Kantan.Cli;
using Kantan.Cli.Controls;
using Kantan.Collections;
using Kantan.Internal;
using Kantan.Model;
using Kantan.Net;
using Kantan.Net.Adapters;
using Kantan.Net.Content;
using Kantan.Net.Content.Resolvers;
using Kantan.Net.Properties;
using Kantan.Text;
using Kantan.Utilities;
using Microsoft.VisualBasic.CompilerServices;
using HttpMethod = System.Net.Http.HttpMethod;

// ReSharper disable InconsistentNaming

// ReSharper disable MethodHasAsyncOverload

// ReSharper disable UnusedMember.Local

// ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060, CS1998, IDE0051,CS0169,4014,CS0649,IDE0044,CS0612

namespace Test;

using System;

public static class Program
{
	private static string[] _rg1 = new[]
	{
		@"https://static.zerochan.net/Atago.%28Azur.Lane%29.full.2750747.png",
		@"https://i.imgur.com/QtCausw.png",
		@"https://www.deviantart.com/sciamano240",
		"https://www.zerochan.net/2750747",
		"http://s1.zerochan.net/atago.(azur.lane).600.2750747.jpg",
		"https://twitter.com/mircosciamart/status/1186775807655587841",
		@"https://en.wikipedia.org/wiki/Lambda_calculus",
		@"http://www.zerochan.net/2750747",
		"https://scontent-ord1-1.xx.fbcdn.net/t31.0-8/14715634_1300559193310808_8524406991247613051_o.jpg",
		"https://kemono.party/data/45/a0/45a04a55cdc142ee78f6f00452886bc4b336d9f35d3d851f5044852a7e26b5da.png"
	};

	private static string[] _rg = new[]
	{
		@"http://www.zerochan.net/2750747",
		@"https://kemono.party/patreon/user/3332300/post/65227512"
	};

	private static async Task Main(string[] args)
	{

		// var v = await Kantan.Net.Content.HttpScanner.ScanAsync(_rg[0], HttpResourceFilter.Default);

		/*
		foreach (var a in _rg) {
			foreach (var aa in await HttpScanner.ScanAsync(a)) {
				Console.WriteLine(aa.Url);
			}
		}*/

		// Debugger.Break();

		var u2 =
			"https://data19.kemono.party/data/1e/90/1e90c71e9bedc2998289ca175e2dcc6580bbbc3d3c698cdbb0f427f0a0d364b7.png?f=Bianca%20bunny%201-3.png";

		// var u = CliAdapters.gallery_dl_resolve("https://kemono.party/patreon/user/3332300/post/65227512");
		// Console.WriteLine(u);

		/*
		 * image/png
text/html
0.8617613
text/html
0.4304751
image/png
1.6637436
image/jpeg
0.3022393
text/html
0.711541
text/html
0.4215932
image/jpeg
0.9666091
text/html
0.4441395
text/html
0.1990817

0.0896905
image/png
1.3038885
		 */

		// KantanNetInit.Close();
		foreach (string s in _rg.Union(_rg1)) {
			var now = Stopwatch.GetTimestamp();
			var o   = await HttpResource.GetAsync(s);
			o?.Resolve(true);
			Console.WriteLine(o);
			var diff = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - now);
			Console.WriteLine(diff.TotalSeconds);

		}

	}
}