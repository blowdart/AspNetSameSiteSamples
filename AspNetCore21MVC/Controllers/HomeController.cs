// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using AspNetCore21MVC.Models;

namespace AspNetCore21MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var cookieDetails = new List<CookieDetails>();
            foreach (var cookieName in HttpContext.Request.Cookies.Keys.Distinct())
            {
                cookieDetails.Add(new CookieDetails
                {
                    Name = cookieName,
                    Value = Request.Cookies[cookieName]
                });
            }
            return View(new DefaultViewModel(cookieDetails));
        }

        [HttpPost]
        public async Task<ActionResult> CreateCookies()
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
                    // To not emit the attribute at all set the SameSite property to (SameSiteMode)(-1).
                    SameSite = SameSiteMode.None
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

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
