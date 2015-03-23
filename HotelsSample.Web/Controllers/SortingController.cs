using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Find;
using EPiServer.Find.Helpers.Text;
using FindSample.Models;
using EPiServer.Sample.Hotels;

namespace FindSample.Controllers
{
    public class SortingController : Controller
    {
        IClient client;

        public SortingController()
        {
            this.client = HotelHelpers.HotelClient;
        }

        public ActionResult Index(string q, string sort)
        {
            if (q == null && sort == null)
            {
                return View();
            }

            ViewBag.SortLinks = new List<Link>
                {
                    new Link
                        {
                            Text = "Relevance",
                            Url = Url.Action("Index", new {q, sort = "relevance"}),
                            CssClass = (sort == null || sort == "relevance") ? "active" : ""
                        },
                    new Link
                        {
                            Text = "Name",
                            Url = Url.Action("Index", new {q, sort = "name"}),
                            CssClass = sort == "name" ? "active" : ""
                        },
                    new Link
                        {
                            Text = "Chain",
                            Url = Url.Action("Index", new {q, sort = "chain"}),
                            CssClass = sort == "chain" ? "active" : ""
                        },
                    new Link
                        {
                            Text = "Country",
                            Url = Url.Action("Index", new {q, sort = "country"}),
                            CssClass = sort == "country" ? "active" : ""
                        },
                    new Link
                        {
                            Text = "Star Rating",
                            Url = Url.Action("Index", new {q, sort = "rating"}),
                            CssClass = sort == "rating" ? "active" : ""
                        }
                };

        ITypeSearch<Hotel> query = client.Search<Hotel>().For(q);

        switch (sort)
        {
            case "name":
                query = query.OrderBy(x => x.Name);
                break;
            case "chain":
                query = query.OrderBy(x => x.Chain);
                break;
            case "country":
                query = query.OrderBy(x => x.Location.Country.Title);
                break;
            case "rating":
                query = query.OrderByDescending(x => x.StarRating);
                break;
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
