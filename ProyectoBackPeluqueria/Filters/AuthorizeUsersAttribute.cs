using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProyectoBackPeluqueria.Filters
{
    public class AuthorizeUsersAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Verificamos si el usuario no está autenticado
            if (!user.Identity.IsAuthenticated)
            {
                // Redirigimos a la acción de login
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
