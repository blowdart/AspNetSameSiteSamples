' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the Apache License, Version 2.0. See License.txt In the project root for license information.

Namespace SameSite
    Module BrowserDetection
        ' Same as https://devblogs.microsoft.com/aspnet/upcoming-samesite-cookie-changes-in-asp-net-and-asp-net-core/
        Function DisallowsSameSiteNone(ByVal userAgent As String) As Boolean
            ' Note that these detections are a starting point. See https://www.chromium.org/updates/same-site/incompatible-clients for more detections.

            ' Cover all iOS based browsers here. This includes
            ' - Safari on iOS 12 for iPhone, iPod Touch, iPad
            ' - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            ' - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            ' All of which are broken by SameSite=None, because they use the iOS networking stack
            If userAgent.Contains("CPU iPhone OS 12") OrElse userAgent.Contains("iPad; CPU OS 12") Then
                Return True
            End If

            ' Cover Mac OS X based browsers that use the Mac OS networking stack. This includes
            ' - Safari on Mac OS X.
            ' This does Not include:
            ' - Chrome on Mac OS X
            ' Because they do Not use the Mac OS networking stack.
            If userAgent.Contains("Macintosh; Intel Mac OS X 10_14") AndAlso userAgent.Contains("Version/") AndAlso userAgent.Contains("Safari") Then
                Return True
            End If

            ' Cover Chrome 50-69, because some versions are broken by SameSite=None,
            ' And none in this range require it.
            ' Note: this covers some pre-Chromium Edge versions,
            ' but pre-Chromium Edge does Not require SameSite=None.
            If userAgent.Contains("Chrome/5") OrElse userAgent.Contains("Chrome/6") Then
                Return True
            End If

            ' Unreal Engine runs Chromium 59, but does Not advertise as Chrome until 4.23. Treat versions of Unreal
            ' that don't specify their Chrome version as lacking support for SameSite=None.
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