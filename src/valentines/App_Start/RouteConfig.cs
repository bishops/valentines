using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RiaLibrary.Web;

namespace valentines
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //routes.IgnoreRoute("elmah.axd");
            routes.IgnoreRoute("admin/elmah.axd");
            routes.IgnoreRoute("admin/{resource}.axd/{*pathInfo}");

            routes.MapRoutes(); // Register Attribute Based Routes which the current assembly contains (RiaLibrary.Web = http://maproutes.codeplex.com)

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            // Route Debugger: to use, uncomment this line:
            //RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);
        }
    }
}