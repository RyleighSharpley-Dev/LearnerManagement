using LearniVerseNew.Models;
using LearniVerseNew.Models.ApplicationModels.Meal_Planner_Models;
using LearniVerseNew.Models.ApplicationModels.ViewModels;
using LearniVerseNew.Models.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LearniVerseNew.Controllers
{
    public class FoodSearchController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private readonly FatsecretHelper _fatsecretHelper;

        public FoodSearchController()
        {
            var httpClient = new HttpClient();
            _fatsecretHelper = new FatsecretHelper(httpClient);
        }

        // Constructor for dependency injection (if you use a DI library)
        public FoodSearchController(FatsecretHelper fatsecretHelper)
        {
            _fatsecretHelper = fatsecretHelper;
        }

        // GET: FoodSearch
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = false, message = "Query cannot be empty" });
            }

            var foodResponse = await _fatsecretHelper.SearchFoodsAsync(query);

            // Directly pass the deserialized object to the view
            return Json(new { success = true, data = foodResponse });
        }



        public async Task<ActionResult> Dashboard(MealPlannerViewModel model)
        {
            string Id = User.Identity.GetUserId();
            var student = await db.Students.FindAsync(Id);

            var foodRecords = db.FoodRecords.Where(fr => fr.StudentID == Id);


            if (!foodRecords.Any()) 
            {
                ViewBag.NoRecords = "Hmm... Looks like you have not recorded any meals. You must be starving! Lets Record a Meal...";
            }

            var todaysRecord = await foodRecords.OrderByDescending(s => s.DateRecorded).FirstAsync();


            if (student == null) 
            {
                return HttpNotFound();
            }

            model.Student = student;
            model.FoodRecords = foodRecords.ToList();
            model.TodaysRecord = todaysRecord;

            return View(model);
        }

        public async Task<ActionResult> RecordMeal(FoodRecord record)
        {
            string Id = User.Identity?.GetUserId();

            DateTime today = DateTime.Today;

            var student = await db.Students.FindAsync(Id);

            if (student == null) 
            {
                return HttpNotFound();
            }


            var todaysRecord = await db.FoodRecords
                           .Where(fr => fr.StudentID == Id && fr.DateRecorded == today)
                           .FirstOrDefaultAsync();

            if (todaysRecord == null) 
            {
                var newRecord = new FoodRecord
                {
                    FoodRecordID = Guid.NewGuid(),
                    DateRecorded = DateTime.Today,
                    StudentID = Id,
                    Student = student,
                    Meals = new List<Meal>()
                };

                db.FoodRecords.Add(newRecord);

                await db.SaveChangesAsync();

                record = newRecord;

                return View(record);
            }

            return View(todaysRecord);


        }

    }
}