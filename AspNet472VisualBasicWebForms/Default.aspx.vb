' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            RenderCookieDetails()
        End If
    End Sub

    Protected Sub WriteCookie_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Const CookieName = "sampleCookie"

        If Request.Cookies(CookieName) Is Nothing Then

            ' Create the cookie
            Dim sameSiteCookie As New HttpCookie(CookieName)

            ' Set a value for the cookie
            sameSiteCookie.Value = "sample"

            ' Set the secure flag, which Chrome's changes will require for SameSite none.
            ' Note this will also require you to be running on HTTPS
            sameSiteCookie.Secure = True

            ' Set the cookie to HTTP only which is good practice unless you really do need
            ' to access it client side in scripts.
            sameSiteCookie.HttpOnly = True

            ' Expire the cookie in 1 minute
            sameSiteCookie.Expires = Date.Now.AddMinutes(1)

            ' Add the SameSite attribute, this will emit the attribute with a value of none.
            ' To Not emit the attribute at all set the SameSite property to -1.
            sameSiteCookie.SameSite = SameSiteMode.None

            ' Add the cookie to the response cookie collection
            Response.Cookies.Add(sameSiteCookie)
        End If

        ' Add things to session to create a session cookie.
        ' Note we're not setting any sameSite attributes here.
        Session.Add("sample", "Sample")

        ' And fake a login to create a membership cookie with forms authentication.
        ' Note we're not setting any sameSite attributes here.
        FormsAuthentication.SetAuthCookie("username", False)

        Response.Redirect("Default.aspx")
    End Sub

    Private Sub RenderCookieDetails()
        If CookieList.Rows.Count > 1 Then
            For i = 1 To CookieList.Rows.Count
                CookieList.Rows.RemoveAt(i)
            Next
        End If


        For Each cookieName As String In Request.Cookies.AllKeys.Distinct()

            Dim cookie As HttpCookie
            cookie = Request.Cookies(cookieName)

            Dim TableRow As New TableRow
            Dim nameCell As New TableCell
            Dim valueCell As New TableCell

            nameCell.Text = HttpUtility.HtmlEncode(cookie.Name)
            valueCell.Text = HttpUtility.HtmlEncode(cookie.Value)

            TableRow.Cells.Add(nameCell)
            TableRow.Cells.Add(valueCell)

            CookieList.Rows.Add(TableRow)
        Next
    End Sub

End Class