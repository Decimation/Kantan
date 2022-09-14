using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.FileTypes;
using Kantan.FileTypes.Impl;
using Kantan.Net;
using Kantan.Net.Utilities;
using Kantan.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;

namespace UnitTest;

[TestFixture]
public class Tests3
{
	[Test]
	[TestCase(@"C:\Users\Deci\Pictures\NSFW\17EA29A6-8966-4801-A508-AC89FABE714D.png")]
	public async Task Test1(string s)
	{
		var stream = File.OpenRead(s);
		var task   = await IFileTypeResolver.Default.ResolveAsync(stream);
		Assert.True(task.Any(x => x.IsType(FileType.MT_IMAGE)));
	}

	/*[Test]
	[TestCase(@"https://kemono.party/patreon/user/587897/post/64451923","image/png")]
	public async Task Test2(string s,string s2)
	{
		var result = await HttpResourceSniffer.Default.ScanAsync(s);

		foreach (HttpResourceHandle httpResource in result) {
			httpResource.Resolve();
		}

		Assert.True(result.Any(x=>x.ResolvedTypes.Select(x=>x.Type).Contains(s2)));
	}*/
}

[TestFixture]
public class MimeTypeTests
{
	[Test]
	[TestCase("http://s1.zerochan.net/atago.(azur.lane).600.2750747.jpg")]
	// [TestCase("https://www.zerochan.net/2750747", "http://static.zerochan.net/atago.(azur.lane).full.2750747.png")]
	// [TestCase(@"C:\Users\Deci\Pictures\NSFW\17EA29A6-8966-4801-A508-AC89FABE714D.png", true, false)]
	// [TestCase("http://s1.zerochan.net/atago.(azur.lane).600.2750747.jpg", false, true)]
	[TestCase("https://kemono.party/data/45/a0/45a04a55cdc142ee78f6f00452886bc4b336d9f35d3d851f5044852a7e26b5da.png")]
	[TestCase(
		"https://data19.kemono.party/data/1e/90/1e90c71e9bedc2998289ca175e2dcc6580bbbc3d3c698cdbb0f427f0a0d364b7.png?f=Bianca%20bunny%201-3.png")]
	public async Task Test1(string u)
	{
		// var binaryUris = MediaSniffer.Scan(u, new HttpMediaResourceFilter());
		// Assert.True(binaryUris.Select(x => x.Value.ToString()).ToList().Contains(s));

		var stream = await u.GetStreamAsync();
		var ft     = await IFileTypeResolver.Default.ResolveAsync(stream);
		Assert.True(ft.Any(f => f.IsType(FileType.MT_IMAGE)));
	}
}

[TestFixture]
public class NetworkTests
{
	[Test]
	public async Task GraphQLTest()
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

		dynamic execute = await g.ExecuteAsync(query);

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
	/*

	[Test]
	public void MediaTypesTest()
	{
		const string jpg = "https://i.ytimg.com/vi/r45a-l9Gqdk/hqdefault.jpg";

		var message = HttpUtilities.GetHttpResponse(jpg);
		Assert.True(UriUtilities.IsUri(jpg, out var u));
		Assert.True(message.ResponseMessage.Content.Resolve().Contains("jpeg"));

	}*/

	/*[Test]
	public void MediaTypesTest2()
	{
		var u = @"https://static.zerochan.net/Atago.%28Azur.Lane%29.full.2750747.png";
		var r = HttpUtilities.GetHttpResponse(u);

		if (r == null) {
			Assert.Inconclusive();
		}

		string type = r.ResponseMessage.Content.Headers.ContentType.MediaType;

		var type2 = MediaSniffer.Resolve(r.ResponseMessage.Content);

		Assert.AreEqual(type2, "image/png");
	}
	*/

	[Test]
	[TestCase(
		"https://danbooru.donmai.us/data/original/ca/7b/__re_l_mayer_ergo_proxy_drawn_by_koyorin__ca7b942f24b30e3a7a6f49b932fa2d56.png?download=1",
		"https://danbooru.donmai.us/data/original/ca/7b/__re_l_mayer_ergo_proxy_drawn_by_koyorin__ca7b942f24b30e3a7a6f49b932fa2d56.png")]
	public void UrlUtilTest(string a, string b)
	{
		Assert.True(UriUtilities.UrlEqual(a, b));
	}
}

[TestFixture]
public class Tests4
{
	[Test]
	[TestCase("https://i.imgur.com/QtCausw.png", "image/jpeg")]
	[TestCase("http://static.zerochan.net/atago.(azur.lane).full.2750747.png", "image/png")]
	public async Task Test1(string s, string type)
	{
		var t  = await s.GetAsync();
		var tt = await (IFileTypeResolver.Default.ResolveAsync(await t.GetStreamAsync()));
		Assert.Contains(new FileType() { MediaType = type }, tt.ToList());
	}

	[Test]
	[TestCase("https://i.imgur.com/QtCausw.png", "image/jpeg")]
	[TestCase("http://static.zerochan.net/atago.(azur.lane).full.2750747.png", "image/png")]
	public async Task Test2(string s, string type)
	{
		var t  = await s.GetAsync();
		var tt = await (MagicResolver.Instance as IFileTypeResolver).ResolveAsync(await t.GetStreamAsync());
		Assert.Contains(new FileType() { MediaType = type }, tt.ToList());

	}

	[Test]
	[TestCase("https://i.imgur.com/QtCausw.png", "image/jpeg")]
	[TestCase("http://static.zerochan.net/atago.(azur.lane).full.2750747.png", "image/png")]
	public async Task Test3(string s, string type)
	{
		var t  = await s.GetAsync();
		var tt = await (FastResolver.Instance as IFileTypeResolver).ResolveAsync(await t.GetStreamAsync());
		Assert.Contains(new FileType() { MediaType = type }, tt.ToList());
	}
}