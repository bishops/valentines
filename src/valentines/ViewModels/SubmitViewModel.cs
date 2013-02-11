using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace valentines.ViewModels
{
    public class SubmitViewModel
    {
        public List<QuestionDisplay> Questions { get; set; }
    }

    public class QuestionDisplay
    {
        public int qID
        {
            get;
            set;
        }
        public string Text
        {
            get;
            set;
        }
        public List<Models.Answer> Answers
        {
            get;
            set;
        }
        public int SelectedAnswer
        {
            get;
            set;
        }
    }
}