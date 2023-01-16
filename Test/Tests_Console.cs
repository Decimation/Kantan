#pragma warning disable CS0612, CS0649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Console.Cli;
using Kantan.Console.Cli.Controls;
using Kantan.Text;

namespace Test;

[Flags]
public enum MyEnum1
{
	a = 1 << 0,
	b = 1 << 1,
	c = 1 << 2
}
#pragma warning disable CS0162

public static partial class Program
{
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

#pragma warning disable CS0169
	private static ConsoleDialog _dialog;
#pragma warning restore CS0169

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

	/*public static async Task ConsoleTest()
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

	}*/

	public static async Task ConsoleTest3(CancellationToken c)
	{
		var dialog = new ConsoleDialog()
		{
			Functions = new()
			{
				[ConsoleKey.F1] = () =>
				{
					Console.WriteLine("g");
					Thread.Sleep(TimeSpan.FromSeconds(1));

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

	public static async Task ConsoleTest2()
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