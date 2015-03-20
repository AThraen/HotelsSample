using EPiServer.Sample.Hotels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Find;
using EPiServer.Find.Api.Facets;
using EPiServer.Find.Api.Querying.Filters;
using EPiServer.Find.Helpers.Text;

/*
 * The following is some sample code that allows you to easily explore EPiServer Find and try out Queries against an index of 
 * 340.000 Hotels worldwide.
 * You can sign up for your own free developer index on find.episerver.com.
 * */
namespace HotelsSample.ConsoleApp
{
    class Program
    {
        static GeoLocation WHITEHOUSE = new GeoLocation(38.897674, -77.036723);
        static GeoLocation MIAMI = new GeoLocation(25.766383, -80.191022);
        static GeoLocation CHICAGO = new GeoLocation(41.876294, -87.618971);
        static GeoLocation LONDON = new GeoLocation(51.505916, -0.075057);



        static void Main(string[] args)
        {
            var cli = HotelHelpers.HotelClient;
            var q = cli.Search<Hotel>();
            //Extend the query with your code here, or use one of the example methods here below:

            //q=TextSearchExample(q);
            //q=AdvancedTextSearchExample(q);
            //q=BasicFilteringExample(q);
            //q=AdvancedFilteringExample(q);
            //q=BasicSortingExample(q);
            //q=GeoSortingExample(q);
            //q = FacetsExample(q);
            //q = PaginationExample(q);

            var res = q.GetResult();
            OutputResults(res);

            //Uncomment the next line to see Similar results to the first of your results.
            //Console.WriteLine("Similar: " + SimilarHotels(cli, res.First()).First().Name);
            Console.ReadLine();
        }

        #region Examples

        /// <summary>
        /// Basic text search in all fields for a given query
        /// </summary>
        /// <param name="cli"></param>
        static ITypeSearch<Hotel> TextSearchExample(ITypeSearch<Hotel> q)
        {
            Console.Write("What should we search for? ");
            string query=Console.ReadLine();
            return q.For(query);
        }

        /// <summary>
        /// Advanced text search searching for query in Name, Description and features. Boosting results where the title contains the query. Boosting highly rated hotels
        /// </summary>
        /// <param name="cli"></param>
        static ITypeSearch<Hotel> AdvancedTextSearchExample(ITypeSearch<Hotel> q)
        {
            Console.Write("What should we search for? ");
            string query = Console.ReadLine();
            return q.For(query)
                .InField(h => h.Name)
                .AndInField(h => h.Description)
                .AndInField(h => h.Features)
                .BoostMatching(h => h.Title.AnyWordBeginsWith(query), 2.5)
                .BoostMatching(h => h.StarRating.InRange(3, 5), 2);
        }

        /// <summary>
        /// Shows hotels with mor than 2 stars within 10 miles of the white house that offer room service, and which are not marriott.
        /// </summary>
        /// <param name="cli"></param>
        static ITypeSearch<Hotel> BasicFilteringExample(ITypeSearch<Hotel> q)
        {
            return q
                .Filter(h => h.StarRating.GreaterThan(2))
                .Filter(h => h.GeoCoordinates.WithinDistanceFrom(WHITEHOUSE, 10.Miles()))
                .Filter(h => h.Features.Match("Room Service"))
                .Filter(h => !h.Chain.Match("Marriott"));
        }

        /// <summary>
        /// Shows hotels within the geographic triangle between Miami, Chicago and London - and where their rating star rating is either 4 or 5, or their review rate them 9 or 10 with more than 50 reviews.
        /// </summary>
        /// <param name="cli"></param>
        static ITypeSearch<Hotel> AdvancedFilteringExample(ITypeSearch<Hotel> q)
        {
            var RatingFilter = q.Client.BuildFilter<Hotel>();
            RatingFilter = RatingFilter.Or(h => h.Ratings.Overall.GreaterThan(8) & h.ReviewCount.GreaterThan(50));
            RatingFilter = RatingFilter.Or(h => h.StarRating.InRange(4, 5));
            return q
                .Filter(RatingFilter)
                .Filter(h => h.GeoCoordinates.Within(new GeoLocation[] { MIAMI, CHICAGO, LONDON }));
        }

        /// <summary>
        /// Show all hotels, best first
        /// </summary>
        /// <param name="cli"></param>
        static ITypeSearch<Hotel> BasicSortingExample(ITypeSearch<Hotel> q)
        {
            return q
                .OrderByDescending(h => h.StarRating)
                .ThenByDescending(h => h.Ratings.Overall)
                .ThenByDescending(h => h.ReviewCount);
        }

        /// <summary>
        /// Order by distance from Tower Bridge in London
        /// </summary>
        /// <param name="cli"></param>
        static ITypeSearch<Hotel> GeoSortingExample(ITypeSearch<Hotel> q)
        {
            return q
                .OrderByDescending(h => h.GeoCoordinates).DistanceFrom(LONDON);
        }

        /// <summary>
        /// Adds a long range of facets for specific parameters that gets aggregated across all results
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        static ITypeSearch<Hotel> FacetsExample(ITypeSearch<Hotel> q)
        {
            return q
                .TermsFacetFor(h => h.Chain) //Hotel Chain Facet
                .TermsFacetFor(h => h.PropertyType, tfr => tfr.Size = 100) //Hotel Type
                .TermsFacetFor(h => h.Features) //List of features offered
                .TermsFacetFor(h => h.Location.Country.Title) //Country - in a complex child object
                .TermsFacetForWordsIn(h => h.PositiveReviews) //Words commonly used in positive reviews
                .RangeFacetFor(h => h.PriceUSD, new NumericRange(20, 50), new NumericRange(51, 100), new NumericRange(101, 500), new NumericRange(501, 10000))
                .HistogramFacetFor(h => h.StarRating, 1)
                .FilterFacet("HasImages",h => h.ImageCount.GreaterThan(0))
                .StatisticalFacetFor(h => h.ReviewCount)
                .GeoDistanceFacetFor(h => h.GeoCoordinates, CHICAGO, new NumericRange(0, 100), new NumericRange(101, 1000), new NumericRange(1001, 5000));
        }

        /// <summary>
        /// Skips the first result, then fetches the next 20.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        static ITypeSearch<Hotel> PaginationExample(ITypeSearch<Hotel> q)
        {
            return q
                .Skip(1).Take(20);
        }

        /// <summary>
        /// Find hotels with a similar description to the current hotel.
        /// </summary>
        /// <param name="cli"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        static List<Hotel> SimilarHotels(IClient cli, Hotel a)
        {
            return
                cli.Search<Hotel>()
                .MoreLike(a.Description) //Hotels with similar description
                .MinimumDocumentFrequency(1)
                .BoostMatching(h => h.PropertyType.Match(a.PropertyType),1.1) //Boost if same property type
                .Filter(h => !h.Id.Match(a.Id)) //Not the current hotel
                .Take(10)
                .GetResult().ToList();
        }

        #endregion

        /// <summary>
        /// Helper method that outputs both facets and results
        /// </summary>
        /// <param name="res"></param>
        static void OutputResults(SearchResults<Hotel> res)
        {
            Console.WriteLine("RESULTS Showing {0} out of {1}. Search took {2} ms", res.Hits.Count(), res.TotalMatching, res.ProcessingInfo.ServerDuration);
            Console.WriteLine("--------------------------------------------------------------");
            if (res.Facets != null) { 
            foreach (var f in res.Facets)
            {
                Console.WriteLine("Facet: {0}", f.Name);
                if (f is TermsFacet)
                {
                    foreach (var t in (f as TermsFacet).Terms)
                    {
                        Console.WriteLine("\t{0} ({1})", t.Term, t.Count);
                    }
                }
                else if (f is FilterFacet)
                {
                    Console.WriteLine("\tMatching: {0}", (f as FilterFacet).Count);
                }
                else if (f is StatisticalFacet)
                {
                    var sf=f as StatisticalFacet;
                    Console.WriteLine("\tMean: {0}\n\tMax: {1}\n\tMin: {2}\n\tTotal: {3}\n\tVariance: {4}\n",sf.Mean,sf.Max,sf.Min,sf.Total,sf.Variance);
                }
                else if (f is NumericRangeFacet)
                {
                    foreach (var r in (f as NumericRangeFacet).Ranges)
                    {
                        Console.WriteLine("\t{0}-{1} ({2})", r.From, r.To, r.Count);
                    }
                }
                else if (f is HistogramFacet)
                {
                    foreach (var r in (f as HistogramFacet).Entries)
                    {
                        Console.WriteLine("\t{0} ({1})", r.Key, r.Count);
                    }
                }
                else if (f is GeoDistanceFacet)
                {
                    foreach (var r in (f as GeoDistanceFacet).Ranges)
                    {
                        Console.WriteLine("\t{0}-{1} ({2})", r.From.Value, r.To.Value, r.TotalCount);
                    }
                }
                Console.WriteLine();
            }
            }
            Console.WriteLine();
            Console.WriteLine("Hits: ");
            foreach (var h in res.Hits)
            {
                Console.WriteLine("\t{0}", h.Document.Title.ToUpper());
                Console.WriteLine("\t{0}", h.Document.LocationsString);
                Console.WriteLine("\tRating: {0}", h.Document.StarRating);
                Console.WriteLine("\tType: {0}", h.Document.PropertyType);
                Console.WriteLine("\t{0}", string.Join(",", h.Document.Features.Take(5).ToArray()));
                Console.WriteLine("\n");
            }

        }
    }
}
