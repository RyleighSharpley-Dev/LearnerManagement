using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LearniVerseNew.Models;
using LearniVerseNew.Models.ApplicationModels;

namespace LearniVerseNew.Controllers
{
    public class StudentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "User")]
        public ActionResult Home(Student student, string id)
        {
            id = User.Identity.Name;

            student =  db.Students.Include(s => s.Enrollments.Select(e => e.Courses)).FirstOrDefault(s => s.StudentEmail == id);

            if (student != null)
            {
                return View(student);
            }
            return RedirectToAction("Account", "Login");
        }

        [Authorize(Roles = "User")]
        public ActionResult MyCourses(string id)
        {
            id = User.Identity.Name;

            var student = db.Students.Include(s => s.Enrollments.Select(e => e.Courses))
                                            .FirstOrDefault(s => s.StudentEmail == id);

            if (student != null)
            {
                
                return View(student);
            }

            return View("Error");

        }

        // GET: Students
        public ActionResult Index()
        {
            var students = db.Students.Include(s => s.Faculty).Include(s => s.Qualification);
            return View(students.ToList());
        }

        // GET: Students/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacultyID = new SelectList(db.Faculties, "FacultyID", "FacultyName", student.FacultyID);
            ViewBag.QualificationID = new SelectList(db.Qualifications, "QualificationID", "Name", student.QualificationID);
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "StudentID,StudentFirstName,StudentLastName,StudentEmail,PhoneNumber,Gender,DOB,FacultyID,QualificationID")] Student student)
        {
            if (ModelState.IsValid)
            {
                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyID = new SelectList(db.Faculties, "FacultyID", "FacultyName", student.FacultyID);
            ViewBag.QualificationID = new SelectList(db.Qualifications, "QualificationID", "Name", student.QualificationID);
            return View(student);
        }

        // GET: Students/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Student student = db.Students.Find(id);
            db.Students.Remove(student);
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
