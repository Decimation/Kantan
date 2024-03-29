global using Assert = NUnit.Framework.Legacy.ClassicAssert;
global using CollectionAssert = NUnit.Framework.Legacy.CollectionAssert;
global using StringAssert = NUnit.Framework.Legacy.StringAssert;
global using DirectoryAssert = NUnit.Framework.Legacy.DirectoryAssert;
global using FileAssert = NUnit.Framework.Legacy.FileAssert;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Unicode;
using System.Threading.Tasks;
using Kantan.Collections;
using Kantan.Console;
using Kantan.Diagnostics;
using Kantan.Model;
using Kantan.Net;
using Kantan.Net.Utilities;
using Kantan.Numeric;
using Kantan.Text;
using Kantan.Utilities;
using NUnit.Framework;

// ReSharper disable PossibleMultipleEnumeration

// ReSharper disable MemberCanBePrivate.Local

// ReSharper disable UnusedMember.Local

// ReSharper disable InconsistentNaming
#pragma warning disable 649, IDE0059, IDE0060, CS0612, IDE0079

namespace UnitTest;

[TestFixture]
public class EnumerableTests
{

	[Test]
	public void CopyToListTest()
	{

		var range      = Enumerable.Range(1, 5);
		var array      = range.ToArray();
		var enumerable = (IEnumerable) range;
		Assert.True(enumerable.CastToList<int>().SequenceEqual(array));
		Assert.True(enumerable.CastToList().SequenceEqual(array.Cast<object>()));

	}

	/*
	[Test]
	public void TryIndexTest()
	{
		var rg = Enumerable.Range(1, 5).ToArray();
		var s  = new Span<int>(rg);

		for (int i = 0; i < s.Length; i++) {
			var x = s.TryIndex(i, out var v);
			Assert.True(x);
			Assert.AreEqual(s[i], v);
		}

		for (int i = s.Length; i < 100; i++) {
			var x = s.TryIndex(i, out var v);

			Assert.False(x);
			Assert.AreEqual(v, default(int));
		}
	}
	*/

	[Test]
	public void AllIndexesOfTest()
	{
		var s = "foobarfoobar";
		Assert.True(s.IndexOfAll("foo", StringComparison.Ordinal).SequenceEqual(new[] { 0, 6 }));
	}

	[Test]
	public void AllIndexesOfTest2()
	{
		var l = new List<int>() { 0, 1, 2, 4, 1, 4 };
		Assert.True(l.IndexOfAll(1).SequenceEqual(new[] { 1, 4 }));
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
#if OTHER
	
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
#endif

	[Test]
	public void EnumTest()
	{
		var name1 = "combo1";
		var p     = Enum.Parse<TestFlags>(name1);
		var flags = EnumHelper.GetSetFlags(p, false, false);
		var str   = flags.QuickJoin();

		Assert.AreEqual(str, "a, b, c, combo1");
	}

	[Test]
	public void EnumTest2()
	{
		var p     = TestFlags2.combo1;
		var flags = EnumHelper.GetSetFlags(p, false, true);
		TestContext.WriteLine(flags.QuickJoin());
		Assert.True(flags.SequenceEqual([TestFlags2.a, TestFlags2.b, TestFlags2.c]));
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
			Require.Assert(false);

		});

		Assert.Throws<NullReferenceException>(() =>
		{
			Require.NotNull(null);

		});

		Assert.Throws<ArgumentNullException>(() =>
		{
			Require.ArgumentNotNull(null);

		});

		Assert.Throws<Exception>(() =>
		{
			Require.Equal("a", "b");

		});

		Assert.DoesNotThrow(() =>
		{
			Require.Equal("a", "a");

		});

		Assert.Throws<ArgumentException>(() =>
		{
			Require.Argument(false, "g");
		});

		Assert.Throws<Exception>(() =>
		{
			Require.Fail();
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
	[TestCase("https://www.zerochan.net/2750747")]
	public void ResetRequestTest(string s)
	{
		var message = new HttpRequestMessage(method: HttpMethod.Get, s);
		var client  = new HttpClient();
		client.Send(message);
		Assert.True(message.IsSent());
		message.ResetStatus();
		Assert.False(message.IsSent());
		var r = client.Send(message);
		Assert.True(r.IsSuccessStatusCode);
	}

	[Test]
	public void StringTest()
	{
		Assert.Null(Strings.NormalizeNull("    "));
		Assert.Null(Strings.NormalizeNull(""));
		Assert.Null(Strings.NormalizeNull(null));
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
[Flags]
internal enum TestFlags2
{

	a = 1,
	b = 1 << 1,
	c = 1 << 2,

	combo1 = a|b | c,

}
internal class EnumerationTestType : Enumeration
{

	public static string str;
	public        string str2;

	public static readonly EnumerationTestType a1 = new(1, "g");

	public EnumerationTestType(int id, string name) : base(id, name) { }

}