' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt In the project root for license information.
Imports Microsoft.Owin
Imports Microsoft.Owin.Infrastructure

Public Class SameSiteCookieManager
    Implements ICookieManager

    Private ReadOnly _innerManager As ICookieManager

    Public Sub New()
        Me.New(New CookieManager())
    End Sub

    Public Sub New(ByVal innerManager As ICookieManager)
        _innerManager = innerManager
    End Sub

    Public Sub AppendResponseCookie(ByVal context As IOwinContext, ByVal key As String, ByVal value As String, ByVal options As CookieOptions) Implements ICookieManager.AppendResponseCookie
        CheckSameSite(context, options)
        _innerManager.AppendResponseCookie(context, key, value, options)
    End Sub

    Public Sub DeleteCookie(ByVal context As IOwinContext, ByVal key As String, ByVal options As CookieOptions) Implements ICookieManager.DeleteCookie
        CheckSameSite(context, options)
        _innerManager.DeleteCookie(context, key, options)
    End Sub

    Public Function GetRequestCookie(ByVal context As IOwinContext, ByVal key As String) As String Implements ICookieManager.GetRequestCookie
        Return _innerManager.GetRequestCookie(context, key)
    End Function

    Private Sub CheckSameSite(ByVal context As IOwinContext, ByVal options As CookieOptions)
        If options.SameSite = Microsoft.Owin.SameSiteMode.None AndAlso SameSite.BrowserDetection.DisallowsSameSiteNone(context.Request.Headers("User-Agent")) Then
            options.SameSite = Nothing
        End If
    End Sub
End Class
