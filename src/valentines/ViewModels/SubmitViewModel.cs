using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using valentines.Helpers;

namespace valentines.ViewModels
{
    public class SubmitViewModel
    {
        public List<QuestionDisplay> Questions { get; set; }
        public bool AlreadySubmitted { get; set; }

        public SubmitViewModel()
        {
            Questions = new List<QuestionDisplay>();
        }

        public DateTime FormCloses { get; set; }
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

        [Required(ErrorMessage="Please select an answer for this question.")]
        [MinimumValue(1, ErrorMessage = "Please select an answer for this question.")]
        public int SelectedAnswer
        {
            get;
            set;
        }
    }
}