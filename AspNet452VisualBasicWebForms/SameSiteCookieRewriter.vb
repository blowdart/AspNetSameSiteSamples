' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
Imports System.Globalization

Module SameSiteCookieRewriter
    Sub FilterSameSiteNoneForIncompatibleUserAgents(ByVal sender As Object)
        Dim application As HttpApplication = TryCast(sender, HttpApplication)

        If application IsNot Nothing Then
            Dim userAgent = application.Context.Request.UserAgent

            If SameSite.BrowserDetection.DisallowsSameSiteNone(userAgent) Then
                HttpContext.Current.Response.AddOnSendingHeaders(
                    Function(context)
                        Dim cookies = context.Response.Cookies

                        For i = 0 To cookies.Count - 1
                            Dim cookie = cookies(i)

                            If cookie.Path.Contains(";"c) Then
                                Const sameSiteProperty As String = "sameSite"
                                Dim splitPath = cookie.Path.Split(";"c)

                                For j = 1 To splitPath.Length - 1
                                    Dim splitAttribute = splitPath(j).Split("="c)

                                    If String.Compare(sameSiteProperty, splitAttribute(0).TrimStart(), False) = 0 Then

                                        If String.Compare("None", splitAttribute(1).TrimStart(), False) = 0 Then
                                            cookie.Path = splitPath(0)
                                        End If
                                    End If
                                Next
                            End If
                        Next
#Disable Warning BC42105 ' Function doesn't return a value on all code paths
                    End Function)
#Enable Warning BC42105 ' Function doesn't return a value on all code paths
            End If
        End If
    End Sub

    Sub SetSameSiteAttribute(ByVal sender As Object, ByVal cookieName As String, ByVal sameSiteValue As String)
        Const sameSiteAttribute As String = "sameSite="
        Dim application As HttpApplication = TryCast(sender, HttpApplication)

        If application Is Nothing Then
            Return
        End If

        Dim c As HttpCookie = application.Response.Cookies(cookieName)

        If c.Path.Contains(sameSiteAttribute) Then
            Dim pathParts As String() = c.Path.Split(New Char() {";"c})

            For i As Integer = 0 To pathParts.Length - 1

                If pathParts(i).Trim().StartsWith(sameSiteAttribute, StringComparison.InvariantCulture) Then
                    pathParts(i) = " " & sameSiteAttribute & " " & sameSiteValue
                End If
            Next

            c.Path = String.Join(";", pathParts)
        Else
            c.Path += "; " & sameSiteAttribute & sameSiteValue
        End If

        If String.Compare("None", sameSiteValue, False, CultureInfo.InvariantCulture) = 0 Then
            c.Secure = True
        End If
    End Sub
End Module
