using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LearniVerseNew.Models;
using LearniVerseNew.Models.ApplicationModels;
using LearniVerseNew.Models.ApplicationModels.Gym_Models;
using Microsoft.AspNet.Identity;

namespace LearniVerseNew.Controllers
{
    public class MembershipsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Memberships
        public ActionResult Index()
        {
            var memberships = db.Memberships.Include(m => m.Student);
            return View(memberships.ToList());
        }

        public ActionResult Plans()
        {
            var basic = db.Plans.FirstOrDefault(p=>p.PlanName == "Basic");
            var premium = db.Plans.FirstOrDefault(p=>p.PlanName == "Premium");

            ViewBag.BasicPlan = basic;
            ViewBag.PremiumPlan = premium;
            return View();
        }


        // GET: Memberships/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Membership membership = db.Memberships.Find(id);
            if (membership == null)
            {
                return HttpNotFound();
            }
            return View(membership);
        }

        // GET: Memberships/Create
        public ActionResult Create(string tier)
        {
            string id = User.Identity.GetUserId();
            var plan = db.Plans.FirstOrDefault(p => p.PlanName == tier);
            var student = db.Students.FirstOrDefault(s => s.StudentID == id );

            if (plan == null || student == null)
            {
                return HttpNotFound();
            }

            var membership = new Membership
            {
                MembershipTier = plan.PlanName,
                MembershipDuration = plan.PlanDuration,
                MembershipPrice = plan.PlanCost,
                StudentID = student.StudentID,
                Student = student,
                MembershipStart = DateTime.Now,
                MembershipEnd = DateTime.Now.AddMonths(plan.PlanDuration)
            };

            return View(membership);
        }

        [HttpPost]
        public ActionResult Subscribe()
        {

        }



        // POST: Memberships/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MembershipID,MembershipTier,MembershipStart,MembershipEnd,MembershipDuration,MembershipPrice,HasPaid,IsActive,StudentID")] Membership membership)
        {
            if (ModelState.IsValid)
            {
                membership.MembershipID = Guid.NewGuid();
                db.Memberships.Add(membership);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", membership.StudentID);
            return View(membership);
        }

        // GET: Memberships/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Membership membership = db.Memberships.Find(id);
            if (membership == null)
            {
                return HttpNotFound();
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", membership.StudentID);
            return View(membership);
        }

        // POST: Memberships/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MembershipID,MembershipTier,MembershipStart,MembershipEnd,MembershipDuration,MembershipPrice,HasPaid,IsActive,StudentID")] Membership membership)
        {
            if (ModelState.IsValid)
            {
                db.Entry(membership).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", membership.StudentID);
            return View(membership);
        }

        // GET: Memberships/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Membership membership = db.Memberships.Find(id);
            if (membership == null)
            {
                return HttpNotFound();
            }
            return View(membership);
        }

        // POST: Memberships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Membership membership = db.Memberships.Find(id);
            db.Memberships.Remove(membership);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
