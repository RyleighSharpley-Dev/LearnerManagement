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
using LearniVerseNew.Models.ApplicationModels.ViewModels;
using LearniVerseNew.Models.Helpers;
using LearniVerseNew.Models.ApplicationModels;
using System.IdentityModel;

namespace LearniVerseNew.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private BlobHelper blobHelper = new BlobHelper();

        // GET: Products
        public ActionResult Index(string productName, Guid? categoryId)
        {
            // Fetch all products
            var products = db.Products.Include(p => p.Category).AsQueryable();

            // Filter by product name if provided
            if (!string.IsNullOrEmpty(productName))
            {
                products = products.Where(p => p.ProductName.Contains(productName));
            }

            // Filter by category if provided
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId);
            }

            // Pass the filtered products to the view
            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName", categoryId); // Pre-select the chosen category
            ViewBag.ProductName = productName; // To retain the search value in the input field

            return View(products.ToList());
        }

        // GET: Products/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Retrieve the product image from Blob Storage
            try
            {
                byte[] photoBytes = await blobHelper.RetriveProductPhotoAsync(product.ImageName);
                if (photoBytes != null)
                {
                    ViewBag.PhotoData = Convert.ToBase64String(photoBytes);
                }
                else
                {
                    ViewBag.DefaultPhotoPath = "/images/default.jpg";
                }
            }
            catch (RequestFailedException)
            {
                ViewBag.DefaultPhotoPath = "/images/default.jpg";
            }

            return View(product);
        }

        // GET: Products/Create
        public ActionResult AddProduct()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddProduct(ProductCreateViewModel model, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    // Save image to blob storage and get the URI
                    (bool success, string name) = blobHelper.UploadProductBlob(ImageFile.FileName, ImageFile.InputStream);

                    if (success)
                    {
                        var product = new Product
                        {
                            ProductID = Guid.NewGuid(),
                            ProductName = model.ProductName,
                            Price = model.Price,
                            Description = model.Description,
                            QuantityInStock = model.QuantityInStock,
                            ImageName = name,
                            CategoryID = model.CategoryID // Assign selected category
                        };

                        db.Products.Add(product);
                        await db.SaveChangesAsync();

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Image upload failed.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please select an image file.");
                }
            }

            // Reload categories in case of model validation errors
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", model.CategoryID);
            return View(model);
        }

        // GET: Products/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            try
            {
                byte[] photoBytes = await blobHelper.RetriveProductPhotoAsync(product.ImageName);
                if (photoBytes != null)
                {
                    ViewBag.PhotoData = Convert.ToBase64String(photoBytes);
                }
                else
                {

                    ViewBag.DefaultPhotoPath = "/images/default.jpg";
                }
            }
            catch (RequestFailedException ex)
            {
                ViewBag.DefaultPhotoPath = "/images/default.jpg";
            }

            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Find the existing product in the database
                var existingProduct = await db.Products.FindAsync(id);

                string oldImageName = existingProduct.ImageName;

                if (existingProduct == null)
                {
                    return HttpNotFound();
                }

                // Update non-image fields
                existingProduct.ProductName = product.ProductName;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.QuantityInStock = product.QuantityInStock;

                // If an image file is uploaded, update the image
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                   blobHelper.DeleteProductBlob(oldImageName);

                    // Upload the new image to Azure Blob (assuming you have the UploadProductBlob method)
                    using (var stream = ImageFile.InputStream)
                    {
                        var uploadResult = blobHelper.UploadProductBlob(ImageFile.FileName, stream);

                        if (!uploadResult.Success)
                        {
                            // Handle failure to upload image (e.g., log it or show an error message)
                            ModelState.AddModelError("", uploadResult.name);
                            return View(product);
                        }

                        // Update the product's ImageName property with the new image URL or name
                        existingProduct.ImageName = ImageFile.FileName;
                    }
                }

                // Mark product as modified and save changes
                db.Entry(existingProduct).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(product);
        }


        // GET: Products/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = await db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Product product = await db.Products.FindAsync(id);
            blobHelper.DeleteProductBlob(product.ImageName);
            db.Products.Remove(product);
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
