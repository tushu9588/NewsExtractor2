using System;

public class NewsItem
{
    public string Title { get; set; }
    public string Url { get; set; }
    public DateTime PublicationDate { get; set; }
    public string Type { get; set; } // "Regular" or "Breaking"
    public string NewsImpact { get; set; }     // "Positive", "Negative", "Archive"
}
