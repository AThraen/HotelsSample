using System.Collections.Generic;

namespace FindSample.Models
{
    public class FacetResult
    {
        public FacetResult(string name, IEnumerable<FacetLink> links)
        {
            Name = name;
            Links = links;
        }

        public string Name { get; private set; }

        public IEnumerable<FacetLink> Links { get; private set; }
    }
}