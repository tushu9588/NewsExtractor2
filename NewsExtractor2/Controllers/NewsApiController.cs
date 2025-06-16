using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace NewsExtractor2.Controllers
{
    [RoutePrefix("api/news")]
    public class NewsApiController : ApiController
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"]?.ConnectionString
    ?? throw new InvalidOperationException("Connection string 'MySqlConn' not found in Web.config.");


        // 🔍 Search API
        [HttpGet]
        [Route("search")]
        public IHttpActionResult SearchNews([FromUri] string title = "", [FromUri] string startDate = "", [FromUri] string endDate = "")
        {
            List<NewsItem> result = new List<NewsItem>();
            DateTime start = string.IsNullOrEmpty(startDate) ? DateTime.Today : DateTime.Parse(startDate);
            DateTime end = string.IsNullOrEmpty(endDate) ? DateTime.Today : DateTime.Parse(endDate);

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = @"SELECT title, url, PublicationDate 
                                 FROM News 
                                 WHERE PublicationDate BETWEEN @start AND @end 
                                 AND title LIKE @title";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end.AddDays(1)); // include full day
                    cmd.Parameters.AddWithValue("@title", $"%{title}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new NewsItem
                            {
                                Title = reader.GetString("title"),
                                Url = reader.GetString("url"),
                                PublicationDate = reader.GetDateTime("PublicationDate")
                            });
                        }
                    }
                }
            }

            return Ok(result);
        }

        // 🔁 Fetch News on Button Click
        [HttpPost]
        [Route("fetchall")]
        public async System.Threading.Tasks.Task<IHttpActionResult> FetchAllNews()
        {
            try
            {
                // Fetch breaking news
                var breaking = await BreakingNewsFetcher.FetchBreakingNewsAsync();
                breaking.ForEach(n => n.Type = "Breaking");

                // Fetch regular news
                var regular = await NewsFetcher.FetchNewsFromSitemapAsync();
                regular.ForEach(n => n.Type = "Regular");

                // Save all using single instance
                var saver = new NewsDatabaseSaver();
                saver.SaveNews(breaking);
                saver.SaveNews(regular);

                return Ok("✅ News fetched & saved successfully.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}


