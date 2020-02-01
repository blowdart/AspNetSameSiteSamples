' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt In the project root for license information.

Imports Microsoft.Owin.Host.SystemWeb
Imports Microsoft.Owin.Security.Cookies
Imports Owin

Public Class Startup
    Public Sub Configuration(app As IAppBuilder)
        app.UseCookieAuthentication(New CookieAuthenticationOptions() With {
            .CookieSameSite = SameSiteMode.None,
            .CookieHttpOnly = True,
            .CookieSecure = CookieSecureOption.Always,
            .CookieManager = New SameSiteCookieManager(New SystemWebCookieManager())
        })
    End Sub
End Class
