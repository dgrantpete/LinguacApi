using LinguacApi.Data.Database;
using LinguacApi.Data.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LinguacApi.Data.Binders
{
    public class AuthenticatedUserBinder(LinguacDbContext dbContext) : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(User))
            {
                return;
            }

            var userId = Guid.Parse(bindingContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Name identifier not found in claims."));

            var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId);
            bindingContext.Result = ModelBindingResult.Success(user);
        }
    }
}