using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AspNetCore31RazorPages.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IReadOnlyCollection<CookieDetails> CookieDetails { get; private set; }

        public void OnGet()
        {
            var cookieList = new List<CookieDetails>();
            foreach (var cookieName in HttpContext.Request.Cookies.Keys.Distinct())
            {
                cookieList.Add(new CookieDetails
                {
                    Name = cookieName,
                    Value = Request.Cookies[cookieName]
                });
            }
            CookieDetails = new ReadOnlyCollection<CookieDetails>(cookieList);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            const string CookieName = "sampleCookie";
            if (Request.Cookies[CookieName] == null)
            {
                var cookieOptions = new CookieOptions
                {
                    // Set the secure flag, which Chrome's changes will require for SameSite none.
                    // Note this will also require you to be running on HTTPS
                    Secure = true,

                    // Set the cookie to HTTP only which is good practice unless you really do need
                    // to access it client side in scripts.
                    HttpOnly = true,

                    // Add the SameSite attribute, this will emit the attribute with a value of none.
                    // To not emit the attribute at all set the SameSite property to SameSiteMode.Unspecified.
                    SameSite = SameSiteMode.Unspecified
                };

                // Add the cookie to the response cookie collection
                Response.Cookies.Append(CookieName, "cookieValue", cookieOptions);
            }

            // Create a session variable to get a session cookie created.
            HttpContext.Session.SetString("session", "value");

            // And fake a login to create an authentication cookie
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "username") };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties());

            return RedirectToPage("./Index");
        }
    }

    public class CookieDetails
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
