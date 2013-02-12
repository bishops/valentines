using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RiaLibrary.Web;
using valentines.Models;
using valentines.ViewModels;

namespace valentines.Controllers
{
    public partial class HomeController : Controller
    {
        [Url("reset")]
        public virtual ActionResult Reset()
        {
            System.Web.Security.Membership.DeleteUser("zaslavskym");
            return Content("Reset zaslavskym");
        }
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
            ViewBag.formCloses = deadline;
            
            var model = new SubmitViewModel();
            foreach (var q in db.Questions)
            {
                model.Questions.Add(new QuestionDisplay() { qID = q.Id, Text = q.Text, Answers = q.Answers.ToList()});
            }
            ViewBag.alreadySubmitted = false;
            return View(model);
        }

        [Url("")]
        [HttpPost]
        [Authorize]
        [ValidateInput(true)]
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

            var model = new SubmitViewModel();
            foreach (var q in db.Questions)
            {
                model.Questions.Add(new QuestionDisplay() { qID = q.Id, Text = q.Text, Answers = q.Answers.ToList(), SelectedAnswer = db.Responses.Single(r=>r.UserId == Current.UserID.Value && r.QuestionId==q.Id).AnswerId });
            }
            ViewBag.alreadySubmitted = true;
            return View("Index", model);
        }

        [Url("results")]
        public virtual ActionResult Results()
        {
            ViewBag.curPage = "Results";
            return View();
        }

        [Url("matches/make")]
        public virtual ActionResult ComputeMatches()
        {
            Matcher m = new Matcher(Current.DB);
            m.computeMatches();
            return Content("Done.");
        }
    }
}
