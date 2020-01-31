// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security.Cookies;

using Owin;

namespace AspNet472CSharpMVC5
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                CookieSameSite = SameSiteMode.None,
                CookieHttpOnly = true,
                CookieSecure = CookieSecureOption.Always,
                CookieManager = new SameSiteCookieManager(new SystemWebCookieManager())
            });
        }
    }
}