' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt In the project root for license information.

Namespace SameSite
    Module BrowserDetection
        Function DisallowsSameSiteNone(ByVal userAgent As String) As Boolean
            If userAgent.Contains("CPU iPhone OS 12") OrElse userAgent.Contains("iPad; CPU OS 12") Then
                Return True
            End If

            If userAgent.Contains("Macintosh; Intel Mac OS X 10_14") AndAlso userAgent.Contains("Version/") AndAlso userAgent.Contains("Safari") Then
                Return True
            End If

            If userAgent.Contains("Chrome/5") OrElse userAgent.Contains("Chrome/6") Then
                Return True
            End If

            If userAgent.Contains("UnrealEngine") AndAlso Not userAgent.Contains("Chrome") Then
                Return True
            End If

            Return False
        End Function

        Function AllowsSameSiteNone(ByVal userAgent As String) As Boolean
            Return Not DisallowsSameSiteNone(userAgent)
        End Function
    End Module
End Namespace