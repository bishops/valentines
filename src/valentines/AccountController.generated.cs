// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

// Make sure the compiler doesn't complain about missing Xml comments
#pragma warning disable 1591
#region T4MVC

using System;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using T4MVC;
namespace valentines.Controllers {
    public partial class AccountController {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected AccountController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result) {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

		[GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
		protected RedirectToRouteResult RedirectToActionPermanent(ActionResult result) {
			var callInfo = result.GetT4MVCResult();
			return RedirectToRoutePermanent(callInfo.RouteValueDictionary);
		}

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Login() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Login);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult Authenticate() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.Authenticate);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public System.Web.Mvc.ActionResult OpenidRegisterFormSubmit() {
            return new T4MVC_ActionResult(Area, Name, ActionNames.OpenidRegisterFormSubmit);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public AccountController Actions { get { return MVC.Account; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Account";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass {
            public readonly string Login = "Login";
            public readonly string Authenticate = "Authenticate";
            public readonly string OpenidRegisterFormSubmit = "OpenidRegisterFormSubmit";
            public readonly string LogOff = "LogOff";
            public readonly string LogOut = "LogOut";
        }


        static readonly ViewNames s_views = new ViewNames();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewNames Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewNames {
            public readonly string LogOff = "~/Views/Account/LogOff.cshtml";
            public readonly string OpenidLogin = "~/Views/Account/OpenidLogin.cshtml";
            public readonly string OpenidRegister = "~/Views/Account/OpenidRegister.cshtml";
            public readonly string WhiteListBlock = "~/Views/Account/WhiteListBlock.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public class T4MVC_AccountController: valentines.Controllers.AccountController {
        public T4MVC_AccountController() : base(Dummy.Instance) { }

        public override System.Web.Mvc.ActionResult Login(System.Guid? OneTimeSignupCode, string ReturnUrl) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Login);
            callInfo.RouteValueDictionary.Add("OneTimeSignupCode", OneTimeSignupCode);
            callInfo.RouteValueDictionary.Add("ReturnUrl", ReturnUrl);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult Authenticate(string returnUrl) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.Authenticate);
            callInfo.RouteValueDictionary.Add("returnUrl", returnUrl);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult OpenidRegisterFormSubmit(valentines.ViewModels.OpenIdRegistrationViewModel model) {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.OpenidRegisterFormSubmit);
            callInfo.RouteValueDictionary.Add("model", model);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult LogOff() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.LogOff);
            return callInfo;
        }

        public override System.Web.Mvc.ActionResult LogOut() {
            var callInfo = new T4MVC_ActionResult(Area, Name, ActionNames.LogOut);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
