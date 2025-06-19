using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

public static class TimesOfIndiaNewsFetcher
{
    private static readonly string[] urls = {
        "https://timesofindia.indiatimes.com/sitemap/today",
        "https://timesofindia.indiatimes.com/sitemap/yesterday"
    };

    public static async Task<List<NewsItem>> FetchNewsAsync()
    {
        List<NewsItem> newsList = new List<NewsItem>();
        HttpClient client = new HttpClient();

        foreach (var url in urls)
        {
            try
            {
                string response = await client.GetStringAsync(url);
                XNamespace ns = "http://www.google.com/schemas/sitemap-news/0.9";
                XDocument doc = XDocument.Parse(response);

                var articles = doc.Descendants(ns + "news");
                foreach (var news in articles)
                {
                    DateTime pubDate = DateTime.Parse(news.Element(ns + "publication_date").Value);
                    if (pubDate.Date == DateTime.Today || pubDate.Date == DateTime.Today.AddDays(-1))
                    {
                        newsList.Add(new NewsItem
                        {
                            Title = news.Element(ns + "title")?.Value,
                            Url = news.Parent?.Element("loc")?.Value,
                            PublicationDate = pubDate,
                            Type = "TimesOfIndia",
                            NewsImpact = null
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or ignore bad sitemap
            }
        }

        return newsList;
    }
}
