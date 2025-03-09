using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankSystem.WebAPI.Controllers
{
    public abstract class ExtendedControllerBase : ControllerBase
    {
        protected int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null 
                || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new Exception("User id is invalid or not found");
            }
            return userId;
        }
    }
}
