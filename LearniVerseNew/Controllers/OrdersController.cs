﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LearniVerseNew.Models;
using LearniVerseNew.Models.ApplicationModels.Store_Models;
using Microsoft.AspNet.Identity;
using LearniVerseNew.Models.Helpers;
using QRCoder;
using System.Drawing;
using System.IO;

namespace LearniVerseNew.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private PdfHelper pdfHelper = new PdfHelper();
        private EmailHelper emailHelper = new EmailHelper();

        public ActionResult OrderHistory(string sortOrder)
        {
            // Get the current user's ID
            string userId = User.Identity.GetUserId();

            // Set the sort order (default is ascending)
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";

            using (var db = new ApplicationDbContext())
            {
                // Fetch the user's orders
                var orders = from o in db.Orders
                             where o.StudentID == userId
                             select o;

                // Sort orders by date
                switch (sortOrder)
                {
                    case "date_desc":
                        orders = orders.OrderByDescending(o => o.DateOrdered);
                        break;
                    default:
                        orders = orders.OrderBy(o => o.DateOrdered);
                        break;
                }

                return View(orders.ToList());
            }
        }




        // GET: Orders
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string sortOrder, string searchString)
        {
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.NameSortParm = sortOrder == "name_asc" ? "name_desc" : "name_asc";

            var orders = from o in db.Orders.Include(o => o.Student)
                         select o;

            // Search
            if (!String.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.Student.StudentFirstName.Contains(searchString)
                                        || o.ShippingAddress.Contains(searchString)
                                        || o.City.Contains(searchString)
                                        || o.PostalCode.Contains(searchString));
            }

            // Sort
            switch (sortOrder)
            {
                case "date_desc":
                    orders = orders.OrderByDescending(o => o.DateOrdered);
                    break;
                case "name_asc":
                    orders = orders.OrderBy(o => o.Student.StudentFirstName);
                    break;
                case "name_desc":
                    orders = orders.OrderByDescending(o => o.Student.StudentFirstName);
                    break;
                default:
                    orders = orders.OrderBy(o => o.DateOrdered);
                    break;
            }

            return View(orders.ToList());
        }

        public async Task<ActionResult> OrderDetails(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        public async Task<ActionResult> EmailInvoice(Guid id)
        {
                var order = db.Orders
                              .Include(o => o.OrderItems.Select(oi => oi.Product))
                              .FirstOrDefault(o => o.OrderID == id);

                if (order == null || order.StudentID != User.Identity.GetUserId())
                {
                    return HttpNotFound(); 
                }

                byte[] pdfInvoice = pdfHelper.GeneratePdfInvoice(order);


                await emailHelper.SendInvoiceAsync(order.Student.StudentEmail, pdfInvoice, $"Invoice_{order.OrderID}.pdf");

                TempData["Message"] = "Invoice sent to your email address.";
                return RedirectToAction("OrderDetails", new { id = order.OrderID });
            
        }

        // GET: Orders/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([Bind(Include = "OrderID,DateOrdered,StudentID,TotalPrice,ShippingAddress,City,State,PostalCode,IsDelivered")] Order order)
        {
            if (ModelState.IsValid)
            {
                order.OrderID = Guid.NewGuid();
                db.Orders.Add(order);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", order.StudentID);
            return View(order);
        }

        // GET: Orders/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", order.StudentID);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "OrderID,DateOrdered,StudentID,TotalPrice,ShippingAddress,City,State,PostalCode,IsDelivered")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.StudentID = new SelectList(db.Students, "StudentID", "StudentFirstName", order.StudentID);
            return View(order);
        }

        // GET: Orders/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = await db.Orders.FindAsync(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Order order = await db.Orders.FindAsync(id);
            db.Orders.Remove(order);
            await db.SaveChangesAsync();
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
