using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection; // For getting services
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Filters
{
    public class ValidatorCheckFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Get the current user's email
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new ForbidResult(); // 403 if not logged in
                return;
            }
            string userEmail = user.Identity.Name;

            // 2. Get the IDs being approved from the Action Arguments
            // The action signature is Approve(List<string> ids, ...)
            var ids = context.ActionArguments["ids"] as List<string>;

            if (ids != null && ids.Any())
            {
                // 3. Resolve the Repository to fetch the actual items
                // We need the DB repo to check who owns these items
                var dbRepo = context.HttpContext.RequestServices.GetRequiredKeyedService<IItemsRepository>("db");
                var allItems = dbRepo.Get();

                foreach (var id in ids)
                {
                    // Find the item (Generic logic for R- or M-)
                    var item = allItems.FirstOrDefault(x => x.GetIdString() == id);

                    if (item != null)
                    {
                        // 4. THE CHECK: Is this user allowed to validate this item?
                        var validators = item.GetValidators();

                        // Case-insensitive check
                        if (!validators.Any(v => v.ToLower() == userEmail.ToLower()))
                        {
                            context.Result = new ForbidResult(); // STOP EXECUTION
                            return;
                        }
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}