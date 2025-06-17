using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

public class NewsDatabaseSaver
{
    private string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

    public void SaveNews(List<NewsItem> newsList)
    {
        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            foreach (var news in newsList)
            {
                string query = @"INSERT IGNORE INTO News (title, url, PublicationDate, type, NewsImpact)
                             VALUES (@title, @url, @date, @type, @impact)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@title", news.Title);
                    cmd.Parameters.AddWithValue("@url", news.Url);
                    cmd.Parameters.AddWithValue("@date", news.PublicationDate);
                    cmd.Parameters.AddWithValue("@type", news.Type);
                    cmd.Parameters.AddWithValue("@impact", news.NewsImpact ?? "Positive");
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

}
        
    

