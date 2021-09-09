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


			var s = "foo".AddColor(Color.Aqua);
			Console.WriteLine(s);

			var s2 = Pastel.Remove(s);
			Console.WriteLine(s2);

			// await ConsoleTest();

			//ConsoleInterop.Init();

			var x = TestClass1.b | TestClass1.c1;
			Console.WriteLine(x);
			Console.WriteLine((1|2));
			Console.WriteLine(TestClass1.b.GetNextId());
			Console.WriteLine(TestClass1.b);
			Console.WriteLine(TestClass1.c1);
		}

		class TestClass1 : FlagsEnumeration
		{
			public TestClass1(int id, string name) : base(id, name) { }

			public static readonly TestClass1 z  = new TestClass1(0 << 0, null);
			public static readonly TestClass1 b  = new TestClass1(1 << 0, null);
			public static readonly TestClass1 c1 = new TestClass1(1 << 1, null);
			public static readonly TestClass1 d  = new TestClass1(1 << 2, null);

			public override TestClass1 Copy() => new(Id, Name);
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

			var r = ConsoleManager.ReadInputAsync(dialog);


			Task.Factory.StartNew(() =>
			{
				Thread.Sleep(1000);
				dialog.Options.Add(new ConsoleOption() { Name = "butt" });
			});

			await r;

			Console.WriteLine(r.Result.Output.QuickJoin());
			Console.WriteLine(r.Result.DragAndDrop);

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
			var dialog = new ConsoleDialog()
			{
				Functions = new()
				{
					[ConsoleKey.F1] = () =>
					{
						Console.WriteLine("g");

					},
				},
				SelectMultiple = true,
				Options        = ConsoleOption.FromEnum<MyEnum>().ToList()
			};


			var r = ConsoleManager.ReadInputAsync(dialog);
			await r;

			Console.WriteLine(r.Result.Output.QuickJoin());

		}
	}
}