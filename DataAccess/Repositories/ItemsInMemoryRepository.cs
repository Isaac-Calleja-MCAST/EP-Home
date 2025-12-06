using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DataAccess.Repositories
{
    public class ItemsInMemoryRepository : IItemsRepository
    {
        private readonly IMemoryCache _cache;
        public ItemsInMemoryRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Save(IEnumerable<IItemValidating> items)
        {
            // If items is null, we clear the cache
            if (items == null)
            {
                _cache.Remove("BulkImportCache");
            }
            else
            {
                // Store in RAM for 60 minutes
                _cache.Set("BulkImportCache", items, TimeSpan.FromMinutes(60));
            }
        }

        public IEnumerable<IItemValidating> Get()
        {
            _cache.TryGetValue("BulkImportCache", out IEnumerable<IItemValidating> items);
            return items ?? new List<IItemValidating>();
        }

        public void Approve(string idString) {}
    }
}