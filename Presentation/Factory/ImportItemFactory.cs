using Domain.Interfaces;
using Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Presentation.Factory
{
    public class ImportItemFactory
    {
        public List<IItemValidating> Create(string json)
        {
            var list = new List<IItemValidating>();
            var jArray = JArray.Parse(json);

            foreach (var token in jArray)
            {
                string type = token["type"]?.ToString();

                if (type == "restaurant")
                {
                    // Restaurant Object Initialization
                    var r = new Restaurant();

                    // Fix ID: remove "R-" prefix
                    string rawId = token["id"].ToString();
                    r.Id = int.Parse(rawId.Replace("R-", ""));

                    // FIELDS
                    r.Name = token["name"]?.ToString();
                    r.OwnerEmailAddress = token["ownerEmailAddress"]?.ToString();
                    r.Description = token["description"]?.ToString();
                    r.Address = token["address"]?.ToString();
                    r.Phone = token["phone"]?.ToString();

                    r.Status = "Pending";
                    list.Add(r);
                }
                else if (type == "menuItem")
                {   
                    // Initialized MenuItem Object
                    var m = new MenuItem();

                    // Fix ID: generate new Guid
                    m.Id = Guid.NewGuid(); // Generate new Guid per brief


                    // NEW FIELD
                    m.Title = token["title"]?.ToString();
                    if (double.TryParse(token["price"]?.ToString(), out double price))
                    {
                        m.Price = price;
                    }
                    m.Currency = token["currency"]?.ToString();

                    // Fixed
                    string rawRestId = token["restaurantId"]?.ToString();
                    if (!string.IsNullOrEmpty(rawRestId))
                    {
                        m.RestaurantId = int.Parse(rawRestId.Replace("R-", ""));
                    }

                    m.Status = "Pending";
                    list.Add(m);
                }
            }
            return list;
        }
    }
}