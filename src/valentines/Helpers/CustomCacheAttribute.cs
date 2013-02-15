/*
 * This file Copyright © 2013 Maxim Zaslavsky, http://maximzaslavsky.com
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace valentines.Helpers
{
    /// <summary>
    /// [OutputCache] but better.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CustomCacheAttribute : OutputCacheAttribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether [no caching for authenticated users].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [no caching for authenticated users]; otherwise, <c>false</c>.
        /// </value>
        public bool NoCachingForAuthenticatedUsers
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether [allow only valid search engines].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [allow only valid search engines]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowOnlyValidSearchEngines
        {
            get;
            set;
        }
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (VaryByParam.IsNullOrEmpty())
            {
                VaryByParam = "none"; // see http://stackoverflow.com/questions/288608/asp-net-mvc-output-caching-the-directive-or-the-configuration-settings-profile-m
            }
            VaryByParam = VaryByParam.Replace(",", ";"); //proper delimiter
            
            this.Location = System.Web.UI.OutputCacheLocation.Client; // to make sure people only get their own cached stuff
            Current.Context.Response.Cache.SetCacheability(HttpCacheability.Private); // same thing, just in case

            if (filterContext.IsChildAction)
            {
                return; // don't cache child actions - see http://haacked.com/archive/2009/11/18/aspnetmvc2-render-action.aspx "Cooperating with Output Caching"
            }

            if (AllowOnlyValidSearchEngines)
            {
                var IsSearchEngine = (filterContext.Controller as valentines.Controllers.CustomControllerBase).IsSearchEngineDns();
                if (IsSearchEngine)
                {
                    base.OnResultExecuting(filterContext);
                    return;
                }
                return;
            }
            else
            {
                if (NoCachingForAuthenticatedUsers)
                {
                    if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                    {
                        base.OnResultExecuting(filterContext);
                    }
                    return;
                }
                else
                {
                    base.OnResultExecuting(filterContext);
                    return;
                }
            }
        }
    }
}