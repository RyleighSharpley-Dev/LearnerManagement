using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LearniVerseNew.Models;
using LearniVerseNew.Models.ApplicationModels.Gym_Models;

namespace LearniVerseNew.Controllers
{
    public class BodyComposistionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BodyComposistions
        public ActionResult Index()
        {
            var bodyComposistions = db.BodyComposistions.Include(b => b.Student);
            return View(bodyComposistions.ToList());
        }

        // GET: BodyComposistions/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BodyComposistion bodyComposistion = db.BodyComposistions.Find(id);
            if (bodyComposistion == null)
            {
                return HttpNotFound();
            }
            return View(bodyComposistion);
        }

        // GET: BodyComposistions/Create
        public ActionResult Create()
        {
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName");
            return View();
        }

        // POST: BodyComposistions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BodyCompositionID,DateRecorded,Height,Weight,BMI,Status,BMR,BodyFatPercentage,LeanMuscleMass,StudentID")] BodyComposistion bodyComposistion)
        {
            if (ModelState.IsValid)
            {
                bodyComposistion.BodyCompositionID = Guid.NewGuid();
                db.BodyComposistions.Add(bodyComposistion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", bodyComposistion.StudentID);
            return View(bodyComposistion);
        }

        // GET: BodyComposistions/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BodyComposistion bodyComposistion = db.BodyComposistions.Find(id);
            if (bodyComposistion == null)
            {
                return HttpNotFound();
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", bodyComposistion.StudentID);
            return View(bodyComposistion);
        }

        // POST: BodyComposistions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BodyCompositionID,DateRecorded,Height,Weight,BMI,Status,BMR,BodyFatPercentage,LeanMuscleMass,StudentID")] BodyComposistion bodyComposistion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bodyComposistion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", bodyComposistion.StudentID);
            return View(bodyComposistion);
        }

        // GET: BodyComposistions/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BodyComposistion bodyComposistion = db.BodyComposistions.Find(id);
            if (bodyComposistion == null)
            {
                return HttpNotFound();
            }
            return View(bodyComposistion);
        }

        // POST: BodyComposistions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            BodyComposistion bodyComposistion = db.BodyComposistions.Find(id);
            db.BodyComposistions.Remove(bodyComposistion);
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
