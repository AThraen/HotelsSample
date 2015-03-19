using EPiServer.Sample.Hotels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Find;
using EPiServer.Find.Api.Facets;
using EPiServer.Find.Api.Querying.Filters;


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
            
            
            var res = cli.Search<Hotel>()          
                .GetResult();

            OutputResults(res);
            Console.ReadLine();
        }


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
                }//DateTime Range, Histogram facets
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
