using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Fitz.WebPortal.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet()
        {
            // If user is already authenticated, redirect to home page
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public IActionResult OnPost(string returnUrl = null)
        {
            // Store the return URL in the session
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Page("/Index");
            }
            else
            {
                returnUrl = Url.Page("/Index");
            }

            // Challenge the Discord authentication
            var properties = new AuthenticationProperties
            {
                RedirectUri = returnUrl
            };

            return Challenge(properties, "Discord");
        }
    }
} 