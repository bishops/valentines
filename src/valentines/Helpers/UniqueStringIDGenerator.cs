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

namespace valentines.Helpers
{
    /// <summary>
    /// Generates unique string IDs; essentially, these are like the IDs of bit.ly short links.
    /// </summary>
    public class UniqueStringIDGenerator
    {
        // from http://stackoverflow.com/questions/1275492/how-can-i-create-an-unique-random-sequence-of-characters-in-c/1275824#1275824

        private string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        // or whatever you want.  Include more characters 
        // for more combinations and shorter URLs

        public UniqueStringIDGenerator()
        {

        }
        public UniqueStringIDGenerator(string customAlphabet)
        {
            alphabet = customAlphabet.Trim();
        }

        /// <summary>
        /// To create a unique string ID, supply the integer ID of this record from your database.
        /// </summary>
        /// <param name="databaseId"></param>
        /// <returns></returns>
        public string Encode(int databaseId)
        {
            string encodedValue = String.Empty;
			//int encodingBase = alphabet.Length; // if alphabet is a-z, encodingBase is 26 (base26 encoding).
            while (databaseId > 1) // while (databaseId > encodingBase)
            {
                int remainder;
                encodedValue += alphabet[Math.DivRem(databaseId, alphabet.Length,
                    out remainder) - 1].ToString();
                databaseId = remainder;
            }
            return encodedValue;
        }

        public int Decode(string code)
        {
            int returnValue = 0;

            for (int thisPosition = 0; thisPosition < code.Length; thisPosition++)
            {
                char thisCharacter = code[thisPosition];

                returnValue += alphabet.IndexOf(thisCharacter) * (int)Math.Pow(alphabet.Length, code.Length - thisPosition - 1);
            }
            return returnValue;
        }
    }
}