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

namespace LearniVerseNew.Controllers
{
    public class QuizController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult AddQuestions(Guid QuizID)
        {
            // Fetch the quiz based on the provided quizId
            var quiz = db.Quizzes.Find(QuizID);

            // Pass the quiz to the view
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddQuestions(Guid QuizID, List<Question> questions)
        {
            // Fetch the quiz based on the provided quizId
            var quiz = db.Quizzes.Find(QuizID);

            // Associate each question with the quiz
            foreach (var question in questions)
            {
                question.QuizID = QuizID;
                // You may need additional logic to validate and save the questions
                db.Questions.Add(question);
            }

            // Save changes to the database
            db.SaveChanges();

            // Redirect back to the quiz details page
            return RedirectToAction("Details", new { id = QuizID });
        }

        public ActionResult Quiz(Guid QuizID)
        {
            
            var quiz = db.Quizzes.Include(q => q.Questions)
                .FirstOrDefault(q => q.QuizID == QuizID);      

            return View(quiz);
        }


        // GET: Quiz
        public ActionResult Index()
        {
            return View(db.Quizzes.ToList());
        }

        // GET: Quiz/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quizzes.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            return View(quiz);
        }

        // GET: Quiz/Create
        [Authorize(Roles = "Teacher")]
        public ActionResult Create()
        {
            string currentTeacherId = User.Identity.Name;

            
            var coursesTaughtByTeacher = db.Courses.Where(c => c.Teacher.TeacherEmail == currentTeacherId).ToList();
            ViewBag.Courses = coursesTaughtByTeacher;

            return View();
        }

        // POST: Quiz/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "QuizID,QuizDescription,QuizMaxMark,QuizDate,QuizStart,QuizEnd,MaxAttempts,Status,CourseID")] Quiz quiz)
        {
            string email = User.Identity.Name;

            var teacher = db.Teachers.FirstOrDefault(t => t.TeacherEmail == email);

            quiz.DateCreated = DateTime.Now;
            quiz.TeacherID = teacher.TeacherID;

            if (ModelState.IsValid)
            {
                quiz.QuizID = Guid.NewGuid();
                db.Quizzes.Add(quiz);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            string currentTeacherId = User.Identity.Name; 

            
            var coursesTaughtByTeacher = db.Courses.Where(c => c.Teacher.TeacherEmail == currentTeacherId).ToList();
            ViewBag.Courses = coursesTaughtByTeacher;

            return View(quiz);
        }

        // GET: Quiz/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quizzes.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            return View(quiz);
        }

        // POST: Quiz/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QuizID,QuizDescription,QuizMaxMark,QuizDate,QuizStart,QuizEnd,DateCreated,MaxAttempts,Status,TeacherID,CourseID")] Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                db.Entry(quiz).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(quiz);
        }

        // GET: Quiz/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quizzes.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            return View(quiz);
        }

        // POST: Quiz/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Quiz quiz = db.Quizzes.Find(id);
            db.Quizzes.Remove(quiz);
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
