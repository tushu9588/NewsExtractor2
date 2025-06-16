using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Configuration;

public static class NewsDatabaseSaver
{
    private static string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

    public static void SaveNews(List<NewsItem> newsItems)
    {
        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            foreach (var item in newsItems)
            {
                string query = "INSERT IGNORE INTO news_table (title, url, publication_date) VALUES (@title, @url, @date)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@title", item.Title);
                    cmd.Parameters.AddWithValue("@url", item.Url);
                    cmd.Parameters.AddWithValue("@date", item.PublicationDate);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}