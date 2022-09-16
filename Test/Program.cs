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
using Kantan.Cli;
using Kantan.Cli.Controls;
using Kantan.Collections;
using Kantan.Model;
using Kantan.Net;
using Kantan.Net.Properties;
using Kantan.Text;
using Kantan.Utilities;
using Microsoft.VisualBasic.CompilerServices;
using HttpMethod = System.Net.Http.HttpMethod;

// ReSharper disable InconsistentNaming

// ReSharper disable MethodHasAsyncOverload

// ReSharper disable UnusedMember.Local

#pragma warning disable IDE0060, CS1998, IDE0051,CS0169,4014,CS0649,IDE0044,CS0612,CS0219, CS0169

namespace Test;

using System;

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
		"https://scontent-ord1-1.xx.fbcdn.net/t31.0-8/14715634_1300559193310808_8524406991247613051_o.jpg",
		"https://kemono.party/data/45/a0/45a04a55cdc142ee78f6f00452886bc4b336d9f35d3d851f5044852a7e26b5da.png"
	};

	private static string[] _rg = new[]
	{
		@"http://www.zerochan.net/2750747",
		@"https://i.imgur.com/QtCausw.png",
		@"https://kemono.party/patreon/user/3332300/post/65227512",
		// @"https://i.pximg.net/img-master/img/2022/05/01/19/44/39/98022741_p0_master1200.jpg",
		"C:\\Users\\Deci\\Pictures\\Test Images\\Test1.jpg"
	};

	private static async Task Main(string[] args)
	{

	}

	public static Dictionary<MemberInfo, object> Dump(object obj, [CanBeNull] Func<MemberInfo, object> getValue = null)
	{
		var members = obj.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Default | BindingFlags.GetField |
		                                       BindingFlags.GetProperty | BindingFlags.Public |
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

	private static async Task Test1()
	{
		// var v = await Kantan.Net.Content.HttpScanner.ScanAsync(_rg[0], HttpResourceSniffer.Default);

		/*
		foreach (var a in _rg) {
			foreach (var aa in await HttpScanner.ScanAsync(a)) {
				Console.WriteLine(aa.Value);
			}
		}*/

		// Debugger.Break();

		var u2 =
			"https://data19.kemono.party/data/1e/90/1e90c71e9bedc2998289ca175e2dcc6580bbbc3d3c698cdbb0f427f0a0d364b7.png?f=Bianca%20bunny%201-3.png";

		// var u = CliAdapters.gallery_dl_resolve("https://kemono.party/patreon/user/3332300/post/65227512");
		// Console.WriteLine(u);

		// KantanNetInit.Close();

		/*foreach (string s in _rg.Union(_rg1)) {
			Console.WriteLine(s);

			var now = Stopwatch.GetTimestamp();
			var o   = await ResourceHandle.GetAsync(s, true) as HttpResourceHandle;
			o?.Resolve(true);

			if (o is not { }) {
				Console.WriteLine($"failed");
				continue;
			}

			Console.WriteLine(o);
			Console.WriteLine(o.NoSniffFlag);

			var diff = TimeSpan.FromTicks(Stopwatch.GetTimestamp() - now);
			Console.WriteLine(diff.TotalSeconds);
		}*/
	}
}