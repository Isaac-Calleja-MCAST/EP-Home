using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Presentation.Factory;
using System.IO;

namespace Presentation.Controllers
{
    public class BulkImportController : Controller
    {
        // 1. Show upload page
        public IActionResult Index() { return View(); }

        // 2. Process JSON
        [HttpPost]
        public IActionResult UploadJson(IFormFile jsonFile,
            [FromKeyedServices("memory")] IItemsRepository memoryRepo)
        {
            if (jsonFile == null) return View("Index");

            // Read file
            string json;
            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
            {
                json = reader.ReadToEnd();
            }

            // Factory creates objects
            var factory = new ImportItemFactory();
            var items = factory.Create(json);

            // Save to Cache
            memoryRepo.Save(items);

            return View("Preview", items); // We will create this view next
        }

        // 3. Commit (Upload Zip placeholder)
        [HttpPost]
        public IActionResult Commit(IFormFile zipFile,
             [FromKeyedServices("memory")] IItemsRepository memoryRepo,
             [FromKeyedServices("db")] IItemsRepository dbRepo)
        {
            // Get items from Cache
            var items = memoryRepo.Get();

            // (AA4.3: Unzip logic would go here. For now we assume files are processed)

            // Save to DB
            dbRepo.Save(items);

            // Clear Cache
            memoryRepo.Save(null);

            return RedirectToAction("Index", "Catalog");
        }
    }
}