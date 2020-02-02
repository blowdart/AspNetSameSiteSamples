' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Public Class Global_asax
    Inherits HttpApplication

    Sub Application_Start(sender As Object, e As EventArgs)
        ' Fires when the application is started
        RouteConfig.RegisterRoutes(RouteTable.Routes)
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Set a sameSite value for both the session and authentication cookie.
        ' We cannot do this in config as a SameSite property is not added until .NET 4.7.2.
        ' Note that this works by creating the empty cookie first. This may have side effects
        ' if you use it for cookies where code always expects a value.
        'SameSiteCookieRewriter.SetSameSiteAttribute(sender, "ASP.NET_SessionId", "None")
        'SameSiteCookieRewriter.SetSameSiteAttribute(sender, ".ASPXAUTH", "None")
        SameSiteCookieRewriter.FilterSameSiteNoneForIncompatibleUserAgents(sender)
    End Sub
End Class