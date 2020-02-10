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
            System.Diagnostics.Debug.WriteLine("Checking if we're on a patched framework");

            var systemWeb = typeof(HttpContextBase).Assembly;

            // Check if we're actually on top of a runtime that has sameSite support.
            IsSameSitePropertyAvailable = systemWeb.GetType("System.Web.SameSiteMode") != null;
            if (IsSameSitePropertyAvailable)
            {
                System.Diagnostics.Debug.WriteLine("We are on a patched framework");

                SameSiteGetter = typeof(HttpCookie).GetProperty("SameSite").GetMethod;
                SameSiteSetter = typeof(HttpCookie).GetProperty("SameSite").SetMethod;
            }
        }

        public static void FilterSameSiteNoneForIncompatibleUserAgents(object sender, IDictionary<string, SameSiteMode> sameSiteOverrides)
        {
            HttpApplication application = sender as HttpApplication;
            if (application != null)
            {
                System.Diagnostics.Debug.WriteLine("Adding AddOnSendingHeaders()");

                HttpContext.Current.Response.AddOnSendingHeaders(context =>
                {
                    System.Diagnostics.Debug.WriteLine("Inside AddOnSendingHeaders()");

                    System.Diagnostics.Debug.WriteLine("Looping though " + context.Response.Headers.Count + " headers");

                    for (int i = 0; i < context.Response.Headers.Count; i++)
                    {
                        string name = context.Response.Headers.GetKey(i);
                        string value = context.Response.Headers.Get(i);

                        System.Diagnostics.Debug.WriteLine("Header Key: " + name + " Value: " + value);

                    }

                    var cookies = context.Response.Cookies;

                    System.Diagnostics.Debug.WriteLine("Looping though " + cookies.Count + " cookies");

                    var userAgent = application.Context.Request.UserAgent;

                    System.Diagnostics.Debug.WriteLine("User Agent " + userAgent);

                    for (var i = 0; i < cookies.Count; i++)
                    {
                        var cookie = cookies[i];

                        System.Diagnostics.Debug.WriteLine("Chomping down on " + cookie.Name);

                        // Check if we need to force a sameSite value for this cookie, before we then transform it based on browser support.
                        if (sameSiteOverrides.ContainsKey(cookie.Name))
                        {
                            System.Diagnostics.Debug.WriteLine("We have a configured override for this cookie.");

                            SetSameSiteAttribute(cookie, sameSiteOverrides[cookie.Name]);
                        }

                        if (SameSite.BrowserDetection.DisallowsSameSiteNone(userAgent))
                        {
                            System.Diagnostics.Debug.WriteLine("Browser doesn't support SameSite=None value");

                            if (IsSameSitePropertyAvailable)
                            {
                                System.Diagnostics.Debug.WriteLine("Removing via setter");
                                object existingValueViaReflection = SameSiteGetter.Invoke(cookie, null);
                                int existingSameSiteValue = Convert.ToInt32(existingValueViaReflection);

                                if (existingSameSiteValue == (int)SameSiteMode.None)
                                {
                                    SameSiteSetter.Invoke(cookie, new object[] { -1 });
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Adjusting the path hack on " + cookie.Path);

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
            System.Diagnostics.Debug.WriteLine("Setting SameSite attribute on "+c.Name);


            // Use the underlying framework SameSite property if available.
            if (IsSameSitePropertyAvailable)
            {
                System.Diagnostics.Debug.WriteLine("Calling into SameSiteProperty and setting " + sameSiteMode.ToString());

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

                System.Diagnostics.Debug.WriteLine("Hacking the path");

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

                System.Diagnostics.Debug.WriteLine("Set path to " + c.Path);


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