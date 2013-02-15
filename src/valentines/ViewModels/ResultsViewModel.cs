using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace valentines.ViewModels
{
    public class ResultsViewModel
    {
        public List<Models.Match> AllSchoolMales { get; set; }
        public List<Models.Match> AllSchoolFemales { get; set; }
        public List<Models.Match> YourGradeMales { get; set; }
        public List<Models.Match> YourGradeFemales { get; set; }

        public Models.Match Nemesis { get; set; }
    }
}