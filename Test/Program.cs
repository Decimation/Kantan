using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Cli;
using Kantan.Cli.Controls;
using Kantan.Collections;
using Kantan.Diagnostics;
using Kantan.Internal;
using Kantan.Model;
using Kantan.Native;
using Kantan.Native.Structures;
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
			/*var s = new string('-', Console.BufferWidth)+'\n'+'\n';
			int    f = Strings.MeasureRows(s);
			Console.WriteLine(s);
			Console.WriteLine(f);*/

			/*var s = new string('-', Console.BufferWidth + 1) + '\n';
			Console.WriteLine(s);
			Debug.WriteLine(Strings.MeasureRows(s));
			Debug.WriteLine(Console.CursorTop);*/


			/*string fooBar = "foo\nbar\n".AddColor(Color.Aquamarine);
			Console.Write(fooBar);
			Debug.WriteLine(Strings.MeasureRows(fooBar));*/
			await ConsoleTest3();
			// await ConsoleTest2();
			
		}



		private class MyClass : IOutline
		{
			/// <inheritdoc />
			public Dictionary<string, object> Outline
				=> new()
				{
					["a"] = "g",
					["x"] = "d",

				};
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

			var r = dialog.ReadInputAsync();


			Task.Factory.StartNew(() =>
			{
				Thread.Sleep(1000);
				dialog.Options.Add(new ConsoleOption() { Name = "butt" });
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
		private static async Task ConsoleTest3()
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
				Status = "hi",
				SelectMultiple = false,
				Options        = ConsoleOption.FromEnum<MyEnum>().ToList()
			};


			var r = dialog.ReadInputAsync();
			await r;

			Console.WriteLine(r.Result);

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
				Status = "hi1",
				SelectMultiple = true,
				Options        = ConsoleOption.FromEnum<MyEnum>().ToList()
			};


			var r = dialog.ReadInputAsync();
			await r;

			Console.WriteLine(r.Result.Output.QuickJoin());

		}
	}
}