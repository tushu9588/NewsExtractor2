using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

public class NewsDatabaseSaver
{
    private string connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

    public void SaveNews(List<NewsItem> newsItems)
    {
        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();

            foreach (var item in newsItems)
            {
                // Check for duplicates
                string checkQuery = "SELECT COUNT(*) FROM news_table WHERE title = @title AND url = @url";
                using (var checkCmd = new MySqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@title", item.Title);
                    checkCmd.Parameters.AddWithValue("@url", item.Url);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0) continue; // Skip duplicate
                }

                // Insert new news item
                string insertQuery = @"INSERT INTO news_table (title, url, publication_date, type)
                                       VALUES (@title, @url, @date, @type)";
                using (var insertCmd = new MySqlCommand(insertQuery, conn))
                {
                    insertCmd.Parameters.AddWithValue("@title", item.Title);
                    insertCmd.Parameters.AddWithValue("@url", item.Url);
                    insertCmd.Parameters.AddWithValue("@date", item.PublicationDate);
                    insertCmd.Parameters.AddWithValue("@type", item.Type);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
