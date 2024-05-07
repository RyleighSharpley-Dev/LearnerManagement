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
using LearniVerseNew.Models.ApplicationModels.ViewModels;
using Newtonsoft.Json;

namespace LearniVerseNew.Controllers
{
    public class QuizController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Quiz(Guid QuizID)
        {
            var quiz = db.Quizzes.Include(q => q.Questions).FirstOrDefault(q => q.QuizID == QuizID);
            var questions = quiz.Questions.ToList();

            var viewModel = new QuizViewModel
            {
                Quiz = quiz,
                Questions = questions,
                SubmittedAnswers = new Dictionary<Guid, string>(),
                CurrentQuestionIndex = 0
            };

            TempData["QuizID"] = quiz.QuizID;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SubmitQuiz(string submittedAnswersJson)
        {
            try
            {
                var submittedAnswers = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(submittedAnswersJson);

                // Store the submitted answers dictionary in the session or perform other processing
                Session["SubmittedAnswers"] = submittedAnswers;
                var quizId = (Guid)TempData["QuizID"];
                // Optionally, you can return a response indicating success
                return RedirectToAction("Review", new { id = quizId });

            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return Json(new { success = false, message = "An error occurred while processing the quiz submission." });
            }
        }


        public ActionResult Review(Guid id)
        {
            var quiz = db.Quizzes.Find(id);

            // Retrieve submitted answers from session
            var submittedAnswers = Session["SubmittedAnswers"] as Dictionary<Guid, string>;

            ViewBag.Answers = submittedAnswers;
            

            return View(quiz);

        }

        public ActionResult View(Guid QuizID)
        {
            // Retrieve all questions for the specified quiz
            var questions = db.Questions.Where(q => q.QuizID == QuizID).ToList();

            return View(questions);
        }

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
                question.QuestionID = Guid.NewGuid();
                question.QuizID = QuizID;
                db.Questions.Add(question);
            }

            // Save changes to the database
            db.SaveChanges();

            // Redirect back to the quiz details page
            return RedirectToAction("Details", new { id = QuizID });
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
