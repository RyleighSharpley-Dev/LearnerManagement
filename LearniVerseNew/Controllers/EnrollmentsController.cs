using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LearniVerseNew.Models;
using LearniVerseNew.Models.ApplicationModels;
using LearniVerseNew.Models.ApplicationModels.ViewModels;
using LearniVerseNew.Models.Helpers;

namespace LearniVerseNew.Controllers
{
    public class EnrollmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "User")]
        public ActionResult MyEnrollments()
        {
            string studentId = User.Identity.Name;

            var enrollments = db.Enrollments
                                .Where(e => e.Student.StudentEmail == studentId)
                                .ToList();

            return View(enrollments);
        }


        [Authorize(Roles = "User")]
        public ActionResult SelectFaculty(string studentId)
        {
            studentId = User.Identity.Name;


            var student = db.Students.FirstOrDefault(s => s.StudentEmail == studentId);

            if (student == null)
            {
                return HttpNotFound();
            }


            var availableFaculties = db.Faculties.ToList();

            var viewModel = new EnrolViewModel
            {
                StudentID = student.StudentID,
                StudentFirstName = student.StudentFirstName,
                StudentLastName = student.StudentLastName,
                AvailableFaculties = availableFaculties
            };

            
            return View(viewModel);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectFaculty(EnrolViewModel viewModel)
        {

            if (ModelState.IsValid)
            {

                var selectedFaculty = db.Faculties.FirstOrDefault(f => f.FacultyID == viewModel.SelectedFaculty);

                if (selectedFaculty != null)
                {

                    var Qualifications = db.Qualifications.Where(q => q.FacultyID == selectedFaculty.FacultyID).ToList();

                    viewModel.AvailableQualifications = Qualifications;

                    TempData["SelectedFaculty"] = viewModel.SelectedFaculty;
                    return View("SelectQualification", viewModel);
                }
                else
                {

                    ModelState.AddModelError(string.Empty, "Invalid faculty selection.");
                    return View(viewModel);
                }
            }

            return View(viewModel);
        }


        [Authorize(Roles = "User")]
        public ActionResult SelectQualification()
        {
            var selectedFacultyId = TempData["SelectedFaculty"].ToString();

            if (selectedFacultyId == null)
            {
                return RedirectToAction("SelectFaculty");
            }


            var availableQualifications = db.Qualifications.Where(q => q.FacultyID == selectedFacultyId).ToList();

            if (availableQualifications.Count == 0)
            {

                ModelState.AddModelError(string.Empty, "No qualifications found for the selected faculty.");
                return View("SelectFaculty");
            }


            return View(availableQualifications);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectQualification(EnrolViewModel viewModel)
        {
            if (ModelState.IsValid)
            {

                TempData["SelectedQualificationID"] = viewModel.SelectedQualificationID;


                return RedirectToAction("SelectCourses");
            }


            return View(viewModel);
        }


        [Authorize(Roles = "User")]
        public ActionResult SelectCourses()
        {
            string QualificationID = TempData["SelectedQualificationID"].ToString();

            var availableCourses = db.Courses.Where(c => c.QualificationID == QualificationID).ToList();

            var viewModel = new EnrolViewModel
            {
                AvailableCourses = availableCourses
            };

            TempData["QualificationID"] = QualificationID;

            return View(viewModel);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectCourses(EnrolViewModel viewModel)
        {
            if (ModelState.IsValid)
            {

                var selectedCourses = db.Courses.Where(c => viewModel.SelectedCourseIDs.Contains(c.CourseID)).ToList();

                TempData["SelectedCourseIDs"] = viewModel.SelectedCourseIDs;

                return View("Confirmation", selectedCourses);
            }


            return View(viewModel);
        }


        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirmation()
        {
            EmailHelper emailer = new EmailHelper();

            string selectedFaculty = TempData["SelectedFaculty"].ToString();
            string selectedQualification = TempData["QualificationID"].ToString();

            var selectedCourseIds = TempData["SelectedCourseIDs"] as IEnumerable<string>;
            if (selectedCourseIds == null)
            {
                return RedirectToAction("SelectFaculty");
            }

            var courseIdsList = selectedCourseIds.ToList();
            string studentId = User.Identity.Name;
            var student = db.Students.FirstOrDefault(s => s.StudentEmail == studentId);
            if (student == null)
            {
                return HttpNotFound();
            }
            // Create a new Enrollment for each selected course
            var enrollment = new Enrollment
            {
                EnrollmentID = Guid.NewGuid(),
                StudentID = student.StudentID,
                EnrollmentDate = DateTime.Now,
                IsApproved = false,
                HasPaid = false,
                Courses = new List<Course>() // Initialize the collection
            };

            foreach (var courseId in courseIdsList)
            {

                // Find the course by its ID
                var course = db.Courses.FirstOrDefault(c => c.CourseID == courseId);
                if (course != null)
                {
                    // Add the course to the Enrollment
                    enrollment.Courses.Add(course);
                }

                // Add the enrollment to the database
                db.Enrollments.Add(enrollment);
            }

            if (selectedFaculty != null && selectedQualification != null)
            {

                student.FacultyID = selectedFaculty;
                student.QualificationID = selectedQualification;

                var entry = db.Entry(student);
                if (entry.State == EntityState.Detached)
                {
                    // If the student is not tracked, attach it to the context
                    db.Students.Attach(student);
                    entry.State = EntityState.Modified;
                }

            }

            // Save changes to the database
            db.SaveChanges();

            // Send email notification
            emailer.SendEmailApprovalPending(student.StudentEmail, $"{student.StudentFirstName} {student.StudentLastName}");

            var paystackService = new PaystackHelper();
            var initializeResponse = paystackService.InitializeTransaction(studentId, Convert.ToInt32(1500 * 100), "https://bd4f-41-144-1-56.ngrok-free.app/Enrollments/ApplicationCallBack"); //

            if (!initializeResponse.Status)
            {
                ViewBag.ErrorMessage = initializeResponse.Message;
                return View("Error");
            }

            // Redirect to Paystack for payment
            return Redirect(initializeResponse.Data.AuthorizationUrl);

            //make the database update after the application fee is received

        }


        [Authorize(Roles = "User")]
        public ActionResult EnrollmentPending()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult PendingApplications()
        {
            var pendingEnrollments = db.Enrollments.Where(e => !e.IsApproved).ToList();
            return View(pendingEnrollments);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveEnrollment(Guid id)
        {
            EmailHelper mailer = new EmailHelper();
           
            var enrollment = db.Enrollments.Find(id);
            if (enrollment != null)
            {
                var student = db.Students.FirstOrDefault(s => s.StudentID == enrollment.StudentID);
                enrollment.IsApproved = true;
                db.SaveChanges();
                mailer.SendEmailApproved(student.StudentEmail, $"{student.StudentFirstName} {student.StudentLastName}");
                ViewData["SuccessMessage"] = $"{enrollment.Student.StudentFirstName} {enrollment.Student.StudentLastName}'s Enrollment application has been approved.";
            }
            return RedirectToAction("PendingApplications");
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(Guid enrollmentId)
        {
            string email = User.Identity.Name;
            var enrollment = db.Enrollments.Find(enrollmentId);
            if (enrollment == null)
            {
                return HttpNotFound();
            }

            decimal totalCost = enrollment.Courses.Sum(c => c.Price);

            var paystackService = new PaystackHelper();
            var initializeResponse = paystackService.InitializeTransaction(email, Convert.ToInt32(totalCost * 100), "https://bd4f-41-144-1-56.ngrok-free.app/Enrollments/PaystackCallback"); // add callback url

            if (!initializeResponse.Status)
            {
                ViewBag.ErrorMessage = initializeResponse.Message;
                return View("Error");
            }

            TempData["enrollmentId"] = enrollment.EnrollmentID;

            return Redirect(initializeResponse.Data.AuthorizationUrl);
        }

        public ActionResult ApplicationCallBack()
        {
            // Retrieve transaction reference from the request
            var reference = Request.Form["reference"];

            // Use Paystack API to verify the transaction
            var paystackService = new PaystackHelper(); // Assuming PaystackHelper is your service class
            var verifyResponse = paystackService.VerifyTransaction(reference);

            // Check if the payment was successful
            if (verifyResponse.Status)
            {
                // Payment was successful, redirect to EnrollmentPending
                return RedirectToAction("EnrollmentPending");
            }
            else
            {
                // Payment verification failed, handle accordingly
                ViewBag.ErrorMessage = verifyResponse.Message;
                return View("Error");
            }
        }

        public ActionResult PaystackCallback(string reference)
        {
            PdfHelper pdfHelper = new PdfHelper();
            var paystackService = new PaystackHelper();
            var verifyResponse = paystackService.VerifyTransaction(reference);
            Guid Id;

            string email = User.Identity.Name;

            Student student = db.Students.FirstOrDefault(s => s.StudentEmail == email);

            if (verifyResponse.Status)
            {
                if (TempData["enrollmentId"] != null && Guid.TryParse(TempData["enrollmentId"].ToString(), out Id))
                {


                    var enrollment = db.Enrollments.Find(Id);
                    if (enrollment != null)
                    {
                        decimal totalCost = verifyResponse.Data.Amount;
                        var paymentReference = Payment.GeneratePaymentReference(student.StudentID);

                        enrollment.HasPaid = true;

                        var payment = new Payment
                        {
                            StudentID = student.StudentID,
                            EnrollmentID = enrollment.EnrollmentID,
                            AmountPaid = (totalCost/100),
                            PaymentReference = paymentReference,
                            PaymentDate = DateTime.Now
                        };

                        db.Payments.Add(payment);
                        db.SaveChanges();

                        List<string> courseIds = enrollment.Courses.Select(c => c.CourseID).ToList();

                        // Generate and email invoice
                        PdfHelper.GenerateAndEmailInvoice(student, courseIds, (totalCost/100), paymentReference);

                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
                else
                {
                   
                    return HttpNotFound();
                }
            }
            else
            {
                // Payment verification failed, handle accordingly
                ViewBag.ErrorMessage = verifyResponse.Message;
                return View("Error");
            }

            return RedirectToAction("Home", "Students");
        }


        // GET: Enrollments
        public ActionResult Index()
        {
            var enrollments = db.Enrollments.Include(e => e.Student);
            return View(enrollments.ToList());
        }

        // GET: Enrollments/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // GET: Enrollments/Create
        public ActionResult Create()
        {
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EnrollmentID,StudentID,IsApproved,HasPaid,EnrollmentDate")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                enrollment.EnrollmentID = Guid.NewGuid();
                db.Enrollments.Add(enrollment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", enrollment.StudentID);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EnrollmentID,StudentID,IsApproved,HasPaid,EnrollmentDate")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(enrollment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Enrollment enrollment = db.Enrollments.Find(id);
            db.Enrollments.Remove(enrollment);
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
