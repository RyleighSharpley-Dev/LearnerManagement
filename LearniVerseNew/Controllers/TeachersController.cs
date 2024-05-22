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
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using LearniVerseNew.Models.ApplicationModels.ViewModels;
using Newtonsoft.Json;

namespace LearniVerseNew.Controllers
{
    public class TeachersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public TeachersController()
        {

        }
        public TeachersController(ApplicationUserManager userManager, RoleManager<IdentityRole> roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;

        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<RoleManager<IdentityRole>>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        // GET: Teachers
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Index(string searchTeacherName, string filterByFaculty)
        {
            var teachers = db.Teachers.AsQueryable();

            // Apply filter based on search criteria
            if (!string.IsNullOrEmpty(searchTeacherName))
            {
                teachers = teachers.Where(t => t.TeacherFirstName.Contains(searchTeacherName) || t.TeacherLastName.Contains(searchTeacherName));
            }

            if (!string.IsNullOrEmpty(filterByFaculty) && filterByFaculty != "All Faculties")
            {
                teachers = teachers.Where(t => t.Faculty.FacultyName == filterByFaculty);
            }

            // Load faculties for the dropdown list
            ViewBag.Faculties = new SelectList(db.Faculties, "FacultyName", "FacultyName");

            return View(await teachers.ToListAsync());
        }

        [Authorize(Roles = "Teacher")]
        public  ActionResult Home(Teacher teacher, string id)
        {
            id = Session["UserId"].ToString();

            teacher = db.Teachers.Include(t => t.Courses)
                                 .Include(x => x.Faculty)
                                 .FirstOrDefault(s => s.TeacherID == id);

            var courses = db.Courses.ToList();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var jsonCourses = JsonConvert.SerializeObject(courses, settings);

            if (teacher != null)
            {
                ViewBag.AllCourses = jsonCourses;
                return View(teacher);
            }
            return RedirectToAction("Account", "Login");
        }

        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> MyCourses(string id)
        {
            id = User.Identity.Name;

            // Retrieve the student from the database
            var teacher = await db.Teachers.FirstOrDefaultAsync(t => t.TeacherEmail == id);

            if (teacher != null)
            {
                // If the student is found, return the view with the student and their enrolled courses
                return View(teacher);
            }

            return View("Error");

        }



        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        // GET: Teachers/Create
        public ActionResult Create()
        {
            ViewBag.FacultyID = new SelectList(db.Faculties, "FacultyID", "FacultyName");
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([Bind(Include = "TeacherFirstName,TeacherLastName,TeacherEmail,TeacherPhoneNumber,Gender,DOB,Qualification, FacultyID,Password,ConfirmPassword")] RegisterTeacherViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.TeacherEmail,
                    Email = model.TeacherEmail
                    // You can set additional properties here as needed
                };

                // Add the user to the AspNetUsers table
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Teacher");
                    var userId = user.Id;

                    Teacher _teacher = new Teacher();

                    _teacher.TeacherID = userId;
                    _teacher.TeacherFirstName = model.TeacherFirstName;
                    _teacher.TeacherLastName = model.TeacherLastName;
                    _teacher.TeacherEmail = model.TeacherEmail;
                    _teacher.TeacherPhoneNumber = model.TeacherPhoneNumber;
                    _teacher.DOB = model.DOB;
                    _teacher.Gender = model.Gender;
                    _teacher.Qualification = model.Qualification;
                    _teacher.FacultyID = model.FacultyID;

                    db.Teachers.Add(_teacher);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

            }
            ViewBag.FacultyID = new SelectList(db.Faculties, "FacultyID", "FacultyName");
            return View(model);
        }

        // GET: Teachers/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            ViewBag.FacultyID = new SelectList(db.Faculties, "FacultyID", "FacultyName", teacher.FacultyID);
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TeacherID,TeacherFirstName,TeacherLastName,TeacherEmail,TeacherPhoneNumber,Gender,DOB,Qualification,FacultyID")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                db.Entry(teacher).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FacultyID = new SelectList(db.Faculties, "FacultyID", "FacultyName", teacher.FacultyID);
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Teacher teacher = db.Teachers.Find(id);
            db.Teachers.Remove(teacher);
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
