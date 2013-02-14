using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using valentines.Models;
using RiaLibrary.Web;
using valentines.Helpers;
using System.Text;
using System.Web.Mail;
using valentines.ViewModels;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using System.Net;
using System.Configuration;
using System.Web.Routing;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;


namespace valentines.Controllers
{
    /// <summary>
    /// Handles account methods.
    /// </summary>
    [HandleError]
    public partial class AccountController : CustomControllerBase
    {
        private static readonly OpenIdRelyingParty openid = new OpenIdRelyingParty();
        static bool LimitToBishopsOpenIds = false;
        static bool LimitToUpperSchool = false;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ViewBag.curPage = "Account";
        }

        // This constructor is used by the MVC framework to instantiate the controller using
        // the default forms authentication and membership providers.

        public AccountController()
            : this(null, null)
        {
        }

        // This constructor is not used by the MVC framework but is instead provided for ease
        // of unit testing this type. See the comments at the end of this file for more
        // information.
        public AccountController(IFormsAuthentication formsAuth, IMembershipService service)
        {
            FormsAuth = formsAuth ?? new FormsAuthenticationService();
            MembershipService = service ?? new AccountMembershipService();
        }

        public IFormsAuthentication FormsAuth
        {
            get;
            private set;
        }

        public IMembershipService MembershipService
        {
            get;
            private set;
        }

        #region OpenID Login and Registration

        [Url("Account/Login/{OneTimeSignupCode?}")]
        [CustomAuthorization(OnlyAllowUnauthenticatedUsers = true)]
        public virtual ActionResult Login(Guid? OneTimeSignupCode, string ReturnUrl)
        {
            if (OneTimeSignupCode.HasValue)
            {
                var db = Current.DB;
                var record = db.OneTimeRegistrationCodes.Where(s => s.Id == OneTimeSignupCode.Value).SingleOrDefault();
                if (record == null || record.UsesRemaining < 1)
                {
                    return RedirectToAction("NotFound", "Error");
                }
                ViewData["OneTimeSignupCode"] = OneTimeSignupCode.Value.ToString();
                if (record.CustomWelcomeName.HasValue())
                {
                    ViewData["WelcomeName"] = record.CustomWelcomeName.Trim();
                }
            }
            if (ReturnUrl.HasValue() && Url.IsLocalUrl(ReturnUrl))
            {
                Session["ReturnURL"] = ReturnUrl;
            }
            return View("OpenidLogin");
        }

        [Url("Account/Authenticate")]
        [ValidateInput(false)]
        [CustomAuthorization(OnlyAllowUnauthenticatedUsers = true)]
        public virtual ActionResult Authenticate(string returnUrl)
        {
            var db = Current.DB;
            if (Request.Form["OneTimeSignupCode"].HasValue())
            {
                Session["OneTimeSignupCode"] = Request.Form["OneTimeSignupCode"];
            }

            // Google Apps only:
            /*
            openid.DiscoveryServices.Clear();
            openid.DiscoveryServices.Insert(0, new HostMetaDiscoveryService() { UseGoogleHostedHostMeta = true }); // this causes errors // previously was Add()

             */
            // Normal
            IAuthenticationResponse response = openid.GetResponse();
            OneTimeRegistrationCode recordcopy = null;
            if (response == null)
            {
                // Stage 2: user submitting Identifier
                Identifier id;

                if (Identifier.TryParse(Request.Form["openid_identifier"], out id))
                {
                    if (WhiteListEnabled)
                    {
                        if (Request.Form["OneTimeSignupCode"].HasValue())
                        {
                            var record = db.OneTimeRegistrationCodes.Where(c => c.Id.ToString() == Request.Form["OneTimeSignupCode"]).SingleOrDefault();
                            if (record == null)
                            {
                                //not allowed in
                                Current.Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                return View("WhiteListBlock");
                            }
                        }
                    }
                    try
                    {
                        IAuthenticationRequest request = openid.CreateRequest(Request.Form["openid_identifier"]);

                        var f = new FetchRequest();
                        
                            f.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
                            f.Attributes.AddRequired(WellKnownAttributes.Name.First);
                            f.Attributes.AddRequired(WellKnownAttributes.Name.Last);
                            f.Attributes.AddOptional(WellKnownAttributes.Name.Alias);
                        request.AddExtension(f);

                        return request.RedirectingResponse.AsActionResult();
                    }
                    catch (ProtocolException ex)
                    {
                        ViewData["Message"] = ex.Message;
                        if (Request.Form["OneTimeSignupCode"].HasValue())
                        {
                            ViewData["OneTimeSignupCode"] = Request.Form["OneTimeSignupCode"];
                        }
                        return View("OpenidLogin");
                    }
                }
                else
                {
                    ViewData["Message"] = "Invalid OpenID";
                    if (Request.Form["OneTimeSignupCode"].HasValue())
                    {
                        ViewData["OneTimeSignupCode"] = Request.Form["OneTimeSignupCode"];
                    }
                    return View("OpenidLogin");
                }
            }
            else
            {
                // Stage 3: OpenID Provider sending assertion response
                switch (response.Status)
                {
                    case AuthenticationStatus.Authenticated:
                        var sreg = response.GetExtension<FetchResponse>();

                        UserOpenId openId = null;
                        openId = db.UserOpenIds.Where(o => o.OpenIdClaim == response.ClaimedIdentifier.ToString()).FirstOrDefault();
                        object signupcode = null;
                        if (Request.Form["OneTimeSignupCode"].HasValue())
                        {
                            signupcode = Request.Form["OneTimeSignupCode"];
                        }
                        else if (Session["OneTimeSignupCode"] != null)
                        {
                            signupcode = Session["OneTimeSignupCode"];
                        }
                        if (WhiteListEnabled)
                        {
                            if (signupcode != null)
                            {
                                var record = db.OneTimeRegistrationCodes.Where(c => c.Id.ToString() == (string)signupcode).SingleOrDefault();
                                if (record == null)
                                {
                                    //not allowed in
                                    try
                                    {
                                        Current.Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    }
                                    catch
                                    {

                                    }
                                    return View("WhiteListBlock");
                                }
                                recordcopy = record;
                                --record.UsesRemaining;
                                if (record.UsesRemaining < 1)
                                {
                                    db.OneTimeRegistrationCodes.DeleteOnSubmit(record);
                                }
                                db.SubmitChanges();
                            }
                            //else if (db.OpenIDWhiteLists.Where(w => w.IsEnabled).Where(w => w.OpenID == response.ClaimedIdentifier.OriginalString).FirstOrDefault() == null && (sreg == null || !sreg.Email.Contains("APPROVEDOPENIDDOMAIN.com") && openId == null))
                            else if ((db.OpenIDWhiteLists.Where(w => w.IsEnabled).Where(w => w.OpenID == response.ClaimedIdentifier.ToString()).FirstOrDefault() == null || sreg == null) && openId == null) // if (not-in-whitelisted-openids or no-openid-submitted) and doesn't-match-any-openid-in-the-system
                            {
                                //not allowed in
                                try
                                {
                                    Current.Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                }
                                catch
                                {

                                }
                                return View("WhiteListBlock");
                            }
                        }


                        if (openId == null)
                        {
                            // create new user
                            string email = "";
                            string login = "";
                            string name = "";
                            if (sreg != null)
                            {
                                email = sreg.GetAttributeValue(WellKnownAttributes.Contact.Email);
                                var nick = "";
                                if (email.IndexOf("@bishopsstudent.org") == -1)
                                {
                                    if (LimitToBishopsOpenIds)
                                    {
                                        ViewData["Message"] = "Please try again and use your Bishop's student email address!";
                                        return View("OpenidLogin");
                                    }
                                    var potentialNick = sreg.GetAttributeValue(WellKnownAttributes.Name.Alias);
                                    if (potentialNick.HasValue())
                                    {
                                        nick = potentialNick;
                                    }
                                    else
                                    {
                                        // make something random
                                        nick = new Random().Next(500,500000).ToString();
                                    }
                                }
                                else
                                {
                                    nick = email.Substring(0, email.IndexOf("@bishopsstudent.org"));
                                }
                                var userNameAvailable = (db.aspnet_Users.Where(u => u.UserName == nick).FirstOrDefault()) == null;
                                login = nick;
                                
                                name = sreg.GetAttributeValue(WellKnownAttributes.Name.First) + " " + sreg.GetAttributeValue(WellKnownAttributes.Name.Last);
                            }

                            // Check in Bishop's class lists (9th to 12th grades) to see if we should allow this user to join (and also fetch their grade level)
                            var lookup = db.BishopsEmails.Where(b => b.Username == login).FirstOrDefault();
                            var grade = 9; // default
                            var gradeSet = false; // should we make grade field disabled in registration form (true = we set grade here and user cannot change, false = user must provide manually)
                            if (lookup == null)
                            {
                                if (LimitToUpperSchool)
                                {
                                    ViewData["Message"] = "Sorry, but only Upper School students may join the site.";
                                    return View("OpenidLogin");
                                }
                            }
                            else
                            {
                                grade = lookup.Grade;
                                gradeSet = true;
                            }
                            
                            var model = new OpenIdRegistrationViewModel()
                            {
                                EmailAddress = email,
                                Nickname = login,
                                FullName = name,
                                Grade = grade,
                                GradeSet = gradeSet,
                                OpenIdClaim = Crypto.EncryptStringAES(response.ClaimedIdentifier.ToString(), "secretstring"),
                                ReturnURL = Session["ReturnURL"] as string
                            };
                            return View("OpenidRegister", model);
                        }
                        else // openId record is not null
                        {
                            var userName = openId.aspnet_User.UserName;

                            FormsAuthentication.SetAuthCookie(userName, true);

                            // Don't use return URL because the user may have accidentally clicked "results" nav link before logging in while results page isn't open yet.
                            /*var URLreturn = Session["ReturnURL"];
                            if (URLreturn == null || !(URLreturn as string).HasValue())
                            {
                                return RedirectToAction("Index", "Home");
                            }
                            return Redirect(URLreturn as string);*/

                            // Decide where to go next
                            if (System.Configuration.ConfigurationManager.AppSettings["ResultsOpen"] != "true")
                            {
                                return RedirectToAction("Index", "Home"); // Send to questionnaire page.
                            }
                            else
                            {
                                return RedirectToAction("Results", "Home"); // Send to results page (if they haven't submitted, it will redirect to form-is-closed page
                            }
                        }

                    case AuthenticationStatus.Canceled:
#if DEBUG
                        ViewData["Message"] = "Canceled at provider";
#else
                        ViewData["Message"] = "Canceled - please try again!";
#endif
                        return View("OpenidLogin");
                    case AuthenticationStatus.Failed:
#if DEBUG
                        ViewData["Message"] = response.Exception.Message;
#else
                        ViewData["Message"] = "Sorry, something went wrong. Please try again!";
#endif
         
                        return View("OpenidLogin");
                }
            }
            return new EmptyResult();
        }

        /// <summary>
        /// Handles OpenID registration form submission.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="captchaValid">if set to <c>true</c> [captcha valid].</param>
        /// <returns></returns>
        [Url("Account/Register/OpenID")]
        [CustomAuthorization(OnlyAllowUnauthenticatedUsers = true)]
        [HttpPost]
        [VerifyReferrer]
        [ValidateAntiForgeryToken]
        public virtual ActionResult OpenidRegisterFormSubmit(OpenIdRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("OpenidRegister", model);
            }

            var DecryptedOpenID = Crypto.DecryptStringAES(model.OpenIdClaim, "secretstring");
            var validator = new IsSemiValidURLAttribute();
            var isValid = validator.IsValid(DecryptedOpenID);
            validator = null;
            if (!isValid)
            {
                //User tried to spoof encryption
                ModelState.AddModelError("OpenID", "There's a problem with the OpenID that you specified.");
                return View("OpenidRegister", model);
            }

            try
            {
                var db = Current.DB;
                var userNameAvailable = (db.aspnet_Users.Where(u => u.UserName == model.Nickname).FirstOrDefault()) == null;
                if (!userNameAvailable)
                {
                    ModelState.AddModelError("Username", "This username is already taken.");
                    return View("OpenidRegister", model);
                }

                // Attempt to register the user
                MembershipCreateStatus createStatus = MembershipService.CreateUser(model.Nickname, Membership.GeneratePassword(7, 0), model.EmailAddress);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    AccountProfile.NewUser.Initialize(model.Nickname, true);
                    AccountProfile.NewUser.FullName = model.FullName.Trim();
                    AccountProfile.NewUser.Grade = model.Grade;
                    AccountProfile.NewUser.Sex = model.SelectedSex;
                    AccountProfile.NewUser.Save();
                    try
                    {
                        //Check OpenID-whitelist status and add OpenID to whitelist if needed
                        if (WhiteListEnabled)
                        {
                            //If we got here, this means that the user used a valid one-time registration code.
                            var whitelistRecord = new OpenIDWhiteList();
                            whitelistRecord.OpenID = DecryptedOpenID;
                            whitelistRecord.IsEnabled = true;
                            db.OpenIDWhiteLists.InsertOnSubmit(whitelistRecord);
                            db.SubmitChanges();
                        }

                        var userid = db.aspnet_Users.Where(u => u.UserName == model.Nickname).Single().UserId; // if we fail here, this usually means that we didn't specify a constant ApplicationName in Web.config, so each user has multiple entries in that table.

                        var openid = new UserOpenId();
                        openid.OpenIdClaim = DecryptedOpenID;
                        openid.UserId = userid;
                        db.UserOpenIds.InsertOnSubmit(openid);
                        db.SubmitChanges();

                        FormsAuth.SignIn(model.Nickname, true /* createPersistentCookie */);

                        if (ConfigurationManager.AppSettings["PromptEmailConfirmation"] == "true")
                        {
                            ViewData["email"] = model.EmailAddress;
                            return View("TimeToValidateYourEmailAddress");
                        }
                        else
                        {
                            /*if (model.ReturnURL.HasValue())
                            {
                                return Redirect(model.ReturnURL);
                            }*/

                            // Decide where to go next
                            if (System.Configuration.ConfigurationManager.AppSettings["ResultsOpen"] != "true")
                            {
                                return RedirectToAction("Index", "Home"); // Send to questionnaire page.
                            }
                            else
                            {
                                return RedirectToAction("Results", "Home"); // Send to results page (if they haven't submitted, it will redirect to form-is-closed page
                            }
                        }
                    }

                    catch
                    {
                        ModelState.AddModelError("_FORM", ErrorCodeToString(createStatus));
                        return View("OpenidRegister", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("_FORM", ErrorCodeToString(createStatus));
                    return View("OpenidRegister", model);
                }
            }
            catch
            {
                return RedirectToAction("InternalServerError", "Error");
            }
        }
        #endregion

        #region Logout

        /// <summary>
        /// Handles logoff.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Url("Account/LogOff")]
        public virtual ActionResult LogOff()
        {
            //separated into Get and Post to prevent attacks - see http://meta.stackoverflow.com/questions/57159/stack-overflow-wmd-editor-anti-csrf/57160#57160
            return View();
        }
        [HttpPost]
        [VerifyReferrer]
        [Authorize]
        [Url("Account/LogOutPOST")]
        public virtual ActionResult LogOut()
        {
            FormsAuth.SignOut();

            return RedirectToAction("Index", "Home");
        }
        #endregion

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity is WindowsIdentity)
            {
                throw new InvalidOperationException("Windows authentication is not supported.");
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validates the change of email.
        /// </summary>
        /// <param name="newEmail">The new email.</param>
        /// <param name="confirmEmail">The confirm email.</param>
        /// <returns></returns>
        private bool ValidateChangeEmail(string newEmail, string confirmEmail)
        {
            if (String.IsNullOrEmpty(newEmail))
            {
                ModelState.AddModelError("NewEmail", "You must specify a new email address.");
            }
            else
            {
                try
                {
                    var a = new System.Net.Mail.MailAddress(newEmail);
                    a = null;

                    //check whether email is already taken
                    if (!string.IsNullOrEmpty(Membership.GetUserNameByEmail(newEmail)))
                    {
                        ModelState.AddModelError("NewEmail", "A user already exists with this email address.");
                    }
                }
                catch
                {
                    ModelState.AddModelError("NewEmail", "You must specify a valid email address.");
                }
            }
            if (String.IsNullOrEmpty(confirmEmail))
            {
                ModelState.AddModelError("ConfirmEmail", "You must enter the new email address a second time.");
            }
            else
            {
                try
                {
                    var a = new System.Net.Mail.MailAddress(confirmEmail);
                    a = null;
                }
                catch
                {
                    ModelState.AddModelError("ConfirmEmail", "You must specify a valid email address.");
                }
            }
            if (!String.Equals(newEmail, confirmEmail, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", "The new email and confirmation email do not match.");
            }
            return ModelState.IsValid;
        }
        /// <summary>
        /// Validates the change of password.
        /// </summary>
        /// <param name="currentPassword">The current password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="confirmPassword">The confirm password.</param>
        /// <returns></returns>
        private bool ValidateChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (String.IsNullOrEmpty(currentPassword))
            {
                ModelState.AddModelError("currentPassword", "You must specify a current password.");
            }
            if (newPassword == null || newPassword.Length < MembershipService.MinPasswordLength)
            {
                ModelState.AddModelError("newPassword",
                    String.Format(CultureInfo.CurrentCulture,
                         "You must specify a new password of {0} or more characters.",
                         MembershipService.MinPasswordLength));
            }

            if (!String.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
            }

            return ModelState.IsValid;
        }

        /// <summary>
        /// Validates the reset change of password.
        /// </summary>
        /// <param name="newPassword">The new password.</param>
        /// <param name="confirmPassword">The confirm password.</param>
        /// <returns></returns>
        private bool ValidateResetPassword(string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < MembershipService.MinPasswordLength)
            {
                ModelState.AddModelError("newPassword",
                    String.Format(CultureInfo.CurrentCulture,
                         "You must specify a new password of {0} or more characters.",
                         MembershipService.MinPasswordLength));
            }

            if (!String.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
            }

            return ModelState.IsValid;
        }
        /// <summary>
        /// Validates the log on.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private bool ValidateLogOn(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("username", "You must specify a username.");
            }
            if (String.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "You must specify a password.");
            }
            if (!MembershipService.ValidateUser(userName, password))
            {
                ModelState.AddModelError("_FORM", "The username or password provided is incorrect.");
            }

            return ModelState.IsValid;
        }

        /// <summary>
        /// Validates the registration.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="confirmPassword">The confirm password.</param>
        /// <returns></returns>
        private bool ValidateRegistration(string userName, string email, string password, string confirmPassword)
        {
            if (String.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("username", "You must specify a username.");
            }
            if (String.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("email", "You must specify an email address.");
            }
            try
            {
                //validate email
                var a = new System.Net.Mail.MailAddress(email);
                a = null;
            }
            catch
            {
                //if an exception occurred, the email is invalid
                ModelState.AddModelError("email", "You must specify a valid email address.");
            }
            if (password == null || password.Length < MembershipService.MinPasswordLength)
            {
                ModelState.AddModelError("password",
                    String.Format(CultureInfo.CurrentCulture,
                         "You must specify a password of {0} or more characters.",
                         MembershipService.MinPasswordLength));
            }
            if (!String.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", "The new password and confirmation password do not match.");
            }
            return ModelState.IsValid;
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://msdn.microsoft.com/en-us/library/system.web.security.membershipcreatestatus.aspx for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }

    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IFormsAuthentication
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthentication
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }
        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        bool ValidateUser(string userName, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
    }

    public class AccountMembershipService : IMembershipService
    {
        private MembershipProvider _provider;

        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            return _provider.ValidateUser(userName, password);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
            return currentUser.ChangePassword(oldPassword, newPassword);
        }



    }
}
