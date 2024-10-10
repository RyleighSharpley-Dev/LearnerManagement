using System;
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
using QRCoder;
using System.Drawing;
using LearniVerseNew.Models.Helpers;

namespace LearniVerseNew.Controllers
{
    public class TrackingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private readonly TrackingHelper trackingHelper;

        public TrackingController()
        {
            
            trackingHelper = new TrackingHelper();
        }

        public async Task<ActionResult> GenerateSendToWarehouseQRCode(Guid orderId)
        {
            var order = await db.Orders.FindAsync(orderId);
            if (order == null) return HttpNotFound();

            string qrCodeBase64 = await trackingHelper.GenerateSendToWarehouseQRCodeAsync(orderId.ToString());

            // Convert base64 string to byte array and return as a file (PNG image)
            byte[] qrCodeBytes = Convert.FromBase64String(qrCodeBase64);
            return File(qrCodeBytes, "image/png");
        }

        // Async method to generate QR code for "At Warehouse" stage
        public async Task<ActionResult> GenerateAtWarehouseQRCode(Guid orderId)
        {
            var order = await db.Orders.FindAsync(orderId);
            if (order == null) return HttpNotFound();

            string qrCodeBase64 = await trackingHelper.GenerateAtWarehouseQRCodeAsync(orderId.ToString());

            byte[] qrCodeBytes = Convert.FromBase64String(qrCodeBase64);
            return File(qrCodeBytes, "image/png");
        }

        // Async method to generate QR code for "Out for Delivery" stage
        public async Task<ActionResult> GenerateOutForDeliveryQRCode(Guid orderId)
        {
            var order = await db.Orders.FindAsync(orderId);
            if (order == null) return HttpNotFound();

            string qrCodeBase64 = await trackingHelper.GenerateOutForDeliveryQRCodeAsync(orderId.ToString());

            byte[] qrCodeBytes = Convert.FromBase64String(qrCodeBase64);
            return File(qrCodeBytes, "image/png");
        }

        // Async method to generate QR code for "Delivered" stage
        public async Task<ActionResult> GenerateDeliveredQRCode(Guid orderId)
        {
            var order = await db.Orders.FindAsync(orderId);
            if (order == null) return HttpNotFound();

            string qrCodeBase64 = await trackingHelper.GenerateDeliveredQRCodeAsync(orderId.ToString());

            byte[] qrCodeBytes = Convert.FromBase64String(qrCodeBase64);
            return File(qrCodeBytes, "image/png");
        }

        
        public async Task<ActionResult> UpdateOrderStatus(Guid orderId, string stage)
        {
            // Retrieve the order from the database
            var order = await db.Orders.Include(o => o.TrackingHistory)
                                        .FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order == null) return HttpNotFound();

            // Get the latest tracking stage
            var currentTracking = order.TrackingHistory.OrderByDescending(th => th.Timestamp).FirstOrDefault();
            string currentStage = currentTracking?.TrackingStage ?? "Received"; // Default to "Received" if no tracking history

            // Determine the next stage
            string nextStage = trackingHelper.GetNextStage(currentStage);

            if (nextStage == null)
            {
                TempData["Error"] = "Order is already at the final stage.";
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Order is already delivered.");
            }

            string user = User.IsInRole("WareHouseAdmin") ? "Warehouse Administrator"
                        : User.IsInRole("Admin") ? "Admin"
                        : "System";

            // Ensure the provided stage matches the expected next stage
            if (nextStage == stage)
            {
                // Add new tracking event with the next stage
                var trackingEvent = new OrderTrackingHistory
                {
                    TrackingID = Guid.NewGuid(),
                    OrderID = order.OrderID,
                    Timestamp = DateTime.Now,
                    UpdatedBy = user,
                    TrackingStage = nextStage
                };

                order.TrackingHistory.Add(trackingEvent);

                // Update the order status to the new stage
                order.Status = nextStage;

                // Save changes asynchronously
                await db.SaveChangesAsync();

                TempData["Message"] = $"Order has been updated to '{nextStage}' stage.";

                // Send an email to notify the user (assuming emailHelper is configured)

                return View("OrderUpdated", order);
            }
            else
            {
                TempData["Error"] = "Invalid or out-of-sequence tracking stage.";
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid tracking stage");
            }
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
