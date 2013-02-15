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
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace valentines.Helpers
{
    /// <summary>
    /// Checks that the field/property is a valid email address.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property,AllowMultiple=false)]
    public class IsValidEmailAddressAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsValidEmailAddressAttribute"/> class.
        /// </summary>
        public IsValidEmailAddressAttribute() : base()
        {
            if(ErrorMessage.IsNullOrEmpty())
                ErrorMessage = "Please enter a valid email address.";
        }
        /// <summary>
        /// Determines whether the specified value is valid.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsValid(object value)
        {
            if (value == null) //we don't care if it's required or not.
            {
                return true;
            }
            var address = (string)value;
            try
            {
                var addressMail = new MailAddress(address);
                addressMail = null;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}