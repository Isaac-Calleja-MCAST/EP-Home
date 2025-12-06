using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Presentation.Controllers
{
    public class CatalogController : Controller
    {
        public IActionResult Index(
            [FromKeyedServices("db")] IItemsRepository dbRepo,
            int? restaurantId)
        {
            var items = dbRepo.Get();

            // Filter: If restaurant clicked, show its menu items
            if (restaurantId.HasValue)
            {
                items = items.OfType<MenuItem>()
                             .Where(x => x.RestaurantId == restaurantId)
                             .Cast<IItemValidating>();
            }
            else
            {
                // Default: Show Approved Restaurants
                items = items.OfType<Restaurant>()
                             .Where(x => x.Status == "Approved")
                             .Cast<IItemValidating>();
            }

            return View(items);
        }

        // SE3.3 Verification Action
        public IActionResult Verification([FromKeyedServices("db")] IItemsRepository dbRepo)
        {
            var allItems = dbRepo.Get();
            var email = User.Identity.Name;
            var isAdmin = email == "admin@myasp.net";

            IEnumerable<IItemValidating> pending;

            if (isAdmin)
            {
                // Admin sees pending restaurants
                pending = allItems.OfType<Restaurant>().Where(x => x.Status == "Pending");
            }
            else
            {
                // Owners see pending menu items for THEIR restaurants
                pending = allItems.OfType<MenuItem>()
                    .Where(m => m.Status == "Pending" && m.Restaurant.OwnerEmailAddress == email);
            }

            // Reuse of the Index view
            return View("Index", pending);
        }

        [HttpPost]
        public IActionResult Approve(List<string> ids, [FromKeyedServices("db")] IItemsRepository dbRepo)
        {
            foreach (var id in ids) dbRepo.Approve(id);
            return RedirectToAction("Verification");
        }
    }
}