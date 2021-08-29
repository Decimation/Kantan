using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Cli;
using Kantan.Collections;
using Kantan.Diagnostics;
using Kantan.Internal;
using Kantan.Model;
using Kantan.Net;
using Kantan.Numeric;
using Kantan.Text;
using Kantan.Utilities;
using RestSharp;

// ReSharper disable UnusedParameter.Local
#pragma warning disable IDE0060, CS1998

namespace Test
{
	public static class Program
	{
		private static async Task Main(string[] args)
		{
			
			Console.WriteLine(MathHelper.LCM(4,6));
			
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
				Options = NConsoleOption.FromArray(new[] { 1, 2, 3 }).ToList()
			};

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
			await r;

			Console.WriteLine(r.Result.QuickJoin());

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

			Console.WriteLine(r.Result.QuickJoin());

		}
	}
}