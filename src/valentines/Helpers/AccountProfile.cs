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
using System.Web.Profile;
using System.Web.Security;

namespace valentines
{
    public class AccountProfile : ProfileBase
    {
        /// <summary>
        /// Gets the current user's Profile.
        /// </summary>
        public static AccountProfile CurrentUser
        {
            get
            {
                if (Membership.GetUser() != null)
                    return ProfileBase.Create(Membership.GetUser().UserName) as AccountProfile;
                else
                    return null;
            }
        }

        public static AccountProfile GetProfileOfUser(string userName)
        {
            return ProfileBase.Create(userName) as AccountProfile;
        }

        internal static AccountProfile NewUser
        {
            get { return System.Web.HttpContext.Current.Profile as AccountProfile; }
        }


        public int Grade
        {
            get { return ((int)(base["Grade"])); }
            set { base["Grade"] = value; Save(); }
        }

        public int Sex
        {
            get { return ((int)(base["Sex"])); }
            set { base["Sex"] = value; Save(); }
        }

        public string FullName
        {
            get { return ((string)(base["FullName"])); }
            set { base["FullName"] = value; Save(); }
        }
        // add additional properties here

        //To use AccountProfile from other places:
        //AccountProfile currentProfile = AccountProfile.CurrentUser;
        //currentProfile.FullName = "Snoopy";
        //currentProfile.OtherProperty = "ABC";
        //currentProfile.Save();

    }
}