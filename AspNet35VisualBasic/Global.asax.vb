Imports System.Globalization

Public Class Global_asax
    Inherits System.Web.HttpApplication

    Sub Application_PostAcquireRequestState(ByVal sender As Object, ByVal e As EventArgs)
        If TypeOf sender Is HttpApplication Then
            Dim app As HttpApplication = sender
            If (SameSite.BrowserDetection.AllowsSameSiteNone(app.Request.UserAgent)) Then
                SetSameSite(app.Response.Cookies("ASP.NET_SessionId"), "None")
                SetSameSite(app.Response.Cookies(".ASPXAUTH"), "None")
            End If
        End If
    End Sub

    Private Sub SetSameSite(ByVal c As HttpCookie, ByVal sameSiteValue As String)
        Const sameSiteAttribute As String = "sameSite="

        If c Is Nothing Then
            Return
        End If

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
End Class