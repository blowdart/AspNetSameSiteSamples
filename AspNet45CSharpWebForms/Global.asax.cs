// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web;

namespace AspNet45CSharpWebForms
{
    public class Global : HttpApplication
    {
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Set a sameSite value for both the session and authentication cookie.
            // We cannot do this in config as a SameSite property is not added until .NET 4.7.2.
            SameSiteCookieRewriter.SetSameSiteAttribute(sender, "ASP.NET_SessionId", "None");
            SameSiteCookieRewriter.SetSameSiteAttribute(sender, ".ASPXAUTH", "None");

            // Now adjust it based on the browser.
            SameSiteCookieRewriter.FilterSameSiteNoneForIncompatibleUserAgents(sender);
        }
    }
}