<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ASP.NET 4.72 VB.NET SameSite MVC sample</title>
</head>
<body>
    <p><b>Current Cookies</b></p>
    <table>
        <thead>
            <tr>
                <td>Cookie</td>
                <td>Value</td>
                <td>Secure</td>
                <td>Domain</td>
                <td>Path</td>
            </tr>
        </thead>
        <tbody>
            @For Each cookie As String In Request.Cookies.AllKeys.Distinct()
                @<tr>
                    <td>@Request.Cookies(cookie).Name</td>
                    <td>@Request.Cookies(cookie).Value</td>
                </tr>
            Next
        </tbody>
     </table>

    @Using Html.BeginForm("CreateCookies", "Home", FormMethod.Post)
        @<input type = "submit" value="Create Cookies" />
    End Using
</body>
</html>
