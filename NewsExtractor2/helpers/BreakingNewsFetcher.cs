using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public static class BreakingNewsFetcher
{
    public static async Task<List<NewsItem>> FetchBreakingNewsAsync()
    {
        List<NewsItem> list = new List<NewsItem>();
        HttpClient client = new HttpClient();
        string url = "https://economictimes.indiatimes.com/etstatic/breakingnews/etjson_bnews.html";
        string json = await client.GetStringAsync(url);

        JArray items = JArray.Parse(json.Split('=')[1].TrimEnd(';'));
        foreach (var item in items)
        {
            DateTime pubDate = DateTime.Now;
            if (pubDate.Date == DateTime.Today || pubDate.Date == DateTime.Today.AddDays(-1))
            {
                list.Add(new NewsItem
                {
                    Title = item["title"].ToString(),
                    Url = item["link"].ToString(),
                    PublicationDate = pubDate
                });
            }
        }

        return list;
    }
}