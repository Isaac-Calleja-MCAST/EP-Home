using Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Factory;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Presentation.Controllers
{
    public class BulkImportController : Controller
    {
        // We need this to get the wwwroot path for saving images
        private readonly IWebHostEnvironment _host;

        // Constructor Injection
        public BulkImportController(IWebHostEnvironment host)
        {
            _host = host;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadJson(IFormFile jsonFile,
            [FromKeyedServices("memory")] IItemsRepository memoryRepo)
        {
            if (jsonFile == null || jsonFile.Length == 0) return RedirectToAction("Index");

            string json;
            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
            {
                json = reader.ReadToEnd();
            }

            var factory = new ImportItemFactory();
            var items = factory.Create(json);

            memoryRepo.Save(items);

            return View("Preview", items);
        }

        public IActionResult DownloadZip([FromKeyedServices("memory")] IItemsRepository memoryRepo)
        {
            var items = memoryRepo.Get();
            var ms = new MemoryStream();

            // Path to the actual default image on your server
            string defaultImgPath = Path.Combine(_host.WebRootPath, "images", "default.jpg");
            bool defaultImageExists = System.IO.File.Exists(defaultImgPath);

            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var item in items)
                {
                    string folderName = item.GetIdString(); // e.g. "R-1001"

                    // Always add the default image into each folder ~ requirement spec
                    archive.CreateEntryFromFile(defaultImgPath, $"{folderName}/default.jpg");
                }
            }

            ms.Position = 0;
            return File(ms, "application/zip", "images-skeleton.zip");
        }

        [HttpPost]
        public IActionResult Commit(IFormFile zipFile,
             [FromKeyedServices("memory")] IItemsRepository memoryRepo,
             [FromKeyedServices("db")] IItemsRepository dbRepo)
        {
            // Materialize the list (ToList) to ensure we can modify properties safely
            var items = memoryRepo.Get().ToList();

            if (zipFile != null && zipFile.Length > 0)
            {
                // Prepare upload directory: wwwroot/images/uploads
                string uploadFolder = Path.Combine(_host.WebRootPath, "images", "uploads");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                using (var stream = zipFile.OpenReadStream())
                using (var archive = new ZipArchive(stream))
                {
                    foreach (var item in items)
                    {
                        // Match zip entries starting with the Item ID (e.g. "R-1001/my-pic.jpg")
                        var imageEntry = archive.Entries.FirstOrDefault(e =>
                            e.FullName.StartsWith(item.GetIdString(), StringComparison.OrdinalIgnoreCase) &&
                            (e.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                             e.FullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                             e.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)));

                        if (imageEntry != null)
                        {
                            // Security: Generate a unique name to prevent overwriting
                            string extension = Path.GetExtension(imageEntry.Name);
                            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                            string filePath = Path.Combine(uploadFolder, uniqueFileName);

                            // Extract
                            imageEntry.ExtractToFile(filePath, true);

                            // Update Model (Cast is necessary as Interface doesn't have ImagePath)
                            if (item is Domain.Models.Restaurant r)
                                r.ImagePath = "/images/uploads/" + uniqueFileName;

                            if (item is Domain.Models.MenuItem m)
                                m.ImagePath = "/images/uploads/" + uniqueFileName;
                        }
                    }
                }
            }

            // Save to Database
            dbRepo.Save(items);

            // Clear Memory Cache
            memoryRepo.Save(null);

            return RedirectToAction("Index", "Catalog");
        }
    }
}