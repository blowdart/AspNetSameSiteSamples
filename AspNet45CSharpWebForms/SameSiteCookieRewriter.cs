using System;
using System.Collections.Generic;
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
    }
}