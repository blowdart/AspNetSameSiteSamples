// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web;

namespace AspNet45CSharpWebForms
{
    public class Global : HttpApplication
    {
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Write up the sameSite filtering and set the cookie names and values
            // where we want to force sameSite but which are out of our control.
            SameSiteCookieRewriter.FilterSameSiteNoneForIncompatibleUserAgents(
                sender,
                new Dictionary<string, SameSiteMode>() { { "ASP.NET_SessionId", SameSiteMode.None }, { ".ASPXAUTH", SameSiteMode.None } });
        }
    }
}