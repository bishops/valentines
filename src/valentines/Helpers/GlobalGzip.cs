/*
 * This file Copyright © 2013 Maxim Zaslavsky, http://maximzaslavsky.com
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web;
using System.Collections.Specialized;
using System.IO;


namespace valentines.Helpers
{
    public class GlobalGzip : IHttpModule
    {
        private EventHandler onBeginRequest;

        public GlobalGzip()
        {
            onBeginRequest = new EventHandler(this.HandleBeginRequest);
        }

        void IHttpModule.Dispose()
        {

        }
        void IHttpModule.Init(HttpApplication context)
        {
            context.BeginRequest += onBeginRequest;
        }


        private void HandleBeginRequest(object sender, EventArgs evargs)
        {
            HttpContext context = HttpContext.Current;
            if (context.Request.Headers["Accept-encoding"] != null && (context.Request.Headers["Accept-encoding"] as string).Contains("gzip"))
            {
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                HttpContext.Current.Response.AppendHeader("Content-encoding", "gzip");
            }
            HttpContext.Current.Response.Cache.VaryByHeaders["Accept-encoding"] = true;
        }
    }
}
