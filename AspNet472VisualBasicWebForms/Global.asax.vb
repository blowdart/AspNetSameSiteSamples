' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

Public Class Global_asax
    Inherits System.Web.HttpApplication

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        SameSiteCookieRewriter.FilterSameSiteNoneForIncompatibleUserAgents(sender)
    End Sub

End Class