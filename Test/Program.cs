using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
	private static async Task Main(string[] args)
	{


		/*var client = new HttpClient();
		var message = await client.GetAsync(@"https://static.zerochan.net/Atago.%28Azur.Lane%29.full.2750747.png");
		var sw = new Stopwatch();
		sw.Start();
		string b = message.Content.GetMediaTypeFromData();
		sw.Stop();
		Console.WriteLine(b);
		Console.WriteLine(message.Content.Headers.ContentType);
		Console.WriteLine(sw.Elapsed.TotalMilliseconds);*/

		// var url = @"https://static.zerochan.net/Atago.%28Azur.Lane%29.full.2750747.png";
		/*var url = @"https://i.imgur.com/QtCausw.png";
		RuntimeHelpers.RunClassConstructor(typeof(MediaSniffer).TypeHandle);
		var sw = Stopwatch.StartNew();
		Console.WriteLine(MediaSniffer.Resolve(url));
		sw.Stop();
		Console.WriteLine(sw.Elapsed.TotalSeconds);
		sw.Restart();
		Console.WriteLine(MediaSniffer.Resolve(url));
		sw.Stop();
		Console.WriteLine(sw.Elapsed.TotalSeconds);
		
		var u=HttpUtilities.GetHttpResponse(
			"https://scontent-ord1-1.xx.fbcdn.net/t31.0-8/14715634_1300559193310808_8524406991247613051_o.jpg");
		Console.WriteLine(u);
		Console.WriteLine(MediaResource.FromUrl(
			                  "https://scontent-ord1-1.xx.fbcdn.net/t31.0-8/14715634_1300559193310808_8524406991247613051_o.jpg",
			                  MediaImageFilter.Default, out var br));
			                  */
		// await ConsoleTest();


		var r = new[]{ 1 };
		r=r.Add(new[] { 2 });
		Console.WriteLine(r.QuickJoin());
	}

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

	private static void ConsoleTest4()
	{
		ConsoleManager.Init();

		ConsoleManager.Win32.WriteConsoleInput(ConsoleManager.StdIn, new[]
		{
			new InputRecord()
			{
				KeyEvent = new KeyEventRecord()
				{
					UnicodeChar = 'A'
				}
			}
		}, 1, out _);
	}

	private static ConsoleDialog _dialog;


	private class ConsoleOption1 : IConsoleOption
	{
		public string a;
		public int    x;

		public Dictionary<string, object> Data
			=> new()
			{
				["a"] = a,
				["x"] = x,

			};

		public ConsoleOption GetConsoleOption()
		{
			return null;
		}
	}

	private static async Task ConsoleTest()
	{
		var dialog = new ConsoleDialog()
		{
			Subtitle = "a\nb\nc",
			Functions = new()
			{
				[ConsoleKey.F1] = () =>
				{
					Console.WriteLine("g");

				},
			},
			//SelectMultiple = true,
			Options = ConsoleOption.FromArray(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }).ToList()
		};
		var myClass = new ConsoleOption1();
		dialog.Options[0].Data = myClass.Data;

		for (int i = 0; i < dialog.Options.Count; i++) {
			dialog.Options[i].Data = myClass.Data;
		}

		dialog.Options[0].Functions[ConsoleModifiers.Shift] = () =>
		{
			Console.WriteLine("shift");
			return null;
		};

		dialog.Options[0].Functions[ConsoleModifiers.Control] = () =>
		{
			Console.WriteLine("ctrl");
			return null;
		};

		dialog.Options[0].Functions[ConsoleModifiers.Control | ConsoleModifiers.Alt] = () =>
		{
			Console.WriteLine("alt+ctrl");
			return null;
		};

		var r = dialog.ReadInputAsync();


		Task.Factory.StartNew(() =>
		{
			Thread.Sleep(1000);
			dialog.Options.Add(new ConsoleOption() { Name = "test" });
		});

		await r;

		Console.WriteLine(r.Result);

	}

	[Flags]
	enum MyEnum1
	{
		a = 1 << 0,
		b = 1 << 1,
		c = 1 << 2
	}


	private static async Task ConsoleTest3(CancellationToken c)
	{
		var dialog = new ConsoleDialog()
		{
			Functions = new()
			{
				[ConsoleKey.F1] = () =>
				{
					Console.WriteLine("g");

				},
			},
			Status         = "hi1",
			SelectMultiple = true,
			Options        = ConsoleOption.FromEnum<MyEnum1>().ToList()
		};


		var r = dialog.ReadInputAsync(c);
		await r;

		Console.WriteLine(r.Result.Output.QuickJoin());

	}

	private static async Task ConsoleTest2()
	{
		var dialog = new ConsoleDialog()
		{
			Functions = new()
			{
				[ConsoleKey.F1] = () =>
				{
					Console.WriteLine("g");

				},
			},
			Status         = "hi1",
			SelectMultiple = true,
			Options        = ConsoleOption.FromEnum<MyEnum1>().ToList()
		};


		var r = dialog.ReadInputAsync();
		await r;

		Console.WriteLine(r.Result.Output.QuickJoin());

	}
}