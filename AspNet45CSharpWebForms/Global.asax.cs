using System;
using System.Web;
using System.Web.Routing;


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