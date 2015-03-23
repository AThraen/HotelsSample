using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Find;
using EPiServer.Find.Helpers.Text;
using FindSample.Models;
using EPiServer.Sample.Hotels;

namespace FindSample.Controllers
{
    public class MapController : Controller
    {
        IClient client;

        public MapController()
        {
            this.client = this.client = HotelHelpers.HotelClient;
        }

        public ActionResult Index(string q, string vertexes)
        {
            return View();
        }

        public ActionResult Filter(string vertexes)
        {
            var points = ParseVertexes(vertexes);
            var results = client.Search<Hotel>()
                .Filter(x => x.GeoCoordinates.Within(points))
                .Select(x => new SearchHit
                {
                    Title = x.Name,
                    Url = x.Website,
                    Location = new List<string> { x.ShortAddress, x.Location.Title, x.Location.Country.Title }.Concatenate(", "),
                    StarRating = (int)x.StarRating
                })
                .GetResult();

            return PartialView("SearchHits", new SearchResult(results, ""));
        }

        IEnumerable<GeoLocation> ParseVertexes(string vertexes)
        {
            var split = vertexes.Split(';');
            foreach (var vertexString in split)
            {
                var latitudeString = vertexString.Split(',')[0];
                var latitude = double.Parse(latitudeString, CultureInfo.CreateSpecificCulture("en-us"));
                var longitudeString = vertexString.Split(',')[1];
                var longitude = double.Parse(longitudeString, CultureInfo.CreateSpecificCulture("en-us"));
                yield return new GeoLocation(latitude, longitude);
            }
        }
    }
}
