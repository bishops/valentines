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

namespace valentines.Helpers
{
    //from: http://stackoverflow.com/questions/1607832/writing-a-compareto-dataannotation-attribute
    /// <summary>
    /// Compares a property to a given comparison value. Applied to a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class CompareValueAttribute : ValidationAttribute
    {
        public CompareValueAttribute() : base()
        {
            if(ErrorMessage.IsNullOrEmpty())
                ErrorMessage = "The value failed validation.";
        }
        /// <summary>
        /// Gets or sets the comparison value.
        /// </summary>
        /// <value>The comparison value.</value>
        public object ComparisonValue
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether the first property can be less than the second property.
        /// </summary>
        /// <value><c>true</c> if less than is allowed; otherwise, <c>false</c>.</value>
        public bool LessThanAllowed
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether the first property can be equal to the second property.
        /// </summary>
        /// <value><c>true</c> if equal to is allowed; otherwise, <c>false</c>.</value>
        public bool EqualToAllowed
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether the first property can be greater than the second property.
        /// </summary>
        /// <value><c>true</c> if greater than is allowed; otherwise, <c>false</c>.</value>
        public bool GreaterThanAllowed
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether null values are allowed.
        /// </summary>
        /// <value><c>true</c> if null values are allowed; otherwise, <c>false</c>.</value>
        public bool AllowNullValues
        {
            get;
            set;
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
            if (value == null)
            {
                return AllowNullValues;
            }
            if(ComparisonValue==null)
            {
                throw new ArgumentNullException("ComparisonValue");
            }

            int result = 0;

            if(ComparisonValue as string == "DateTime.Now")
            {
                result = (value as IComparable).CompareTo((DateTime.Now as IComparable));
            }
            else
            {
                result = (value as IComparable).CompareTo((ComparisonValue as IComparable));
            }

            

            switch (result)
            {
                case -1: return LessThanAllowed;
                case 0: return EqualToAllowed;
                case 1: return GreaterThanAllowed;
                default: throw new ApplicationException("Something just failed in ComparePropertiesAttribute");
            }
        }
    }
}