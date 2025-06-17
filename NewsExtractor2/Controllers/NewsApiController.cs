using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MySql.Data.MySqlClient;
using System.Configuration;
using NewsExtractor2; // Make sure this matches your namespace

namespace NewsExtractor2.Controllers
{
    [RoutePrefix("api/news")]
    public class NewsApiController : ApiController
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"]?.ConnectionString
            ?? throw new InvalidOperationException("Connection string 'MySqlConn' not found in Web.config.");

        // 🔍 GET: api/news/all
        // ✅ GET: api/news/search?title=...&startDate=...&endDate=...
        [HttpGet]
        [Route("search")]
        public IHttpActionResult SearchNews(string title = "", string startDate = "", string endDate = "")
        {
            List<NewsItem> results = new List<NewsItem>();

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = @"SELECT Id,title, url, PublicationDate, type, NewsImpact FROM News
                         WHERE (@title = '' OR title LIKE @title)
                         AND (PublicationDate BETWEEN @start AND @end)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@title", "%" + title + "%");

                    DateTime start = string.IsNullOrEmpty(startDate) ? DateTime.Today : DateTime.Parse(startDate);
                    DateTime end = string.IsNullOrEmpty(endDate) ? DateTime.Today : DateTime.Parse(endDate);

                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end);

                    using (var reader = cmd.ExecuteReader())
                    {
                        
                        
                            while (reader.Read())
                            {
                                results.Add(new NewsItem
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Title = reader["Title"].ToString(),
                                    Url = reader["Url"].ToString(),
                                    PublicationDate = Convert.ToDateTime(reader["PublicationDate"]),
                                    Type = reader["Type"].ToString(),
                                    NewsImpact = reader["NewsImpact"] != DBNull.Value
                                                    ? reader["NewsImpact"].ToString()
                                                    : "Positive"  // or use null if you prefer
                                });
                            }




                        }
                    }
            }

            return Ok(results);
        }

        


        // 🔁 POST: api/news/fetchall
        [HttpPost]
        [Route("fetchall")]
        public async System.Threading.Tasks.Task<IHttpActionResult> FetchAllNews()
        {
            try
            {
                var breaking = await BreakingNewsFetcher.FetchBreakingNewsAsync();
                breaking.ForEach(n =>
                {
                    n.Type = "Breaking";
                    n.NewsImpact = "Positive";
                });

                var regular = await NewsFetcher.FetchNewsFromSitemapAsync();
                regular.ForEach(n =>
                {
                    n.Type = "Regular";
                    n.NewsImpact = "Positive";
                });

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

        // ✏️ PUT: api/news/updateimpact
        [HttpPut]
        [Route("updateimpact")]
        public IHttpActionResult UpdateNewsImpact([FromBody] NewsItem news)
        {
            if (string.IsNullOrEmpty(news.Title) || string.IsNullOrEmpty(news.NewsImpact))
                return BadRequest("Title and NewsImpact are required.");

            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = @"UPDATE News SET NewsImpact = @impact WHERE title = @title";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@impact", news.NewsImpact);
                    cmd.Parameters.AddWithValue("@title", news.Title);
                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                        return NotFound();
                }
            }

            return Ok("✅ NewsImpact updated successfully.");
        }

    }
}
