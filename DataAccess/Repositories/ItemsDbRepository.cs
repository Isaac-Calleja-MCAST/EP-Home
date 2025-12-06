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
            foreach (var item in items)
            {
                if (item is Restaurant r) _context.Restaurants.Add(r);
                if (item is MenuItem m) _context.MenuItems.Add(m);
            }
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
                int id = int.Parse(idString.Substring(2));
                var r = _context.Restaurants.Find(id);
                if (r != null) r.Status = "Approved";
            }
            else if (idString.StartsWith("M-"))
            {
                Guid id = Guid.Parse(idString.Substring(2));
                var m = _context.MenuItems.Find(id);
                if (m != null) m.Status = "Approved";
            }
            _context.SaveChanges();
        }
    }
}