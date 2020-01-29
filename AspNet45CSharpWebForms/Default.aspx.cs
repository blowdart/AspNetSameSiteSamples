using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspNet45CSharpWebForms
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                RenderCookieDetails();
            }
        }

        protected void WriteCookie_Click(object sender, EventArgs e)
        {
            const string CookieName = "sampleCookie";
            if (Request.Cookies[CookieName] == null)
            {
                // Create the cookie
                HttpCookie sameSiteCookie = new HttpCookie(CookieName);

                // Set a value for the cookie
                sameSiteCookie.Value = "sample";

                // Set the secure flag, which Chrome's changes will require for SameSite none.
                // Note this will also require you to be running on HTTPS
                sameSiteCookie.Secure = true;

                // Set the cookie to HTTP only which is good practice unless you really do need
                // to access it client side in scripts.
                sameSiteCookie.HttpOnly = true;

                // Add the SameSite attribute
                // As .NET 4.5 does not support SameSite as a property you
                // must append the attribute and value to the cookie path property
                sameSiteCookie.Path += "; sameSite=Lax";

                // Add the cookie to the response cookie collection
                Response.Cookies.Add(sameSiteCookie);
            }

            // Create a pretend authentication cookie which does not have the SameSite attribute on it.
            const string SampleAuthCookie = ".ASPXAUTH";
            if (Request.Cookies[SampleAuthCookie] == null)
            {
                // Create the cookie
                HttpCookie sameSiteCookie = new HttpCookie(SampleAuthCookie);

                // Set a value for the cookie
                sameSiteCookie.Value = "authentication";

                // Set the secure flag, which Chrome's changes will require for SameSite none.
                // Note this will also require you to be running on HTTPS
                sameSiteCookie.Secure = true;

                // Set the cookie to HTTP only which is good practice unless you really do need
                // to access it client side in scripts.
                sameSiteCookie.HttpOnly = true;

                // Add the SameSite attribute
                // As .NET 4.5 does not support SameSite as a property you
                // must append the attribute and value to the cookie path property
                sameSiteCookie.Path += "; sameSite=None";

                // Add the cookie to the response cookie collection
                Response.Cookies.Add(sameSiteCookie);
            }

            Response.Redirect("Default.aspx");
        }


        private void RenderCookieDetails()
        {
            for (int i = 1; i < CookieList.Rows.Count; i++)
            {
                CookieList.Rows.RemoveAt(i);
            }

            foreach (var cookieName in Request.Cookies.AllKeys)
            {
                var cookie = Request.Cookies[cookieName];

                TableRow tableRow = new TableRow();
                TableCell nameCell = new TableCell();
                TableCell valueCell = new TableCell();
                TableCell secureCell = new TableCell();
                TableCell domainCell = new TableCell();
                TableCell pathCell = new TableCell();

                nameCell.Text = HttpUtility.HtmlEncode(cookie.Name);
                valueCell.Text = HttpUtility.HtmlEncode(cookie.Value);
                secureCell.Text = cookie.Secure.ToString(CultureInfo.InvariantCulture);
                domainCell.Text = HttpUtility.HtmlEncode(cookie.Domain);
                pathCell.Text = HttpUtility.HtmlEncode(cookie.Path);

                tableRow.Cells.Add(nameCell);
                tableRow.Cells.Add(valueCell);
                tableRow.Cells.Add(secureCell);
                tableRow.Cells.Add(domainCell);
                tableRow.Cells.Add(pathCell);

                CookieList.Rows.Add(tableRow);
            }
        }
    }
}