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
using Kantan.Model;
using Kantan.Net;
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
		var url = @"https://i.imgur.com/QtCausw.png";


		var x = Stopwatch.StartNew();

		using var request = new HttpRequestMessage
		{
			RequestUri = new Uri(url),
			Method     = HttpMethod.Get
		};
		var message = new HttpClient().Send(request);
		Console.WriteLine(message);
		x.Stop();
		Console.WriteLine(x.Elapsed.TotalMilliseconds);
		x.Restart();
		var n2 = await url.SendAsync(verb: HttpMethod.Get);
		x.Stop();
		Console.WriteLine(x.Elapsed.TotalMilliseconds);
		Console.WriteLine(n2);
		Console.WriteLine(await IPUtilities.GetIPInformationAsync());

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