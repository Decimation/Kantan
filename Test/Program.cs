using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Cli;
using Kantan.Cli.Controls;
using Kantan.Collections;
using Kantan.Model;
using Kantan.Net;
using Kantan.Text;
using Kantan.Utilities;
using Microsoft.VisualBasic.CompilerServices;
// ReSharper disable InconsistentNaming


// ReSharper disable MethodHasAsyncOverload

// ReSharper disable UnusedMember.Local

// ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060, CS1998, IDE0051,CS0169,4014,CS0649


namespace Test;

public static class Program
{
	private static async Task Main(string[] args)
	{
		/*var u= @"https://static.zerochan.net/Atago.%28Azur.Lane%29.full.2750747.png";
		var r = HttpUtilities.GetHttpResponse(u);

		var b = r.Content.ReadAsByteArrayAsync();
		b.Wait();
		Console.WriteLine(r.Content.Headers.ContentType.MediaType);
		Console.WriteLine(MediaTypes.ResolveFromData(b.Result));*/


		/*var task = Task.Run(() =>
		{
			Thread.Sleep(1000);
			return 1;

		});
		var xc = task.RunSync();
		Console.WriteLine(xc);
		task = Task.Run(() =>
		{
			Thread.Sleep(1000);
			return 1;

		});
		Console.WriteLine(task.Result);
		task = Task.Run(() =>
		{
			Thread.Sleep(1000);
			return 1;

		});
		Console.WriteLine(task.RunSync());*/


		/*foreach (var v in BinaryResourceSniffer.Scan("https://www.zerochan.net/2750747", b: new BinaryImageFilter())) {
			Console.WriteLine(v.Url);
		}*/

		var u = "https://www.zerochan.net/2750747";

		var v =BinaryResourceSniffer.Scan(u, BinaryResourceSniffer.ImageFilter);

		foreach (object o in v) {
			Console.WriteLine(o);
		}
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


	private class MyClass : IConsoleOption
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
		var myClass = new MyClass();
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
	enum MyEnum
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
			Options        = ConsoleOption.FromEnum<MyEnum>().ToList()
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
			Options        = ConsoleOption.FromEnum<MyEnum>().ToList()
		};


		var r = dialog.ReadInputAsync();
		await r;

		Console.WriteLine(r.Result.Output.QuickJoin());

	}
}