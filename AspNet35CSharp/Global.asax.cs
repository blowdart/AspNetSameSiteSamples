using System;
using System.Globalization;
using System.Web;

namespace AspNet35CSharp
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            // Set SessionState cookie to SameSite=None
            if (sender is HttpApplication app)
            {
                if (SameSite.BrowserDetection.AllowsSameSiteNone(app.Request.UserAgent))
                {
                    string[] allKeys = app.Response.Cookies.AllKeys;
                    foreach (string cookieName in allKeys)
                    {
                        switch (cookieName)
                        {
                            case "ASP.Net_SessionId":
                            case ".ASPXAUTH":
                                SetSameSite(app.Response.Cookies[cookieName], "None");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void SetSameSite(HttpCookie c, string sameSiteValue)
        {
            const string sameSiteAttribute = "sameSite=";

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