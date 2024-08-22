using LearniVerseNew.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LearniVerseNew.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Gainz()
        {
            string id = User.Identity.GetUserId();

            var student = db.Students.Include(st => st.Memberships).FirstOrDefault(s => s.StudentID == id);

            var membership = student?.Memberships
                           .OrderByDescending(s => s.MembershipStart)
                           .FirstOrDefault();

            if (membership != null && membership.IsActive) 
            {
               return RedirectToAction("MembershipHome");
            }                                     

            return View();
        }

        public ActionResult MembershipHome()
        {
            string id = User.Identity.GetUserId();

            var student = db.Students.Include(st => st.Memberships)
                                     .Include(bd => bd.BodyComposistions)
                                     .Include(fr => fr.FoodRecords)
                                     .FirstOrDefault(s => s.StudentID == id);

            var membership = student.Memberships.OrderByDescending(s => s.MembershipStart).First();
            var todaysFood = student.FoodRecords.OrderByDescending(s => s.DateRecorded).First();
            var latestBody = student.BodyComposistions.OrderByDescending(s => s.DateRecorded).First();

            ViewBag.Student = student;
            ViewBag.Membership = membership;
            ViewBag.TodaysFood = todaysFood;
            ViewBag.LatestBody = latestBody;

            double totalCalories = 0;
            if (todaysFood != null)
            {
                totalCalories = todaysFood.Meals.Sum(m => m.FoodItems.Sum(f => f.CaloriesPerServing));
            }

            ViewBag.TotalCalories = totalCalories;

            return View();
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}