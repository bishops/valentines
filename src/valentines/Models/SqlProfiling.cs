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
using System.Configuration;
using System.Data.SqlClient;
using StackExchange.Profiling;

namespace valentines.Models
{
    /// <summary>
    /// This partial class is for MvcMiniProfiler, so that SQL queries are also profiled. See http://code.google.com/p/mvc-mini-profiler/ and http://code.google.com/p/stack-exchange-data-explorer/source/browse/App/StackExchange.DataExplorer/Models/DBContext.cs
    /// </summary>
    public partial class ValentinesDataContext
    {
        /// <summary>
        /// Answers a new DBContext for the current site.
        /// </summary>
        public static ValentinesDataContext GetContext()
        {
            var cnnString = ConfigurationManager.ConnectionStrings["valentinesConnectionString"].ConnectionString;
            var conn = new StackExchange.Profiling.Data.ProfiledDbConnection(new SqlConnection(cnnString), MiniProfiler.Current);
            return new ValentinesDataContext(conn);
        }

    }
}