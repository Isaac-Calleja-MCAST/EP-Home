using System.Collections.Generic;
using Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Restaurant : IItemValidating
    {
        [Key]
        public int Id { get; set; } // Identity Int
        public string Name { get; set; }
        public string OwnerEmailAddress { get; set; }
        public string Status { get; set; } = "Pending";

        // --- NEW PROPERTIES ---
        public string Description { get; set; } // For the Card View
        public string Address { get; set; }     // From JSON
        public string Phone { get; set; }       // From JSON
        public string ImagePath { get; set; }   // For AA4.3 (Uploads)
        // ----------------------

        // Navigation Property for relational DB
        public virtual ICollection<MenuItem> MenuItems { get; set; }

        // Interface Implementation
        public string GetIdString() => "R-" + Id;

        public string GetCardPartial()
        {
            return "_RestaurantCard"; // Name of the partial view file
        }

        public List<string> GetValidators()
        {
            // Site Admin approves restaurants.
            return new List<string> { "admin@myasp.net" };
        }
    }
}