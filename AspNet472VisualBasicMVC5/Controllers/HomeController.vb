' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt In the project root for license information.

Imports System.Security.Claims
Imports Microsoft.Owin.Security
Imports Microsoft.Owin.Security.Cookies

Public Class HomeController
    Inherits System.Web.Mvc.Controller

    Function Index() As ActionResult
        Return View()
    End Function

    <HttpPost()>
    Function CreateCookies() As ActionResult
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

        ' Create a session variable to get a session cookie created.
        Session("sample") = "sample"

        ' And fake a login to create a membership cookie with OWIN cookie authentication.
        ' Note we're not setting any sameSite attributes here.
        Dim claims = {New Claim(ClaimTypes.NameIdentifier, "username")}
        Dim identity = New ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationType)
        Dim ctx = HttpContext.GetOwinContext()
        Dim authManager = ctx.Authentication
        authManager.SignIn(New AuthenticationProperties With {.IsPersistent = False}, identity)

        Return RedirectToAction("Index")
    End Function
End Class
