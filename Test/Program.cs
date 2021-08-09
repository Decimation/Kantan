using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		[Flags]
		private enum flag
		{
			a=1,
			b=1<<1,
			c=1<<2
		}
		private static async Task Main(string[] args)
		{
			flag x = 0;

			var c = new CliHandler();

			c.Parameters.Add(new()
			{
				ParameterId = "-x", Function = o =>
				{
					Debug.WriteLine($"{o[0]}");
					
					x = Enum.Parse<flag>(o[0]);
					return null;
				},
				ArgumentCount = 1
			});

			c.Run();

			Console.WriteLine(x);

			//new NConsoleDialog() {Subtitle = "a\nb\nc", Options = NConsoleOption.FromArray(new[]{1,2,3})}.Read();
		}
	}
}