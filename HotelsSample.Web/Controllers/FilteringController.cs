using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Find;
using EPiServer.Find.Helpers.Text;
using FindSample.Models;
using EPiServer.Sample.Hotels;

namespace FindSample.Controllers
{
    public class FilteringController : Controller
    {
        IClient client;

        public FilteringController()
        {
            this.client = HotelHelpers.HotelClient;
        }

        public ActionResult Index(string q, IEnumerable<string> chains, IEnumerable<string> countries)
        {
            //Retrieving values for fields that should be possible to filter on.
            //Often we would get these from a database, CMS or other source.
            var filterValues = client.Search<Hotel>()
                .Take(0)
                .TermsFacetFor(x => x.Chain, tfr => tfr.Size=50)
                .TermsFacetFor(x => x.Location.Country.Title, tfr => tfr.Size=50)
                .StaticallyCacheFor(TimeSpan.FromHours(1))
                .GetResult();

            ViewBag.Chains = filterValues.TermsFacetFor(x => x.Chain)
                .Select(x => new SelectListItem { Text = x.Term, Value = x.Term});

            ViewBag.Countries = filterValues.TermsFacetFor(x => x.Location.Country.Title)
                .Select(x => new SelectListItem { Text = x.Term, Value = x.Term});

            //As UrlHelper.Action doesn't add query string parameter with empty values
            //we need to check all parameters in case the user has searched without
            //entering a search term (he/she has just hit the search button)
            if (q == null && chains == null && countries == null)
            {
                return View();
            }

            ITypeSearch<Hotel> query = client.Search<Hotel>()
                .For(q);

            if (chains != null)
            {
                var chainFilter = client.BuildFilter<Hotel>();
                foreach (var chain in chains)
                {
                    chainFilter = chainFilter.Or(x => x.Chain.Match(chain));
                }
                query = query.Filter(chainFilter);
            }

            if (countries != null)
            {
                var countryFilter = client.BuildFilter<Hotel>();
                foreach (var country in countries)
                {
                    countryFilter = countryFilter.Or(x => x.Location.Country.Title.Match(country));
                }
                query = query.Filter(countryFilter);
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

            return View(new SearchResult(results, q));
        }
    }
}
