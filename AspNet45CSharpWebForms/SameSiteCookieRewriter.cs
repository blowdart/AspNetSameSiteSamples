// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Web;

namespace AspNet45CSharpWebForms
{
    public static class SameSiteCookieRewriter
    {
        public static void FilterSameSiteNoneForIncompatibleUserAgents(object sender)
        {
            HttpApplication application = sender as HttpApplication;
            if (application != null)
            {
                var userAgent = application.Context.Request.UserAgent;
                if (SameSite.BrowserDetection.DisallowsSameSiteNone(userAgent))
                {
                    HttpContext.Current.Response.AddOnSendingHeaders(context =>
                    {
                        var cookies = context.Response.Cookies;
                        for (var i = 0; i < cookies.Count; i++)
                        {
                            var cookie = cookies[i];

                            // As SameSite in .NET < 4.7.2 can only be manually appended to path, we need to split the path to see if the attribute is set.
                            // This is very fragile and should be considered a last resort. Users should consider updating to .NET 4.7.2 where possible.
                            if (cookie.Path.Contains(';'))
                            {
                                const string sameSiteProperty = "sameSite";
                                var splitPath = cookie.Path.Split(';');
                                // We potentially have a sameSite attribute
                                for (var j = 1; j < splitPath.Length; j++)
                                {
                                    var splitAttribute = splitPath[j].Split('=');

                                    if (string.Compare(sameSiteProperty, splitAttribute[0].TrimStart(), false) == 0)
                                    {
                                        // We have an appended sameSite attribute.
                                        if (string.Compare("None", splitAttribute[1].TrimStart(), false) == 0)
                                        {
                                            // And it's set to the new none value for a browser that doesn't support that value.
                                            // So we need to strip the attribute off to revert to not sending it at all.
                                            cookie.Path = splitPath[0];
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }

        public static void SetSameSiteAttribute(object sender, string cookieName, string sameSiteValue)
        {
            const string sameSiteAttribute = "sameSite=";

            HttpApplication application = sender as HttpApplication;
            if (application == null)
            {
                return;
            }

            HttpCookie c = application.Response.Cookies[cookieName];
            if (c == null)
            {
                return;
            }

            // Cookie already has a SameSite value. Replace it.
            if (c.Path.Contains(sameSiteAttribute))
            {
                // Find the SameSite value
                string[] pathParts = c.Path.Split(new char[] { ';' });
                for (int i = 0; i < pathParts.Length; i++)
                {
                    // Update the SameSite value
                    if (pathParts[i].Trim().StartsWith(sameSiteAttribute, StringComparison.InvariantCulture))
                    {
                        pathParts[i] = " " + sameSiteAttribute + " " + sameSiteValue;
                    }
                }

                // Replace the path
                c.Path = string.Join(";", pathParts);
            }
            else
            {
                // Adding a value where it didn't exist before is easy.
                c.Path += "; " + sameSiteAttribute + sameSiteValue;
            }

            // If we set the sameSite attribute to none the new Chrome changes also need it to be marked as secure.
            // Your website must be running on HTTPS for the Secure flag to work as expected.
            if (string.Compare("None", sameSiteValue, false, CultureInfo.InvariantCulture) == 0)
            {
                c.Secure = true;
            }
        }

    }
}