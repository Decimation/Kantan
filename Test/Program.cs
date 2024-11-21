using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using AngleSharp.Io.Network;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Util;
using JetBrains.Annotations;
using Kantan;
using Kantan.Collections;
using Kantan.Console;
using Kantan.Console.Cli;
using Kantan.Console.Cli.Controls;
using Kantan.Diagnostics;
using Kantan.Model;
using Kantan.Net.Properties;
using Kantan.Net.Utilities;
using Kantan.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.CompilerServices;
using Terminal.Gui;
using HttpMethod = System.Net.Http.HttpMethod;

#pragma warning disable CS0168

// ReSharper disable InconsistentNaming

// ReSharper disable MethodHasAsyncOverload

// ReSharper disable UnusedMember.Local

#pragma warning disable IDE0060, CS1998, IDE0051,CS0169,4014,CS0649,IDE0044,CS0612,CS0219, CS0169,IDE1006,IDE0059

namespace Test;

using System;
using Kantan.Model.MemberIndex;
using Kantan.Net.Web;

public static partial class Program
{

	private static string[] _rg1 = new[]
	{
		@"https://static.zerochan.net/Atago.%28Azur.Lane%29.full.2750747.png",
		@"https://i.imgur.com/QtCausw.png",
		@"https://www.deviantart.com/sciamano240",
		"https://www.zerochan.net/2750747",
		"http://s1.zerochan.net/atago.(azur.lane).600.2750747.jpg",
		"https://twitter.com/mircosciamart/status/1186775807655587841",
		@"https://en.wikipedia.org/wiki/Lambda_calculus",
		@"http://www.zerochan.net/2750747",
	};

	private static string[] _rg = new[]
	{
		@"http://www.zerochan.net/2750747",
		@"https://i.imgur.com/QtCausw.png",

		// @"https://i.pximg.net/img-master/img/2022/05/01/19/44/39/98022741_p0_master1200.jpg",
		"C:\\Users\\Deci\\Pictures\\Test Images\\Test1.jpg"
	};

	private static async Task Main(string[] args)
	{
		var c = new FirefoxCookieReader();
		await c.Connection.OpenAsync();

		foreach (IBrowserCookie cookie in (await c.ReadCookiesAsync("google"))) {
			Console.WriteLine(cookie);
		}
	}

	private static void Test2()
	{
		var butt    = new butt() { a = "foo", c=321 };
		var objects = MemberUtility.Select(butt, TestEnum.a | TestEnum.c | TestEnum.b);
		foreach (object o in objects) {
			Console.WriteLine(o);

		}

		Console.WriteLine(String.Join(',', objects));
	}

	[MemberIndexType(Formatter = typeof(bf), Mode = MemberIndexMode.Inclusive)]
	public class butt
	{

		public string a { get; init; }

		public int b;

		public int c { get; init; }

	}

	public class bf : IMemberIndexFormatter
	{

		public KeyValuePair<string, string> Format(object o, MemberInfo m)
		{
			return new($"g {m.Name}", "butt");
		}

	}
#if OBSOLETE
	public class TE1 : Enumeration<byte>
	{

		public static readonly TE1 a = new(0, "butt");
		public static readonly TE1 b = new(1, "hi");
		public static readonly TE1 c = new("waifu");

		public TE1(string name) : base(TE1.GetNextId<TE1>(), name) { }

		public TE1(byte id, string name) : base(id, name) { }

	}

	public class TestObj1 : IMemberTransformer
	{

		private int m_i = 1;

		public string p2 { get; set; }

		public string Ps => "hi";

		public int PI
		{
			get => m_i;
		}

	}

	private static void Test3()
	{
		var m = new MyClass() { i = 123, f = "foo" };

		foreach (var v in m.Data) {
			Console.WriteLine($"{v}");
		}
	}
	public class MyClass : IMap
	{

		public int i { get; set; }

		public string f;

		public Dictionary<string, object> Data
		{
			get => IMap.ToMap(this);
		}

	}
#endif

	[Flags]
	public enum TestEnum
	{
		None=0,
		a   = 1 << 0,
		b   = 1 << 1,
		c   = 1 << 2,
		all = a | b | c

	}

	public static Dictionary<MemberInfo, object> Dump(object obj, [CanBeNull] Func<MemberInfo, object> getValue = null)
	{
		var members = obj.GetType().GetMembers(BindingFlags.Instance    | BindingFlags.Default | BindingFlags.GetField |
		                                       BindingFlags.GetProperty | BindingFlags.Public  |
		                                       BindingFlags.NonPublic);

		getValue ??= m =>
		{
			object v;

			switch (m.MemberType) {
				case MemberTypes.Field:
					v = ((FieldInfo) m).GetValue(obj);
					break;

				case MemberTypes.Property:
					v = ((PropertyInfo) m).GetValue(obj);
					break;

				default:
					return null;
			}

			return v;
		};

		var map = new Dictionary<MemberInfo, object>();

		foreach (var m in members) {
			try {
				object v;
				v = getValue(m);

				if (v is not { }) {
					continue;
				}

				map.Add(m, v);
			}
			catch (Exception e) { }

		}

		return map;
	}

}