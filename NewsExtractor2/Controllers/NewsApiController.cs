using System;
using System.Collections.Generic;
using System.Web.Http;
using MySql.Data.MySqlClient;
using System.Configuration;
using NewsExtractor2;

namespace NewsExtractor2.Controllers
{
    [RoutePrefix("api/news")]
    public class NewsApiController : ApiController
    {
        private readonly string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"]?.ConnectionString
            ?? throw new InvalidOperationException("Connection string 'MySqlConn' not found in Web.config.");

        // ✅ GET: api/news/search?title=...&startDate=...&endDate=...
        [HttpGet]
        [Route("search")]
        public IHttpActionResult SearchNews(string title = "", string startDate = "", string endDate = "", string impact = "", int page = 1, int pageSize = 20)
        {
            var results = new List<NewsItem>();
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                string query = @"
            SELECT Id, title, url, PublicationDate, type, NewsImpact
            FROM News
            WHERE (PublicationDate BETWEEN @start AND @end)
              AND (@title = '' OR title LIKE CONCAT('%', @title, '%'))
              AND (@impact = '' OR NewsImpact = @impact)
            ORDER BY PublicationDate DESC
            LIMIT @offset, @pageSize";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    DateTime start = string.IsNullOrEmpty(startDate) ? DateTime.Today.AddDays(-1) : DateTime.Parse(startDate);
                    DateTime end = string.IsNullOrEmpty(endDate) ? DateTime.Today : DateTime.Parse(endDate);

                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@impact", impact);
                    cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@pageSize", pageSize);

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
                                Type = reader["type"].ToString(),
                                NewsImpact = reader["NewsImpact"]?.ToString()
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
                    n.NewsImpact = null;
                });

                var regular = await NewsFetcher.FetchNewsFromSitemapAsync();
                regular.ForEach(n =>
                {
                    n.Type = "Regular";
                    n.NewsImpact = null;
                });

                var toi = await TimesOfIndiaNewsFetcher.FetchNewsAsync(); // ✅ New
                toi.ForEach(n =>
                {
                    n.Type = "TimesOfIndia";
                    n.NewsImpact = null;
                });

                var saver = new NewsDatabaseSaver();
                saver.SaveNews(breaking);
                saver.SaveNews(regular);
                saver.SaveNews(toi); // ✅ Save TOI too

                return Ok("✅ All sources fetched & saved.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // ✏️ PUT: api/news/updateimpact?id=123&impact=Positive
        [HttpPut]
        [Route("updateimpact")]
        public IHttpActionResult UpdateNewsImpact(int id, string impact)
        {
            if (string.IsNullOrEmpty(impact) || !(impact == "Positive" || impact == "Negative" || impact == "Archive"))
                return BadRequest("Impact must be Positive, Negative, or Archive.");

            using (var connection = new MySqlConnection(connStr))
            {
                connection.Open();
                string query = "UPDATE News SET NewsImpact = @impact WHERE Id = @id LIMIT 1";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@impact", impact);
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        return Ok("✅ NewsImpact updated successfully.");
                    else
                        return NotFound();
                }
            }
        }
    }
}
