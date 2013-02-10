using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace valentines.Models
{
    public class Matcher
    {
        private ValentinesDataContext db;
        public Matcher(ValentinesDataContext DB)
        {
            this.db = DB;
        }

        public void computeMatches()
        {

        }
    }
}