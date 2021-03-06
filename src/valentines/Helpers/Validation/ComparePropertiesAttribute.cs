﻿/*
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
    /// Compares two properties. Applied to a class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ComparePropertiesAttribute : ValidationAttribute
    {
        public ComparePropertiesAttribute() : base()
        {
            if(ErrorMessage.IsNullOrEmpty())
                ErrorMessage = "The fields failed validation.";
        }
        /// <summary>
        /// Gets or sets the first property to compare.
        /// </summary>
        /// <value>The first property to compare.</value>
        public string ComparisonProperty1
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the second property to compare.
        /// </summary>
        /// <value>The second property to compare.</value>
        public string ComparisonProperty2
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

        private static IComparable GetComparablePropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;
            var type = obj.GetType();
            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo == null) return null;
            return propertyInfo.GetValue(obj, null) as IComparable;
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
            var comp1 = GetComparablePropertyValue(value, ComparisonProperty1);
            var comp2 = GetComparablePropertyValue(value, ComparisonProperty2);

            if (comp1 == null && comp2 == null)
            {
                return AllowNullValues;
            }
            if (comp1 == null || comp2 == null)
                return false;

            var result = comp1.CompareTo(comp2);

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