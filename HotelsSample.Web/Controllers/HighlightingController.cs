using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Find;
using EPiServer.Find.Helpers.Text;
using FindSample.Models;
using EPiServer.Sample.Hotels;

namespace FindSample.Controllers
{
    public class HighlightingController : Controller
    {
        IClient client;

        public HighlightingController()
        {
            this.client = HotelHelpers.HotelClient;
        }

        public ActionResult Index(string q)
        {
            if (q == null)
            {
                return View();
            }

            var results = client.Search<Hotel>()
                .For(q).InField(h => h.Description)
                .Select(x => new SearchHit
                {
                    Title = !string.IsNullOrEmpty(x.Name.AsHighlighted()) ? x.Name.AsHighlighted() : x.Name,
                    Url = x.Website ,
                    Location = new List<string> { x.ShortAddress, x.Location.Title, x.Location.Country.Title }.Concatenate(", "),
                    StarRating = (int)x.StarRating,
                    Text = x.Description.AsHighlighted(
                        new HighlightSpec
                        {
                            FragmentSize = 200, 
                            NumberOfFragments = 2,
                            Concatenation = highlights => highlights.Concatenate(" ... ")
                        })
                })
                .GetResult();

            ViewBag.Query = q;
            ViewBag.Id = results.ProcessingInfo.ServerDuration;
            ViewBag.Hits = results.TotalMatching;

            return View(new SearchResult(results, q));
        }
    }
}
