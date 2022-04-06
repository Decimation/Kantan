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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
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
using Kantan.Net.Content;
using Kantan.Net.Media;
using Kantan.Net.Media.Filters;
using Kantan.Net.Media.Resolvers;
using Kantan.Text;
using Kantan.Utilities;
using Microsoft.VisualBasic.CompilerServices;
using HttpMethod = System.Net.Http.HttpMethod;

// ReSharper disable InconsistentNaming

// ReSharper disable MethodHasAsyncOverload

// ReSharper disable UnusedMember.Local

// ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060, CS1998, IDE0051,CS0169,4014,CS0649,IDE0044

namespace Test;

using System;

public static class Program
{
	private static void Test1()
	{
		var url = @"https://static.zerochan.net/Atago.%28Azur.Lane%29.full.2750747.png";

		var rg = new[]
		{
			"https://www.zerochan.net/2750747", "http://s1.zerochan.net/atago.(azur.lane).600.2750747.jpg",
			"https://twitter.com/mircosciamart/status/1186775807655587841"
		};

		var binaryUris = MediaSniffer.Scan(rg[^1], new MediaImageFilter())
		                             .Union(MediaSniffer.Scan(rg[1], new MediaImageFilter()));

		foreach (var v1 in rg) {
			var v2 = MediaSniffer.Scan(v1, MediaImageFilter.Default);

			foreach (MediaResource v in v2) {
				Console.WriteLine(v.Url);

			}
		}

		_ = new Url(rg[0]).Host;
		_ = new Uri(rg[0]).Host;
	}

	private static async Task Main(string[] args)
	{

		var rg = new[]
		{
			@"https://static.zerochan.net/Atago.%28Azur.Lane%29.full.2750747.png",
			@"https://i.imgur.com/QtCausw.png",
			"https://scontent-ord1-1.xx.fbcdn.net/t31.0-8/14715634_1300559193310808_8524406991247613051_o.jpg",
			"https://kemono.party/data/45/a0/45a04a55cdc142ee78f6f00452886bc4b336d9f35d3d851f5044852a7e26b5da.png"
		};

		foreach (string s in rg) {
			Console.WriteLine(s);

			/*try {
				Console.WriteLine(MediaResource.FromUrl(
					                  s, MediaImageFilter.Default, out var br));
			}
			catch (Exception e) {
				Console.WriteLine($"Failed 1");
			}*/

			var r = await MBinaryResource.GetResourceAsync(s);

			if (r== null) {
				Console.WriteLine("Failed 2");
			}
			else {
				var c = MScanner.Check(r);
				Console.WriteLine(c.QuickJoin());

			}
		}
	}
}