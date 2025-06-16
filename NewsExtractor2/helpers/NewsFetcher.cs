using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

public static class NewsFetcher
{
    public static async Task<List<NewsItem>> FetchNewsFromSitemapAsync()
    {
        List<NewsItem> newsList = new List<NewsItem>();
        string url = "https://m.economictimes.com/sitemap/today";
        HttpClient client = new HttpClient();
        var response = await client.GetStringAsync(url);
        XNamespace ns = "http://www.google.com/schemas/sitemap-news/0.9";
        XDocument doc = XDocument.Parse(response);

        var articles = doc.Descendants(ns + "news").Where(n =>
        {
            var pubDate = DateTime.Parse(n.Element(ns + "publication_date").Value);
            return pubDate.Date == DateTime.Today || pubDate.Date == DateTime.Today.AddDays(-1);
        });

        foreach (var news in articles)
        {
            newsList.Add(new NewsItem
            {
                Title = news.Element(ns + "title").Value,
                Url = news.Parent.Element("loc").Value,
                PublicationDate = DateTime.Parse(news.Element(ns + "publication_date").Value)
            });
        }

        return newsList;
    }
}