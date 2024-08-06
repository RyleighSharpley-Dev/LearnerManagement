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
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace LearniVerseNew.Controllers
{
    public class QuizController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult TakeQuiz(Guid QuizID)
        {
            string studentId = Session["UserId"]?.ToString();
            if (string.IsNullOrEmpty(studentId))
            {
                return RedirectToAction("Login", "Account");
            }

            int attemptCount = db.QuizAttempts.Count(a => a.QuizID == QuizID && a.StudentID == studentId);

            var quiz = db.Quizzes.Find(QuizID);
            var currentTime = DateTime.Now;

            if (currentTime.Date > quiz.QuizDate.Date || (quiz.QuizStart > currentTime.TimeOfDay && quiz.QuizEnd < currentTime.TimeOfDay))
            {
                ViewBag.QuizAvailabilityMessage = "The quiz is currently not available.";
                ViewBag.CourseID = quiz.CourseID;
                return View("QuizNotAvailable");
            }

            ViewBag.UserAttempts = attemptCount;
            return View(quiz);
        }

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

                
                Session["SubmittedAnswers"] = submittedAnswers;

                var quizId = (Guid)TempData["QuizID"];

                Session["quizSubmitted"] = true;

                return RedirectToAction("Review", new { id = quizId });

            }
            catch (Exception ex)
            {
                
                return Json(new { success = false, message = "An error occurred while processing the quiz submission." });
            }
        }


        public ActionResult Review(Guid id)
        {
            var quiz = db.Quizzes.Find(id);

            var submittedAnswers = Session["SubmittedAnswers"] as Dictionary<Guid, string>;

            ViewBag.Answers = submittedAnswers;
            
            return View(quiz);

        }

        [HttpPost]
        public ActionResult MarkQuiz()
        {
            Session["quizSubmitted"] = false;
            string email = User.Identity.Name;
            var quizId = (Guid)TempData["QuizID"];
            var student = db.Students.FirstOrDefault(s => s.StudentEmail == email);
            int totalScore = 0;

            var quiz = db.Quizzes.Include(q=>q.Questions).
                FirstOrDefault(r => r.QuizID == quizId);

            var selectedAnswers = Session["SubmittedAnswers"] as Dictionary<Guid, string>;

            foreach (var question in quiz.Questions)
            {
                
                if (selectedAnswers.ContainsKey(question.QuestionID))
                {
                    string selectedAnswer = selectedAnswers[question.QuestionID];

                   
                    if (selectedAnswer == question.CorrectAnswer)
                    {
                        totalScore += question.Weighting;
                    }
                }
            }

            var attempt = new QuizAttempt()
            {
                QuizAttemptID = Guid.NewGuid(),
                QuizID = quizId,
                StudentID = student.StudentID,
                AttemptDate = DateTime.Now,
                MarkObtained = totalScore
            };

            db.QuizAttempts.Add(attempt);

            db.SaveChanges();

            return RedirectToAction("QuizSubmitted", "Quiz");

        }

        public ActionResult QuizSubmitted()
        {

            return View();
        }



        public ActionResult View(Guid QuizID)
        {
            
            var questions = db.Questions.Where(q => q.QuizID == QuizID).ToList();

            return View(questions);
        }

        public ActionResult AddQuestions(Guid QuizID)
        {
            
            var quiz = db.Quizzes.Find(QuizID);

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
        public ActionResult Create([Bind(Include = "QuizID,QuizDescription,QuizMaxMark,QuizDate,QuizStart,QuizEnd,MaxAttempts,Status,Duration,CourseID")] Quiz quiz)
        {
            string email = User.Identity.Name;

            var teacher = db.Teachers.FirstOrDefault(t => t.TeacherEmail == email);

            quiz.DateCreated = DateTime.Now;
            quiz.TeacherID = teacher.TeacherID;

            if (ModelState.IsValid)
            {
                quiz.QuizID = Guid.NewGuid();
                //quiz.Duration = quiz.Duration;
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
