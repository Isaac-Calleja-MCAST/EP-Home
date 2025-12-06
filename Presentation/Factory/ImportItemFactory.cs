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
                    // Map JSON to Restaurant
                    var r = token.ToObject<Restaurant>();

                    // Fix ID: remove "R-" prefix
                    string rawId = token["id"].ToString();
                    r.Id = int.Parse(rawId.Replace("R-", ""));

                    // NEW FIELDS
                    r.Description = token["description"]?.ToString();
                    r.Address = token["address"]?.ToString();
                    r.Phone = token["phone"]?.ToString();

                    r.Status = "Pending";
                    list.Add(r);
                }
                else if (type == "menuItem")
                {
                    var m = token.ToObject<MenuItem>();

                    // Fix ID: generate new Guid
                    m.Id = Guid.NewGuid(); // Generate new Guid per brief

                    // Fix Restaurant ID link
                    string rawRestId = token["restaurantId"].ToString();
                    m.RestaurantId = int.Parse(rawRestId.Replace("R-", ""));

                    // NEW FIELD
                    m.Currency = token["currency"]?.ToString();

                    m.Status = "Pending";
                    list.Add(m);
                }
            }
            return list;
        }
    }
}