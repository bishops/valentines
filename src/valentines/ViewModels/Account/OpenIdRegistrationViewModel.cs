using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using valentines.Helpers;
using DotNetOpenAuth.OpenId;
using System.Web.Mvc;
using valentines.Models;

namespace valentines.ViewModels
{
    public class OpenIdRegistrationViewModel
    {
        [Required(ErrorMessage="You must specify an email address.")]
        [Display(Name="Email address",Description="Enter your email address here.")]
        [IsValidEmailAddress(ErrorMessage="This is not a valid email address.")]
        public string EmailAddress
        {
            get;
            set;
        }
        [Display(Name="Username",Description="Enter your username for the site here.")]
        [Required(ErrorMessage="You must choose a username.")]
        public string Nickname
        {
            get;
            set;
        }
        [Display(Name="Full Name",Description="Please enter your real name here.")]
        [Required(ErrorMessage = "You must enter your full name.")]
        public string FullName
        {
            get;
            set;
        }

        [Display(Name = "Grade", Description = "Please enter your grade level (e.g. 11).")]
        public int Grade
        {
            get;
            set;
        }

        [Required(ErrorMessage="You must enter your sex.")]
        [DisplayName("Sex")]
        public int SelectedSex
        {
            get;
            set;
        }

        [DisplayName("Sex options")]
        public IEnumerable<SelectListItem> Sexes
        {
            get;
            set;
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdRegistrationViewModel"/> class.
        /// </summary>
        public OpenIdRegistrationViewModel()
        {
            if (Sexes == null)
            {
                Sexes = new SelectList(MagicStrings.SexList, "Id", "Name");
            }
            if (Grade == null || Grade == 0)
            {
                Grade = 9;
            }
        }

        [Required(ErrorMessage="An OpenID claim must be included.")]
        public string OpenIdClaim
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the return URL.
        /// </summary>
        /// <value>
        /// The return URL.
        /// </value>
        public string ReturnURL
        {
            get;
            set;
        }

    }
}