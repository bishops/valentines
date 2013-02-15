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
using System.Reflection;

namespace valentines.Helpers
{
    //from: http://byatool.com/mvc/custom-data-annotations-with-mvc-how-to-check-multiple-properties-at-one-time/
    /// <summary>
    /// Properties must match. Applied to a class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple=true)]
    public class PropertiesMustMatchAttribute : ValidationAttribute
    {
        /// <summary>
        /// Gets or sets the name of the first property.
        /// </summary>
        /// <value>The first name of the property.</value>
        public String FirstPropertyName { get; set; }
        /// <summary>
        /// Gets or sets the name of the second property.
        /// </summary>
        /// <value>The name of the second property.</value>
        public String SecondPropertyName { get; set; } 

        //Constructor to take in the property names that are supposed to be checked
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertiesMustMatchAttribute"/> class.
        /// </summary>
        /// <param name="firstPropertyName">Name of the first property.</param>
        /// <param name="secondPropertyName">Name of the second property.</param>
        public PropertiesMustMatchAttribute(String firstPropertyName, String secondPropertyName )
        {
            FirstPropertyName = firstPropertyName;
            SecondPropertyName = secondPropertyName;
            if (ErrorMessage.IsNullOrEmpty())
                ErrorMessage = "The fields failed validation.";
        }

        /// <summary>
        /// Determines whether the specified value is valid.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        public override Boolean IsValid(Object value)
        {
            Type objectType = value.GetType();
            //Get the property info for the object passed in.  This is the class the attribute is
            //  attached to
            //I would suggest caching this part... at least the PropertyInfo[]
            PropertyInfo[] neededProperties =
            objectType.GetProperties()
            .Where(propertyInfo => propertyInfo.Name == FirstPropertyName || propertyInfo.Name == SecondPropertyName)
            .ToArray();

            if(neededProperties.Count() != 2)
            {
            throw new ApplicationException("PropertiesMustMatchAttribute error on " + objectType.Name);
            }

            Boolean isValid = true;

            //Convert both values to string and compare...  Probably could be done better than this
            //  but let's not get bogged down with how dumb I am.  We should be concerned about
            //  dumb you are, jerkface.
            if(!Convert.ToString(neededProperties[0].GetValue(value, null)).Equals(Convert.ToString(neededProperties[1].GetValue(value, null))))
            {
                isValid = false;
            }

            return isValid;
        }
    }
}
