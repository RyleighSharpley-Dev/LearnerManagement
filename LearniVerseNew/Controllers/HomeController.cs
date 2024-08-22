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

            var membership = student.Memberships.OrderByDescending(s => s.MembershipStart).First();

            if (membership != null && membership.IsActive) 
            {
               return RedirectToAction("MyBody", "BodyComposistions");
            }                                     

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