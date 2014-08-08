/*
 * This is a route *provider*. Such providers are very common in Orchard: to hook into some services, to extend some functionality you have to
 * implement an interface. Because of the extensible nature of Orchard and because one should be able to switch modules on and off route 
 * registration happens a bit different than in standard ASP.NET MVC applications: we use IRouteProvider.
 */

using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace Custom.RatingTable
{
    [OrchardFeature("Custom.RatingTable")]
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[]
            {
                new RouteDescriptor
                {
                    Route = new Route(
                        "Admin/RatingTableDashboard",
                        new RouteValueDictionary
                        {
                            {"area", "Custom.RatingTable"},
                            {"controller", "RatingTableAdmin"},
                            {"action", "GetRatingTableEditor"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary
                        {
                            {"area", "Custom.RatingTable"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}