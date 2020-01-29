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
            SameSiteCookieRewriter.FilterSameSiteNoneForIncompatibleUserAgents(sender);
        }
    }
}