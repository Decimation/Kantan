using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Cli;
using Kantan.Collections;
using Kantan.Diagnostics;
using Kantan.Internal;
using Kantan.Model;
using Kantan.Native;
using Kantan.Net;
using Kantan.Numeric;
using Kantan.Text;
using Kantan.Utilities;
using Microsoft.Win32.SafeHandles;
using RestSharp;

// ReSharper disable UnusedMember.Local
#pragma warning disable 4014

// ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060, CS1998

namespace Test
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			Console.OutputEncoding = CodePagesEncodingProvider.Instance.GetEncoding(437);


			/*NativeInput.Init();

			while (!NativeInput.KeyAvailable) {
				
			}

			var r=NativeInput.Read();

			Console.WriteLine(r);*/

			await ConsoleTest();

			
			
		}


		

		class MyClass : IOutline
		{
			/// <inheritdoc />
			public Dictionary<string, object> Outline => new Dictionary<string, object>()
			{
				["a"] = "g",
				["x"] = "d",

			};
		}

		private static async Task ConsoleTest()
		{
			var dialog = new NConsoleDialog()
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
				Options = NConsoleOption.FromArray(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }).ToList()
			};
			dialog.Options[0].Data = new MyClass();

			for (int i = 0; i < dialog.Options.Count; i++) {
				dialog.Options[i].Data = new MyClass();
			}

			dialog.Options[0].Functions[ConsoleModifiers.Shift] = () =>
			{
				Console.WriteLine("butt");
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

			var r = NConsole.ReadInputAsync(dialog);


			Task.Factory.StartNew(() =>
			{
				Thread.Sleep(1000);
				dialog.Options.Add(new NConsoleOption() { Name = "butt" });
			});

			await r;

			Console.WriteLine(r.Result.Output.QuickJoin());

		}

		[Flags]
		enum MyEnum
		{
			a = 1 << 0,
			b = 1 << 1,
			c = 1 << 2
		}

		private static async Task ConsoleTest2()
		{
			var dialog = new NConsoleDialog()
			{
				Functions = new()
				{
					[ConsoleKey.F1] = () =>
					{
						Console.WriteLine("g");

					},
				},
				SelectMultiple = true,
				Options        = NConsoleOption.FromEnum<MyEnum>().ToList()
			};


			var r = NConsole.ReadInputAsync(dialog);
			await r;

			Console.WriteLine(r.Result.Output.QuickJoin());

		}
	}
}