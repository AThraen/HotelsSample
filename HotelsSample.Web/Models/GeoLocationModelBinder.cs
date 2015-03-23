using System.Globalization;
using System.Web.Mvc;
using EPiServer.Find;

namespace FindSample.Models
{
    public class GeoLocationModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var latitude = double.Parse(bindingContext.ValueProvider.GetValue("Coordinates.Latitude").AttemptedValue.Replace(",", "."), CultureInfo.GetCultureInfo("en-us"));
            var longitude = double.Parse(bindingContext.ValueProvider.GetValue("Coordinates.Longitude").AttemptedValue.Replace(",", "."), CultureInfo.GetCultureInfo("en-us"));
            return new GeoLocation(latitude, longitude);
        }
    }
}