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
using LearniVerseNew.Models.ApplicationModels.ViewModels;
using Newtonsoft.Json;

namespace LearniVerseNew.Controllers
{
    public class StudentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "User")]
        public ActionResult Home(Student student, string id)
        {
            id = Session["UserId"].ToString();

            student = db.Students.Include(s => s.Enrollments.Select(e => e.Courses))
                                  .Include(f => f.Faculty)
                                  .Include(q => q.Qualification)
                                  .Include(q => q.QuizAttempts.Select(a => a.Quiz))
                                  .Include(sc => sc.StudySessions)
                                  .FirstOrDefault(s => s.StudentID == id);

            if (student != null)
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                // Group quiz attempts by course and calculate the highest mark for each quiz
                var courseHighestMarks = student.QuizAttempts
                    .GroupBy(a => a.Quiz.CourseID)
                    .Select(group => new
                    {
                        CourseID = group.Key,
                        HighestMark = group.Max(a => a.MarkObtained)
                    })
                    .ToList();

                // Create new lists to store flattened TaskItems
                var studySessionTaskItems = new List<TaskItem>();
                foreach (var session in student.StudySessions)
                {
                    studySessionTaskItems.AddRange(session.TaskItems);
                }

                ViewBag.CourseHighestMarks = courseHighestMarks;
                ViewBag.StudySessionTaskItems = Newtonsoft.Json.JsonConvert.SerializeObject(studySessionTaskItems, settings);

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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Index(string searchName)
        {
            IQueryable<Student> students = db.Students;

            if (!string.IsNullOrEmpty(searchName))
            {
                students = students.Where(s => s.StudentFirstName.Contains(searchName) || s.StudentLastName.Contains(searchName));
            }


            return View(await students.ToListAsync());
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
