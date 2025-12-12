using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Context;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore; // For .Include()

namespace DataAccess.Repositories
{
    public class ItemsDbRepository : IItemsRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemsDbRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Save(IEnumerable<IItemValidating> items)
        {
            // 1. (Default Images)
            foreach (var item in items)
            {
                if (item is Restaurant r && string.IsNullOrEmpty(r.ImagePath))
                    r.ImagePath = "/images/default.jpg";
                if (item is MenuItem m && string.IsNullOrEmpty(m.ImagePath))
                    m.ImagePath = "/images/default.jpg";
            }

            var restaurants = items.OfType<Restaurant>().ToList();
            var menuItems = items.OfType<MenuItem>().ToList();

            // 2. LINKING LOGIC
            // using JSON IDs to connect the C# objects together
            foreach (var menu in menuItems)
            {
                // Find the parent restaurant in list
                var parent = restaurants.FirstOrDefault(r => r.Id == menu.RestaurantId);

                if (parent != null)
                {
                    menu.Restaurant = parent; // Linking by Object Reference

                    if (parent.MenuItems == null)
                    {
                        parent.MenuItems = new List<MenuItem>();
                    }
                    parent.MenuItems.Add(menu);
                }
            }

            // 3. RESETING IDs
            foreach (var r in restaurants)
            {
                r.Id = 0; // Reset to 0 so DB generates new ID (Identity)
            }

            // 4. SAVE
            _context.Restaurants.AddRange(restaurants);
            _context.SaveChanges();
        }


        public IEnumerable<IItemValidating> Get()
        {
            var list = new List<IItemValidating>();
            // We include MenuItems when loading Restaurants, and Restaurant when loading MenuItems
            list.AddRange(_context.Restaurants.Include(r => r.MenuItems));
            list.AddRange(_context.MenuItems.Include(m => m.Restaurant));
            return list;
        }

        public void Approve(string idString)
        {
            if (idString.StartsWith("R-"))
            {
                int id = int.Parse(idString.Replace("R-", ""));
                var r = _context.Restaurants.Find(id);
                if (r != null) r.Status = "Approved";
            }
            else if (idString.StartsWith("M-"))
            {
                if (Guid.TryParse(idString.Replace("M-", ""), out Guid id))
                {
                    var m = _context.MenuItems.Find(id);
                    if (m != null) m.Status = "Approved";
                }
            }
            _context.SaveChanges();
        }
    }
}