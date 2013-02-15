/*
 * This file Copyright © 2013 Maxim Zaslavsky, http://maximzaslavsky.com
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

namespace valentines.Helpers
{
    public static class HtmlUtilities
    {
        private const int FetchDefaultTimeoutMs = 2000;

        /// <summary>
        /// returns Html Encoded string
        /// </summary>
        public static string Encode(string html)
        {
            return HttpUtility.HtmlEncode(html);
        }

        /// <summary>
        /// returns Url Encoded string
        /// </summary>
        public static string UrlEncode(string html)
        {
            return HttpUtility.UrlEncode(html);
        }

        /// <summary>
        /// fast (and maybe a bit inaccurate) check to see if the querystring contains the specified key
        /// </summary>
        public static bool QueryStringContains(string url, string key)
        {
            return url.Contains(key + "=");
        }

        /// <summary>
        /// removes the specified key, and any value, from the querystring. 
        /// for www.example.com/bar.foo?x=1&y=2&z=3 if you pass "y" you'll get back 
        /// www.example.com/bar.foo?x=1&z=3
        /// </summary>
        public static string QueryStringRemove(string url, string key)
        {
            if (url.IsNullOrEmpty()) return "";
            return Regex.Replace(url, @"[?&]" + key + "=[^&]*", "");
        }

        /// <summary>
        /// returns the value, if any, of the specified key in the querystring
        /// </summary>
        public static string QueryStringValue(string url, string key)
        {
            if (url.IsNullOrEmpty()) return "";
            return Regex.Match(url, key + "=.*").ToString().Replace(key + "=", "");
        }

        /// <summary>
        /// Produces optional, URL-friendly version of a title, "like-this-one". 
        /// hand-tuned for speed, reflects performance refactoring contributed by John Gietzen (user otac0n) 
        /// </summary>
        public static string URLFriendly(string title)
        {
            if (title == null) return "";

            const int maxlen = 80;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            string s;
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char) (c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' || c == '\\' || c == '-' || c == '_')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c >= 128)
                {
                    s = c.ToString().ToLowerInvariant();
                    if ("àåáâäãåą".Contains(s))
                    {
                        sb.Append("a");
                    }
                    else if ("èéêëę".Contains(s))
                    {
                        sb.Append("e");
                    }
                    else if ("ìíîïı".Contains(s))
                    {
                        sb.Append("i");
                    }
                    else if ("òóôõöø".Contains(s))
                    {
                        sb.Append("o");
                    }
                    else if ("ùúûü".Contains(s))
                    {
                        sb.Append("u");
                    }
                    else if ("çćč".Contains(s))
                    {
                        sb.Append("c");
                    }
                    else if ("żźž".Contains(s))
                    {
                        sb.Append("z");
                    }
                    else if ("śşš".Contains(s))
                    {
                        sb.Append("s");
                    }
                    else if ("ñń".Contains(s))
                    {
                        sb.Append("n");
                    }
                    else if ("ýŸ".Contains(s))
                    {
                        sb.Append("y");
                    }
                    else if (c == 'ł')
                    {
                        sb.Append("l");
                    }
                    else if (c == 'đ')
                    {
                        sb.Append("d");
                    }
                    else if (c == 'ß')
                    {
                        sb.Append("ss");
                    }
                    else if (c == 'ğ')
                    {
                        sb.Append("g");
                    }
                    prevdash = false;
                }
                if (i == maxlen) break;
            }

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        public static HtmlString RenderHtmlInRazor(string html)
        {
            return new HtmlString(html);
        }

    }
}