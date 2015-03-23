using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Find;
using EPiServer.Find.Helpers.Text;
using FindSample.Models;
using EPiServer.Sample.Hotels;

namespace FindSample.Controllers
{
    public class PagingController : Controller
    {
        IClient client;

        public PagingController()
        {
            this.client = this.client = HotelHelpers.HotelClient;
        }

        public ActionResult Index(string q, int? p)
        {
            if (q == null && !p.HasValue)
            {
                return View();
            }

            int pageSize = 10;
            var page = p ?? 1;

            var results = client.Search<Hotel>()
                .For(q)
                .Take(pageSize)
                .Skip((page-1) * 10)
                .Select(x => new SearchHit
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

            var totalPages = results.TotalMatching/pageSize;
            if (results.TotalMatching % pageSize > 0)
            {
                totalPages++;
            }

            return View(new SearchResult(results, q)
                            {
                                CurrentPage = page, 
                                TotalPages = totalPages
                            });
        }
    }
}
