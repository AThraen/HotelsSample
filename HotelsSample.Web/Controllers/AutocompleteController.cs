using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Find;
using EPiServer.Find.Helpers.Text;
using FindSample.Models;
using EPiServer.Sample.Hotels;

namespace FindSample.Controllers
{
    public class AutocompleteController : Controller
    {
        IClient client;

        public AutocompleteController()
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

            return View(new SearchResult(results, q));
        }

        public JsonResult Prefix(string term)
        {
            var results = client.Search<Hotel>()
                .Filter(x => x.Name.PrefixCaseInsensitive(term))
                .Select(x => x.Name)
                .StaticallyCacheFor(TimeSpan.FromHours(1))
                .GetResult();

            return Json(results.Select(x => x), JsonRequestBehavior.AllowGet);
        }
    }
}
