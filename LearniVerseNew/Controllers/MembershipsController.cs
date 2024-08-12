﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using LearniVerseNew.Models;
using LearniVerseNew.Models.ApplicationModels;
using LearniVerseNew.Models.ApplicationModels.Gym_Models;
using LearniVerseNew.Models.Helpers;
using Microsoft.AspNet.Identity;
using Membership = LearniVerseNew.Models.ApplicationModels.Gym_Models.Membership;

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

        public ActionResult MyMemberships()
        {
             string id = User.Identity.GetUserId();

            var memberships = db.Memberships.Where(m => m.StudentID == id)
                .Include(p => p.Plan).ToList();

            return View(memberships); 
        }

        public ActionResult Plans()
        {
            var basic = db.Plans.FirstOrDefault(p => p.PlanName == "Basic");
            var premium = db.Plans.FirstOrDefault(p => p.PlanName == "Premium");

            ViewBag.BasicPlan = basic;
            ViewBag.PremiumPlan = premium;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Subscribe(string email, string planName)
        {
            string callbackUrl = "https://4636-41-144-66-74.ngrok-free.app/Memberships/SubscriptionCallBack"; //chamge in Prod

            var student = db.Students.FirstOrDefault(s => s.StudentEmail == email);
            if (student == null)
            {
                return HttpNotFound();
            }

            var plan = db.Plans.FirstOrDefault(s => s.PlanName == planName);

            if (plan == null)
            {
                return HttpNotFound();
            }

            PaystackHelper paystack  = new PaystackHelper();

           var transactionResponse = paystack.InitializeSubscriptionTransaction(student.StudentEmail, plan.PlanCode, callbackUrl);

            if (transactionResponse.Status)
            {
                // Redirect the user to Paystack payment page
                return Redirect(transactionResponse.Data.AuthorizationUrl);
            }
            else
            {
                // Handle error
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Transaction initialization failed.");
            }

        }


        public async Task<ActionResult> SubscriptionCallBack(string reference)
        {
            var paystackService = new PaystackHelper();
            var verifyResponse = paystackService.VerifyTransaction(reference);

            if (verifyResponse.Status)
            {
                string id = User.Identity.GetUserId();
                var student = await db.Students.FindAsync(id);

                if (student == null)
                {
                    // Handle case where student is not found
                    return View("Error", new { message = "Student not found." });
                }

                decimal amount = verifyResponse.Data.Amount / 100; // Convert from kobo to currency

                var plan = await db.Plans.FirstOrDefaultAsync(p => p.PlanCost == amount);

                if (plan == null)
                {
                    // Handle case where the plan is not found
                    return View("Error", new { message = "Plan not found." });
                }

                Guid membershipId = Guid.NewGuid();
                Guid paymentId = Guid.NewGuid();

                var membershipPayment = new MembershipPayment
                {
                    PaymentID = paymentId,
                    MembershipID = membershipId,
                    StudentID = student.StudentID,
                    Student = student,
                    Amount = amount,
                    CustomerCode = verifyResponse.Data.Customer.CustomerCode,
                    PlanCode = plan.PlanCode,
                    Status = verifyResponse.Data.Status,  // Ensure this matches your status expectations
                    TransactionDate = DateTime.UtcNow
                };

                var membership = new Membership
                {
                    MembershipID = membershipId,
                    MembershipTier = plan.PlanName,
                    MembershipDuration = plan.PlanDuration,
                    MembershipPrice = plan.PlanCost,
                    StudentID = student.StudentID,
                    Student = student,
                    IsActive = true,
                    HasPaid = true,
                    MembershipStart = DateTime.Now,
                    MembershipEnd = DateTime.Now.AddMonths(plan.PlanDuration),
                    PlanID = plan.PlanID,
                    Plan = plan,
                    PaymentID = paymentId,
                };

                db.Memberships.Add(membership);
                db.MembershipPayments.Add(membershipPayment);

                await db.SaveChangesAsync();

                return RedirectToAction("SubscriptionSuccess");
            }
            else
            {
                ViewBag.ErrorMessage = verifyResponse.Message;
                return View("Error");
            }
        }


        public ActionResult SubscriptionSuccess()
        {
            return View();
        }


        public async Task<ActionResult> RequestCancel(Guid Id)
        {

            var membership = await db.Memberships.FindAsync(Id);

            return View(membership); 
        }

        public async Task<ActionResult> ConfirmCancel(Guid Id)
        {

            var membership = await db.Memberships.FindAsync(Id);

            membership.CancelRequested = true;

            await db.SaveChangesAsync();

            //Add email

            return RedirectToAction("RequestConfirmed");
        }

        public ActionResult RequestConfirmed()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CancelRequests(string searchEmail)
        {
            var memberships = db.Memberships
                                .Include(s => s.Student)
                                .Include(m => m.MembershipPayments)
                                .Where(c => c.CancelRequested == true);

            if (!string.IsNullOrEmpty(searchEmail))
            {
                memberships = memberships.Where(m => m.Student.StudentEmail.Contains(searchEmail));
            }

            return View(memberships.ToList());
        }

        public ActionResult ApproveCancelRequest(Guid id)
        {
            //fix tomorrow

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
