using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;

// ReSharper disable once IdentifierTypo
namespace EmojipediaApi
{
    public class EmojiSearcher : IDisposable
    {
        private const string BaseUrl = "https://emojipedia.org";
        internal static HttpClient Client { get; private set; }

        public void Dispose()
        {
            Client.Dispose();
        }

        public EmojiSearcher()
        {
            Client = new HttpClient { BaseAddress = new Uri(BaseUrl), Timeout = TimeSpan.FromSeconds(5) };
        }
        ~EmojiSearcher()
        {
            Dispose();
        }

        public SearchResult[] Search(string searchString)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(("/search/?q=" + searchString).Replace(" ", "%20"), UriKind.Relative)
            };
            var resp = Client.SendAsync(request).GetAwaiter().GetResult();
            var respString = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(respString);
            var searchResults = htmlDoc.DocumentNode.SelectNodes("//ol[@class='search-results']/li");
            var resultCheck = htmlDoc.DocumentNode.SelectNodes(searchResults[0].XPath + "/h2");
            if (resultCheck == null || resultCheck.Count <= 0)
                return new SearchResult[] { }; // No results found

            return (from result in searchResults let fullTitle = htmlDoc.DocumentNode.SelectSingleNode(result.XPath + "/h2/a").InnerText let splTitle = fullTitle.Split(new[] { ' ' }, 2) let emoji = splTitle[0] let name = splTitle[1] let desc = htmlDoc.DocumentNode.SelectSingleNode(result.XPath + "/p").InnerText let url = "https://emojipedia.org/" + htmlDoc.DocumentNode.SelectSingleNode(result.XPath + "/h2/a").Attributes["href"].Value select new SearchResult(emoji, name, desc, url)).ToArray();
        }

        public EmojiInfo Random()
        {
            var req = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("/random", UriKind.Relative)
            };
            HttpResponseMessage resp = null;
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    resp = Client.SendAsync(req).GetAwaiter().GetResult();
                    break;
                }
                catch
                {
                    // continue the loop
                }
            }

            if (resp == null)
            {
                throw new TimeoutException("The Emojipedia server timed out.");
            }
            var url = resp.RequestMessage.RequestUri;
            return new EmojiInfo(url);
        }
        public class SearchResult
        {
            public string Emoji { get; }
            public string Name { get; }
            public string ShortDescription { get; }
            public string Url { get; }

            public SearchResult(string emoji, string name, string shortDesc, string url)
            {
                Emoji = emoji;
                Name = name;
                ShortDescription = shortDesc;
                Url = url;
            }

            public EmojiInfo GetEmojiInfo() =>
                new EmojiInfo(this);
        }
        public class EmojiInfo
        {
            public string Emoji { get; private set; }
            public string UnicodeName { get; private set; }
            public string AppleName { get; private set; }
            public string Description { get; private set; } = string.Empty;
            public string Url { get; private set; }
            public string[] AlsoKnownAs { get; private set; }

            public EmojiInfo(SearchResult result) =>
            Setup(result);

            public EmojiInfo(Uri emojiUri)
            {
                var req = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = emojiUri };
                var resp = Client.SendAsync(req).GetAwaiter().GetResult();
                var respString = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var doc = new HtmlDocument();
                doc.LoadHtml(respString);
                var content = doc.DocumentNode.SelectSingleNode("//div[@class='content']/article");
                var fullTitle = doc.DocumentNode.SelectSingleNode(content.XPath + "/h1").InnerText;
                var splTitle = fullTitle.Split(new[] { ' ' }, 2);


                var emoji = splTitle[0];
                var name = splTitle[1];


                var result = new SearchResult(emoji, name, null, emojiUri.AbsoluteUri);
                Setup(result);
            }

            private void Setup(SearchResult result)
            {
                Emoji = result.Emoji;
                Url = result.Url;

                var req = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(Url, UriKind.Absolute) };

                var resp = Client.SendAsync(req).GetAwaiter().GetResult();
                var respString = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(respString);

                var aliases = htmlDoc.DocumentNode.SelectNodes("//section[@class='aliases']/ul/li");
                if (aliases != null && aliases.Count > 0)
                {
                    AlsoKnownAs = aliases.Select(alias => alias.InnerText.Split(new[] { ' ' }, 2)[1]).ToArray();
                }
                else
                    AlsoKnownAs = new string[] { };

                var unicode = htmlDoc.DocumentNode.SelectSingleNode("//section[@class='unicodename']/p");
                UnicodeName = unicode != null ? unicode.InnerText.Split(new[] { ' ' }, 2)[1] : result.Name;

                var apple = htmlDoc.DocumentNode.SelectSingleNode("//section[@class='applenames']/p");
                AppleName = apple != null ? apple.InnerText.Split(new[] { ' ' }, 2)[1] : UnicodeName;

                var desc = htmlDoc.DocumentNode.SelectNodes("//section[@class='description']/p");
                if (desc == null || desc.Count <= 0) return;
                foreach (var section in desc)
                {
                    Description += section.InnerText.Replace("&nbsp;", " ") + " ";
                }

                Description = WebUtility.HtmlDecode(Description.Remove(Description.Length - 2).Replace("\n", ""));
            }
        }
    }
}
