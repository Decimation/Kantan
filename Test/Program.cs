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
			var dialog = new NConsoleDialog()
			{
				Subtitle = "a\nb\nc",
				Functions = new()
				{
					[ConsoleKey.F1]=() =>
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
			}; dialog.Options[0].Functions[ConsoleModifiers.Control | ConsoleModifiers.Alt] = () =>
			{
				Console.WriteLine("alt+ctrl");
				return null;
			};
			
			var r = NConsole.ReadOptionsAsync(dialog);
			await r;
			
			Console.WriteLine(r.Result.QuickJoin());
			
		}
	}
}