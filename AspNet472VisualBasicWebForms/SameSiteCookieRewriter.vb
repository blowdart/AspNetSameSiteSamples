' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

Module SameSiteCookieRewriter
    Sub FilterSameSiteNoneForIncompatibleUserAgents(ByVal sender As Object)
        Dim application As HttpApplication = TryCast(sender, HttpApplication)

        If application IsNot Nothing Then
            Dim userAgent = application.Context.Request.UserAgent

            If SameSite.DisallowsSameSiteNone(userAgent) Then
                application.Response.AddOnSendingHeaders(
                    Function(context)
                        Dim cookies = context.Response.Cookies

                        For i = 0 To cookies.Count - 1
                            Dim cookie = cookies(i)

                            If cookie.SameSite = SameSiteMode.None Then
                                cookie.SameSite = CType((-1), SameSiteMode)
                            End If
                        Next
#Disable Warning BC42105 ' Function doesn't return a value on all code paths
                    End Function)
#Enable Warning BC42105 ' Function doesn't return a value on all code paths
            End If
        End If
    End Sub
End Module

