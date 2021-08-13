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
using Kantan.Internal;
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
				Functions = new Action[]
				{
					() =>
					{
						Console.WriteLine("g");

					},
				},
				Options = NConsoleOption.FromArray(new[] {1, 2, 3}).ToList()
			};

			var t=Task.Factory.StartNew(() =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(3));
				dialog.Options.Add(new NConsoleOption() {Name = "a"});
			});
			var r=NConsole.ReadOptionsAsync(dialog);
			await r;

			Console.WriteLine("--");
			Console.ReadKey();
			var r2=NConsole.ReadOptions(dialog);
			Console.WriteLine(r.Result.QuickJoin());
			Console.WriteLine(r2.QuickJoin());


		}
	}
}