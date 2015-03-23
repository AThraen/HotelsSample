using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Find;
using EPiServer.Find.Helpers.Text;
using FindSample.Models;
using EPiServer.Sample.Hotels;

namespace FindSample.Controllers
{
    public class BasicSearchController : Controller
    {
        IClient client;

        public BasicSearchController()
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
                .For(q)
                .Select(x => new SearchHit
                            {
                                Title = x.Name,
                                Url = x.Website,
                                Location = new List<string> { x.ShortAddress, x.Location.Title, x.Location.Country.Title }.Concatenate(", "),
                                StarRating = (int) x.StarRating
                            })
                .GetResult();

            ViewBag.Query = q;
            ViewBag.Id = results.ProcessingInfo.ServerDuration;
            ViewBag.Hits = results.TotalMatching;

            return View(new SearchResult(results, q));
        }
    }
}
