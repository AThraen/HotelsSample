using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Find;
using EPiServer.Find.Api.Facets;
using EPiServer.Find.Helpers.Text;
using FindSample.Models;
using EPiServer.Sample.Hotels;

namespace FindSample.Controllers
{
    public class FacetsController : Controller
    {
        IClient client;

        public FacetsController()
        {
            this.client = HotelHelpers.HotelClient;
        }

        public ActionResult Index(string q, string chain, string country, int? rating)
        {
            //As UrlHelper.Action doesn't add query string parameter with empty values
            //we need to check all parameters in case the user has searched without
            //entering a search term (he/she has just hit the search button)
            if (q == null && chain == null && country == null && !rating.HasValue)
            {
                return View();
            }

            var query = client.Search<Hotel>()
                .For(q)
                .TermsFacetFor(x => x.Chain)
                .TermsFacetFor(x => x.Location.Country.Title)
                .HistogramFacetFor(x => x.StarRating, 1);

            //Apply filters added by facet links
            if (!string.IsNullOrWhiteSpace(chain))
            {
                query = query.Filter(x => x.Chain.MatchCaseInsensitive(chain));
            }

            if (!string.IsNullOrWhiteSpace(country))
            {
                query = query.Filter(x => x.Location.Country.Title.MatchCaseInsensitive(country));
            }

            if (rating.HasValue)
            {
                query = query.Filter(x => x.StarRating.Match(rating.Value));
            }

            var results = query.Select(x => new SearchHit
            {
                Title = x.Name,
                Url = x.Website,
                Location = new List<string> { x.ShortAddress, x.Location.Title, x.Location.Country.Title }.Concatenate(", "),
                StarRating = (int)x.StarRating
            })
                .GetResult();

            ViewBag.Query = q;
            ViewBag.Id = results.ProcessingInfo.ServerDuration;
            ViewBag.Hits = results.TotalMatching;
            
            //Groups of links for each facet
            var facets = new List<FacetResult>();
            
            var chainFacet = (TermsFacet) results.Facets["Chain"];
            var chainLinks = new FacetResult("Chains",
                chainFacet.Terms.Select(x => new FacetLink
                {
                    Text = x.Term,
                    Count = x.Count,
                    Url = Url.Action("Index", new { q, chain = x.Term, country, rating })
                }));
            facets.Add(chainLinks);

            var countryFacet = (TermsFacet)results.Facets["Location.Country.Title"];
            var countryLinks = new FacetResult("Countries",
                countryFacet.Terms.Select(x => new FacetLink
                {
                    Text = x.Term,
                    Count = x.Count,
                    Url = Url.Action("Index", new { q, chain, country = x.Term, rating })
                }));
            facets.Add(countryLinks);

            var ratingFacet = results.HistogramFacetFor(x => (int) x.StarRating);
            var ratingFacetLinks = new FacetResult("Star Rating",
                ratingFacet.Entries.Select(x => new FacetLink
                {
                    Text = "",
                    CssClass = "stars-" + x.Key,
                    Count = x.Count ,
                    Url = Url.Action("Index", new { q, chain, country, rating = x.Key })
                }));
            facets.Add(ratingFacetLinks);

            //Links for removing filters
            ViewBag.Filters = new List<FacetLink>();
            
            if (!string.IsNullOrEmpty(chain))
            {
                ViewBag.Filters.Add(new FacetLink
                {
                    Text = chain,
                    Url = Url.Action("Index", new { q, country, rating })
                });
            }

            if (!string.IsNullOrEmpty(country))
            {
                ViewBag.Filters.Add(new FacetLink
                {
                    Text = country,
                    Url = Url.Action("Index", new { q, chain, rating })
                });
            }

            if (rating.HasValue)
            {
                ViewBag.Filters.Add(new FacetLink
                {
                    Text = "",
                    CssClass = "stars-" + rating,
                    Url = Url.Action("Index", new { q, country, chain })
                });
            }

            return View(new SearchResult(results, q) { Facets = facets });
        }
    }
}
