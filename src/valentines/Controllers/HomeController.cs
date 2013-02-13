using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiaLibrary.Web;
using valentines.Models;
using valentines.ViewModels;
using valentines.Helpers;
using System.Threading;
using System.Web.Security;

namespace valentines.Controllers
{
    public partial class HomeController : Controller
    {
        [Url("")]
        [HttpGet]
        [Authorize]
        public virtual ActionResult Index()
        {
            ViewBag.curPage = "Home";
            var db = Current.DB;

            var responses = db.Responses.Where(r => r.UserId == Current.UserID.Value);
            if (responses.Any()) // responses already exist for this user
            {
                if (System.Configuration.ConfigurationManager.AppSettings["ResultsOpen"] == "true")
                {
                    return RedirectToAction("Results"); // show results page
                }
                return RedirectToAction("AlreadySubmitted"); // show thank you screen
            }

            var deadline = DateTime.Parse(System.Configuration.ConfigurationManager.AppSettings["FormDeadline"]);
            if (DateTime.Now > deadline)
            {
                // The form deadline has passed and they have not submitted anything :(
                return View("MissedTheDeadline");
            }
            
            var model = new SubmitViewModel();
            model.FormCloses = deadline;
            foreach (var q in db.Questions.OrderBy(q=>q.Id))
            {
                model.Questions.Add(new QuestionDisplay() { qID = q.Id, Text = q.Text, Answers = q.Answers.OrderBy(a=>a.AnswerOrder).ToList()});
            }
            model.AlreadySubmitted = false;
            return View(model);
        }

        [Url("")]
        [HttpPost]
        [Authorize]
        [ValidateInput(true)]
        [VerifyReferrer]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Index(SubmitViewModel model)
        {
            ViewBag.curPage = "Home";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var db = Current.DB;

            // Just in case, start with blank slate
            db.Responses.DeleteAllOnSubmit(db.Responses.Where(r => r.UserId == Current.UserID));
            db.SubmitChanges();

            // Insert responses
            foreach (var q in model.Questions)
            {
                var r = new Response();
                r.UserId = Current.UserID.Value;
                r.QuestionId = q.qID;
                r.AnswerId = q.SelectedAnswer;
                r.Date = DateTime.UtcNow;
                db.Responses.InsertOnSubmit(r);
            }
            db.SubmitChanges();

            return RedirectToAction("AlreadySubmitted");
        }

        [Url("done")]
        public virtual ActionResult AlreadySubmitted()
        {
            // Mode for having already submitted the form - data is loaded but locked, and we display a "thank you" message at the top.
            ViewBag.curPage = "Home";
            var db = Current.DB;

            var responses = db.Responses.Where(r => r.UserId == Current.UserID.Value);
            if (!responses.Any()) // no responses exist for this user, i.e. they haven't submitted the form yet, so we're in the wrong place
            {
                return RedirectToAction("Index");
            }

            if (System.Configuration.ConfigurationManager.AppSettings["ResultsOpen"] == "true")
            {
                return RedirectToAction("Results"); // show results page
            }

            var model = new SubmitViewModel();
            foreach (var q in db.Questions)
            {
                model.Questions.Add(new QuestionDisplay() { qID = q.Id, Text = q.Text, Answers = q.Answers.ToList(), SelectedAnswer = db.Responses.Single(r=>r.UserId == Current.UserID.Value && r.QuestionId==q.Id).AnswerId });
            }
            model.AlreadySubmitted = true;
            model.FormCloses = DateTime.Parse(System.Configuration.ConfigurationManager.AppSettings["FormDeadline"]);
            return View("Index", model);
        }

        [Url("results")]
        [Authorize]
        public virtual ActionResult Results()
        {
            ViewBag.curPage = "Results";

            if (System.Configuration.ConfigurationManager.AppSettings["ResultsOpen"] != "true")
            {
                return View("ResultsComingSoon");
            }

            var db = Current.DB;
            var matches = db.Matches.Where(m => m.RequestUser == Current.UserID.Value);
            if (!matches.Any()) // no rows returned
            {
                // Must not have submitted the form :(
                return RedirectToAction("Index"); // this will show explanation that did not submit form on time
            }

            // matchedsex=false is male, matchedsex=true is female
            var allSchoolMales = db.Matches.Where(m => m.MatchedSex == false).OrderByDescending(m=>m.CompatibilityIndex).Take(5).ToList();
            foreach (var i in allSchoolMales)
            {
                i.FillProperties();
            }
            var allSchoolFemales = db.Matches.Where(m => m.MatchedSex == true).OrderByDescending(m => m.CompatibilityIndex).Take(5).ToList();
            foreach (var i in allSchoolFemales)
            {
                i.FillProperties();
            }
            var yourGradeMales = db.Matches.Where(m => m.MatchedSex == false && m.AreSameGrade == true).OrderByDescending(m => m.CompatibilityIndex).Take(5).ToList();
            foreach (var i in yourGradeMales)
            {
                i.FillProperties();
            }
            var yourGradeFemales = db.Matches.Where(m => m.MatchedSex == true && m.AreSameGrade == true).OrderByDescending(m => m.CompatibilityIndex).Take(5).ToList();
            foreach (var i in yourGradeFemales)
            {
                i.FillProperties();
            }
            var nemesis = db.Matches.OrderBy(m => m.CompatibilityIndex).First(); // ascending order

            var model = new ResultsViewModel()
            {
                AllSchoolFemales = allSchoolFemales,
                AllSchoolMales = allSchoolMales,
                YourGradeFemales = yourGradeFemales,
                YourGradeMales = yourGradeMales,
                Nemesis = nemesis
            };

            return View(model);
        }

        [Url("matches/make")]
        public virtual ActionResult ComputeMatches()
        {
#if DEBUG
            // do everything synchronously - warning: timeouts!
            Matcher m = new Matcher(Current.DB);
            m.computeMatches();
            return Content("Done.");
            
#else
            // launch async task
            try
            {
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    Matcher m = new Matcher(Current.DB);
                    m.computeMatches();
                });
                return Content("Started.");
            }
            catch (Exception ex)
            {
                return Content("Error " + ex.Message);
            }
#endif
        }

        #region Administrative Tasks

        [Url("reset")]
        public virtual ActionResult Reset()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["AllowResetOrElevate"] != "true")
            {
                return RedirectToAction("NotFound", "Error");
            }
            System.Web.Security.Membership.DeleteUser("zaslavskym");
            return Content("Reset zaslavskym");
        }

        [Url("resetresp/{name}")]
        public virtual ActionResult ResetHalf(string name)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["AllowResetOrElevate"] != "true")
            {
                return RedirectToAction("NotFound", "Error");
            }
            var db = Current.DB;
            db.Matches.DeleteAllOnSubmit(db.Matches.Where(p => p.aspnet_User.UserName == name));
            db.Matches.DeleteAllOnSubmit(db.Matches.Where(p => p.aspnet_User1.UserName == name));
            db.Responses.DeleteAllOnSubmit(db.Responses.Where(p => p.aspnet_User.UserName == name));
            return Content("Reset responses " + name);
        }

        [Url("reset/all")]
        public virtual ActionResult ResetAll()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["AllowResetOrElevate"] != "true")
            {
                return RedirectToAction("NotFound", "Error");
            }

            new AccountController().FormsAuth.SignOut(); // otherwise auth cookie will persist if we're logged in and then we won't have a userID but Request.IsAuthenticated will still be true.

            var db = Current.DB;
            db.Matches.DeleteAllOnSubmit(db.Matches); // clear all rows
            db.SubmitChanges();

            db.Responses.DeleteAllOnSubmit(db.Responses);
            db.SubmitChanges();

            db.UserOpenIds.DeleteAllOnSubmit(db.UserOpenIds);
            db.SubmitChanges();

            foreach (string role in Roles.GetAllRoles())
            {
                Roles.RemoveUsersFromRole(Roles.GetUsersInRole(role), role);
                Roles.DeleteRole(role);
            }

            foreach (MembershipUser u in Membership.GetAllUsers())
            {
                Membership.DeleteUser(u.UserName, true);
            }

            Roles.CreateRole("Administrator");

            return Content("Done");
        }

        [Url("reset/admin")]
        public virtual ActionResult ElevateMZ()
        {
            if (System.Configuration.ConfigurationManager.AppSettings["AllowResetOrElevate"] != "true")
            {
                return RedirectToAction("NotFound", "Error");
            }
            Roles.AddUserToRole("zaslavskym", "Administrator");
            return Content("Done.");
        }

        #endregion
    }
}
