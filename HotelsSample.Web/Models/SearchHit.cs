using System;
namespace FindSample.Models
{
    public class SearchHit
    {
        public string Title { get; set; }

        public Uri Url { get; set; }

        public string Location { get; set; }

        public string Text { get; set; }

        public int StarRating { get; set; }
    }
}