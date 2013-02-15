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
using valentines.Controllers;
using System.Web.Security;
using System.Web.Routing;
using System.Configuration;

namespace valentines.Helpers
{
    public class CustomAuthorizationAttribute : ActionFilterAttribute
    {
       
        /// <summary>
        /// Gets or sets the authorized roles.
        /// </summary>
        /// <value>The authorized roles.</value>
        public string AuthorizedRoles
        {
            get;
            set; 
        }
        /// <summary>
        /// Gets or sets the unauthorized roles.
        /// </summary>
        /// <value>The unauthorized roles.</value>
        public string UnauthorizedRoles
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether only unauthenticated (anonymous) users should be allowed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if only unauthenticated (anonymous) users should be allowed; otherwise, <c>false</c>.
        /// </value>
        public bool OnlyAllowUnauthenticatedUsers
        {
            get;
            set;
        }

        /// <summary>
        /// Called by the MVC framework before the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                if(OnlyAllowUnauthenticatedUsers)
                {
                    return;
                }
                //use the current url for the redirect
                string redirectOnSuccess = filterContext.HttpContext.Request.Url.AbsolutePath;

                //send them off to the login page
                string redirectUrl = string.Format("?ReturnUrl={0}", redirectOnSuccess);
                string loginUrl = FormsAuthentication.LoginUrl + redirectUrl;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.Redirect(loginUrl, true);
                return;

            }
            else //User is authenticated
            {
                var httpContext = new HttpContextWrapper(HttpContext.Current);
                var requestContext = new RequestContext(httpContext, new RouteData());
                var u = new UrlHelper(requestContext);

                if(OnlyAllowUnauthenticatedUsers)
                {
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.Redirect(u.Action("Forbidden", "Error"), true);
                    return;
                }
                

                //Unauthorized roles
                bool isUnAuthorized = false;
                if (!string.IsNullOrEmpty(UnauthorizedRoles))
                {
                    if (!(UnauthorizedRoles.Trim() == ""))
                    {
                        var roleSplit = UnauthorizedRoles.Split(',');
                        foreach (var role in roleSplit)
                        {
                            if (filterContext.HttpContext.User.IsInRole(role.Trim()))
                            {
                                isUnAuthorized = true;
                                break;
                            }
                        }
                    }
                }
                if (isUnAuthorized)
                {
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.Redirect(u.Action("Forbidden", "Error"), true);
                    return;
                }

                //Authorized roles
                bool isAuthorized = false;
                if(!string.IsNullOrEmpty(AuthorizedRoles))
                {
                    if(!(AuthorizedRoles.Trim() == ""))
                    {
                        var roleSplit = AuthorizedRoles.Split(',');
                        foreach (var role in roleSplit)
                        {
                            if (filterContext.HttpContext.User.IsInRole(role.Trim()))
                            {
                                isAuthorized = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    isAuthorized = true;
                }
                if (!isAuthorized)
                {
                    filterContext.HttpContext.Response.Clear();
                    filterContext.HttpContext.Response.Redirect(u.Action("Forbidden", "Error"), true);
                    return;
                }
            }
        }
    }
}