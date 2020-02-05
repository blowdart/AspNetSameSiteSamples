// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AspNet45CSharpWebForms
{
    public static class SameSiteCookieRewriter
    {
        internal static readonly bool IsSameSitePropertyAvailable;
        internal static readonly MethodInfo SameSiteGetter;
        internal static readonly MethodInfo SameSiteSetter;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static SameSiteCookieRewriter()
        {
            var systemWeb = typeof(HttpContextBase).Assembly;

            // Check if we're actually on top of a runtime that has sameSite support.
            IsSameSitePropertyAvailable = systemWeb.GetType("System.Web.SameSiteMode") != null;
            if (IsSameSitePropertyAvailable)
            {
                SameSiteGetter = typeof(HttpCookie).GetProperty("SameSite").GetMethod;
                SameSiteSetter = typeof(HttpCookie).GetProperty("SameSite").SetMethod;
            }
        }

        public static void FilterSameSiteNoneForIncompatibleUserAgents(object sender, IDictionary<string, SameSiteMode> sameSiteOverrides)
        {
            HttpApplication application = sender as HttpApplication;
            if (application != null)
            {
                HttpContext.Current.Response.AddOnSendingHeaders(context =>
                {
                    var cookies = context.Response.Cookies;
                    var userAgent = application.Context.Request.UserAgent;

                    for (var i = 0; i < cookies.Count; i++)
                    {
                        var cookie = cookies[i];

                        // Check if we need to force a sameSite value for this cookie, before we then transform it based on browser support.
                        if (sameSiteOverrides.ContainsKey(cookie.Name))
                        {
                            SetSameSiteAttribute(cookie, sameSiteOverrides[cookie.Name]);
                        }

                        if (SameSite.BrowserDetection.DisallowsSameSiteNone(userAgent))
                        {
                            if (IsSameSitePropertyAvailable)
                            {
                                object existingValueViaReflection = SameSiteGetter.Invoke(cookie, null);
                                int existingSameSiteValue = Convert.ToInt32(existingValueViaReflection);

                                if (existingSameSiteValue == (int)SameSiteMode.None)
                                {
                                    SameSiteSetter.Invoke(cookie, new object[] { -1 });
                                }
                            }
                            else
                            {
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
                        }
                    }
                });
            }
        }

        private static void SetSameSiteAttribute(HttpCookie c, SameSiteMode sameSiteMode)
        {
            // Use the underlying framework SameSite property if available.
            if (IsSameSitePropertyAvailable)
            {
                // Are we running on a runtime that has been patched to support SameSite -1 to avoid writing the value?
                SameSiteSetter.Invoke(c, new object[]
                {
                    sameSiteMode
                });

                // If we set the sameSite attribute to none the new Chrome changes also need it to be marked as secure.
                // Your website must be running on HTTPS for the Secure flag to work as expected.
                if (sameSiteMode == SameSiteMode.None)
                {
                    c.Secure = true;
                }

                return;
            }
            else
            {
                const string sameSiteAttribute = "sameSite=";

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
                            pathParts[i] = " " + sameSiteAttribute + " " + sameSiteMode.ToString();
                        }
                    }

                    // Replace the path
                    c.Path = string.Join(";", pathParts);
                }
                else
                {
                    // Adding a value where it didn't exist before is easy.
                    c.Path += "; " + sameSiteAttribute + sameSiteMode.ToString();
                }

                // If we set the sameSite attribute to none the new Chrome changes also need it to be marked as secure.
                // Your website must be running on HTTPS for the Secure flag to work as expected.
                if (string.Compare("None", sameSiteMode.ToString(), false, CultureInfo.InvariantCulture) == 0)
                {
                    c.Secure = true;
                }
            }
        }
    }

    public enum SameSiteMode
    {
        None = 0,
        Lax = 1,
        Strict = 2
    }
}