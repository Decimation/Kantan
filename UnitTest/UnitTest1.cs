using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Unicode;
using Kantan.Cli;
using Kantan.Collections;
using Kantan.Diagnostics;
using Kantan.Model;
using Kantan.Net;
using Kantan.Numeric;
using Kantan.Text;
using Kantan.Utilities;
using NUnit.Framework;

// ReSharper disable MemberCanBePrivate.Local

// ReSharper disable UnusedMember.Local

// ReSharper disable InconsistentNaming
#pragma warning disable 649, IDE0059

namespace UnitTest;

[TestFixture]
public class EnumerableTests
{

	[Test]
	public void Test1()
	{
		var rg = Enumerable.Range(1,5).ToArray();
		var s  = new Span<int>(rg);

		for (int i = 0; i < s.Length; i++) {
			var x = s.TryIndex(i, out var v);
			Assert.True(x);
			Assert.AreEqual(s[i],v );
		}

		for (int i = s.Length; i < 100; i++) {
			var x = s.TryIndex(i, out var v);

			Assert.False(x);
			Assert.AreEqual(v, default(int));
		}
	}

	[Test]
	public void AllIndexesOfTest()
	{
		var s = "foobarfoobar";
		Assert.True(s.AllIndexesOf("foo").SequenceEqual(new[] { 0, 6 }));
	}

	[Test]
	public void AllIndexesOfTest2()
	{
		var l = new List<int>() { 0, 1, 2, 4, 1, 4 };
		Assert.True(l.AllIndexesOf(1).SequenceEqual(new[] { 1, 4 }));
	}

	[Test]
	public void ReplaceAllSequencesTest2()
	{
		var rg      = new List<int> { 1, 2, 3, 9, 9, 9, 1, 2, 3 };
		var search  = new List<int> { 1, 2, 3 };
		var replace = new List<int> { 3, 2, 1 };

		var rg2  = rg.ReplaceAllSequences(search, replace);
		var rg2x = new List<int> { 3, 2, 1, 9, 9, 9, 3, 2, 1 };

		Assert.True(rg2.SequenceEqual(rg2x));

		var rg3 = new List<int>()
		{
			1, 2, 3, 3, 2, 1, 5, 6, 1, 2, 3, 5, 5, 5, 1, 2, 3
		};

		var rg3x = rg3.ReplaceAllSequences(new List<int>() { 1, 2, 3 }, new List<int>() { 4, 5, 6 });

		Assert.True(rg3x.SequenceEqual(new List<int>()
		{
			4, 5, 6, 3, 2, 1, 5, 6, 4, 5, 6, 5, 5, 5, 4, 5, 6
		}));
	}


	[Test]
	public void ReplaceAllSequencesTest()
	{
		var rg      = new List<int> { 1, 2, 3, 4, 5, 6, 3, 4, 5 };
		var search  = new List<int> { 3, 4, 5 };
		var replace = new List<int> { 5, 4, 3 };

		rg.ReplaceAllSequences(search, replace);

		TestContext.WriteLine($"{rg.QuickJoin()}");

		var rgNew = new List<int> { 1, 2, 5, 4, 3, 6, 5, 4, 3 };

		Assert.True(rg.SequenceEqual(rgNew));

		// var rg2      = new[] {"a", "foo", "bar", "hi"};
		// var search2  = new[] {"foo", "bar"};
		// var replace2 = new[] {"goo"};
		//
		//
		// rg2 = rg2.ReplaceAllSequences(search2, replace2);
		//
		// //TestContext.WriteLine($"{rg2.QuickJoin()}");
		// var rg2New = new[] {"a", "goo", "hi"};
		// Assert.True(rg2.SequenceEqual(rg2New));
	}
}

[TestFixture]
public class Tests
{
	[SetUp]
	public void Setup() { }

	[Test]
	public void CliTest()
	{
		TestFlags x = 0;

		var c = new CliHandler();

		c.Parameters.Add(new()
		{
			ParameterId = "-x",
			Function = o =>
			{
				x = Enum.Parse<TestFlags>(o[0]);
				return null;
			},
			ArgumentCount = 1
		});

		c.Run(new[] { "-x", "a,b" });

		Assert.AreEqual(TestFlags.a | TestFlags.b, x);

		//

		string x1 = null;
		c = new CliHandler();

		CliParameter item = new()
		{
			ParameterId = null,
			Function = o =>
			{
				x1 = o[0];
				return null;
			},
			ArgumentCount = 1
		};
		c.Default = item;

		c.Run(new[] { "hello" });

		Assert.AreEqual("hello", x1);

		//

		c = new CliHandler { Default = null };

		Assert.Throws<InvalidOperationException>(() =>
		{
			c.Run(new[] { "hello" });

		});

		//

		string s = null;

		c = new CliHandler
		{
			Default = new()
			{
				ParameterId = null,
				Function = o =>
				{
					s = o[0];
					return null;
				},
				ArgumentCount = 1
			}
		};

		Assert.DoesNotThrow(() =>
		{
			c.Run(new[] { "hello" });

		});
		Assert.AreEqual("hello", s);

	}

	[Test]
	public void MathTest()
	{
		Assert.AreEqual(MathHelper.Add(1, 1), 2);
		Assert.AreEqual(MathHelper.Subtract(1, 1), 0);

		Assert.AreEqual(MathHelper.Multiply(2, 2), 4);
		Assert.AreEqual(MathHelper.Divide(10, 5), 2);

	}

	[Test]
	public void EnumTest()
	{
		var name1 = "combo1";
		var p     = Enum.Parse<TestFlags>(name1);
		var flags = EnumHelper.GetSetFlags(p, false);
		var str   = flags.QuickJoin();

		Assert.AreEqual(str, "a, b, c, combo1");
	}

	[Test]
	public void EnumerationTest()
	{
		var rg = Enumeration.GetAll<EnumerationTestType>().ToArray();

		TestContext.WriteLine(rg.Length);

		foreach (var a in rg) {
			TestContext.WriteLine(a.ToString());
		}

		TestContext.WriteLine(Enumeration.GetNextId<EnumerationTestType>());
	}

	[Test]
	public void GuardTest()
	{
		Assert.Throws<Exception>(() =>
		{
			Guard.Assert(false);

		});

		Assert.Throws<NullReferenceException>(() =>
		{
			Guard.AssertNotNull(null);

		});

		Assert.Throws<ArgumentNullException>(() =>
		{
			Guard.AssertArgumentNotNull(null);

		});

		Assert.Throws<Exception>(() =>
		{
			Guard.AssertEqual("a", "b");

		});

		Assert.DoesNotThrow(() =>
		{
			Guard.AssertEqual("a", "a");

		});

		Assert.Throws<ArgumentException>(() =>
		{
			Guard.AssertArgument(false, "g");
		});

		Assert.Throws<Exception>(() =>
		{
			Guard.Fail();
		});
	}

	[Test]
	public void CharTest()
	{
		Assert.True(Strings.IsCharInRange(0x0400, UnicodeRanges.Cyrillic));
		Assert.True(Strings.IsCharInRange(0x04FF, UnicodeRanges.Cyrillic));

		Assert.False(Strings.IsCharInRange(0x04FF + 1, UnicodeRanges.Cyrillic));
		Assert.False(Strings.IsCharInRange(0x0, UnicodeRanges.Cyrillic));
		Assert.True(Strings.IsCharInRange('A', UnicodeRanges.BasicLatin));
		Assert.True(Strings.IsCharInRange(Strings.Constants.CHECK_MARK, UnicodeRanges.Dingbats));

	}

	[Test]
	public void StringTest()
	{
		Assert.Null(Strings.NormalizeNull("    "));
		Assert.Null(Strings.NormalizeNull(""));
		Assert.Null(Strings.NormalizeNull(null));
	}
}

[TestFixture]
public class NetworkTests
{
	[Test]
	public void GraphQLTest()
	{
		var g = new GraphQLClient("https://graphql.anilist.co/");

		var query = @"query ($id: Int) { # Define which variables will be used in the query (id)
				Media(id: $id, type: ANIME) { # Insert our variables into the query arguments (id) (type: ANIME is hard-coded in the query)
					id
					title {
						romaji
						english
						native
					}
				}
			}";

		dynamic execute = g.Execute(query);

		Assert.True(execute.data.Media.title.english.ToString().Contains("Cowboy Bebop"));
	}

	[Test]
	[TestCase(@"https://i.imgur.com/QtCausw.png", true)]
	[TestCase(@"http://tidder.xyz/?imagelink=https://i.imgur.com/QtCausw.png", false)]
	[TestCase(@"http://tidder.xyz/", false)]
	[TestCase(@"https://i.imgur.com/QtCausw.png", true)]
	public void UriAliveTest(string s, bool b)
	{
		//Assert.AreEqual(b, Network.IsAlive(new Uri((s))));
		Assert.Pass();
	}

	[Test]
	public void MediaTypesTest()
	{
		const string jpg = "https://i.ytimg.com/vi/r45a-l9Gqdk/hqdefault.jpg";

		var i = HttpUtilities.GetHttpResponse(jpg).Content.Headers.ContentType.ToString();
		Assert.True(UriUtilities.IsUri(jpg, out var u));
		Assert.True(MediaTypes.GetExtensions(i).Contains("jpe"));
		Assert.True(MediaTypes.GetTypeComponent(i) == "image");
		Assert.True(MediaTypes.GetSubTypeComponent(i) == "jpeg");
	}

	[Test]
	[TestCase(
		"https://danbooru.donmai.us/data/original/ca/7b/__re_l_mayer_ergo_proxy_drawn_by_koyorin__ca7b942f24b30e3a7a6f49b932fa2d56.png?download=1",
		"https://danbooru.donmai.us/data/original/ca/7b/__re_l_mayer_ergo_proxy_drawn_by_koyorin__ca7b942f24b30e3a7a6f49b932fa2d56.png")]
	public void UrlUtilTest(string a, string b)
	{
		Assert.True(UriUtilities.UrlEqual(a, b));
	}
}

[Flags]
internal enum TestFlags
{
	a = 0,
	b = 1 << 0,
	c = 1 << 1,

	combo1 = b | c,
}

internal class EnumerationTestType : Enumeration
{
	public static string str;
	public        string str2;

	public static readonly EnumerationTestType a1 = new(1, "g");

	public EnumerationTestType(int id, string name) : base(id, name) { }
}