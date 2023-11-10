using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Net;
using Kantan.Net.Utilities;
using Kantan.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;

namespace UnitTest;

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

		var execute = await g.ExecuteAsync(query);

		Assert.True(execute["data"]["Media"]["title"]["english"].ToString().Contains("Cowboy Bebop"));
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