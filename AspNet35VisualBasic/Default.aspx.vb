' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

Imports System.Globalization

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

            ' Add the SameSite attribute
            ' As .NET 3.5 does Not support SameSite as a property you
            ' must append the attribute And value to the cookie path property
            sameSiteCookie.Path += "; sameSite=Lax"

            ' Add the cookie to the response cookie collection
            Response.Cookies.Add(sameSiteCookie)
        End If

        Response.Redirect("~/default.aspx")
    End Sub

    Private Sub RenderCookieDetails()
        For Each cookieName As String In Request.Cookies.AllKeys

            Dim cookie As HttpCookie
            cookie = Request.Cookies(cookieName)

            Dim TableRow As New TableRow
            Dim nameCell As New TableCell
            Dim valueCell As New TableCell
            Dim secureCell As New TableCell
            Dim domainCell As New TableCell
            Dim pathCell As New TableCell

            nameCell.Text = HttpUtility.HtmlEncode(cookie.Name)
            valueCell.Text = HttpUtility.HtmlEncode(cookie.Value)
            secureCell.Text = cookie.Secure.ToString(CultureInfo.InvariantCulture)
            domainCell.Text = HttpUtility.HtmlEncode(cookie.Domain)
            pathCell.Text = HttpUtility.HtmlEncode(cookie.Path)

            TableRow.Cells.Add(nameCell)
            TableRow.Cells.Add(valueCell)
            TableRow.Cells.Add(secureCell)
            TableRow.Cells.Add(domainCell)
            TableRow.Cells.Add(pathCell)

            CookieList.Rows.Add(TableRow)
        Next
    End Sub

End Class