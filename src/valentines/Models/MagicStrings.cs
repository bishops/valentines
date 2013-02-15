using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace valentines.Models
{
    public static class MagicStrings
    {
        public static dynamic SexList = new[] 
                {
                    new {
                        Id = 1,
                        Name = "Male"
                    },
                    new {
                        Id = 2,
                        Name = "Female"
                    }
                };
    }
}