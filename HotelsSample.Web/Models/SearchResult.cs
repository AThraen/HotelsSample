using System.Collections.Generic;
using EPiServer.Find;

namespace FindSample.Models
{
    public class SearchResult
    {
        public SearchResult(SearchResults<SearchHit> results, string searchTerm)
        {
            Results = results;
            SearchTerm = searchTerm;
            CurrentPage = 1;
        }

        public SearchResults<SearchHit> Results { get; private set; }

        public string SearchTerm { get; private set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public IEnumerable<FacetResult> Facets { get; set; }
    }
}