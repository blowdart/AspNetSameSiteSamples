using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspNet472CSharpWebForms
{
    public partial class Default : System.Web.UI.Page
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

                // Add the SameSite attribute, this will emit the attribute with a value of none.
                // To not emit the attribute at all set the SameSite property to -1.
                sameSiteCookie.SameSite = SameSiteMode.None;

                // Add the cookie to the response cookie collection
                Response.Cookies.Add(sameSiteCookie);
            }

            // Add things to session to create a session cookie.
            // Note we're not setting any sameSite attributes here.
            Session["sample"] = "Sample";

            // And fake a login to create a membership cookie with forms authentication.
            // Note we're not setting any sameSite attributes here.
            FormsAuthentication.SetAuthCookie("username", false);

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