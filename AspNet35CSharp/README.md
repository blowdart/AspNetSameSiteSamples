# SameSite Cookie Sample
## .NET Framework 3.5
### Summary

.NET Framework 3.5 has no built-in support for the [SameSite](https://www.owasp.org/index.php/SameSite) attribute, however it can be added to a cookie by 
manually appending the attribute and value to the `Path` property on a `Cookie` instance.

### <a name="sampleCode"></a>Writing the SameSite attribute

Following is an example of how to write a SameSite attribute on a cookie;

```c#
// Create the cookie
HttpCookie sameSiteCookie = new HttpCookie("SameSiteSample");

// Set a value for the cookie
sameSiteCookie.Value = "sample";

// Set the secure flag, which Chrome's changes will require for SameSite none.
// Note this will also require you to be running on HTTPS
sameSiteCookie.Secure = true;

// Set the cookie to HTTP only which is good practice unless you really do need
// to access it client side in scripts.
sameSiteCookie.HttpOnly = true;

// Add the SameSite attribute
// As .NET 3.5 does not support SameSite as a property you
// must append the attribute and value to the cookie path property
sameSiteCookie.Path += "; sameSite=Lax";

// Add the cookie to the response cookie collection
Response.Cookies.Add(sameSiteCookie);
```

If you examine the `Path` property on an inbound cookie written using this method you will see the SameSite attribute appended to it.
This should not have any side effects in your code, the `Path` property is a hint for browsers, not for use server side.

### Running the sample

If you run the sample project please load your browser debugger on the initial page and use it to view the cookie collection for the site.
To do so in Edge and Chrome press `F12` then select the `Application` tab and click the site URL under the `Cookies` option in the `Storage` section.

![Browser Debugger Cookie List](BrowserDebugger.jpg)

You can see from the image above that the cookie created by the sample when you click the "Create SameSite Cookie" button has a SameSite attribute value of `Lax`,
matching the value set in the [sample code](#sampleCode).

## Intercepting cookies you do not control

In .NET 3.5 there is no reliable way to intercept responses outside of an unmanaged IIS module. While you may be tempted to use the `Application_PreSendRequestHeaders`
global event it is [unreliable](https://docs.microsoft.com/en-us/dotnet/api/system.web.httpapplication.presendrequestheaders?view=netframework-3.5) and is unaware 
of any modules in your ASP.NET pipeline. Attempting to use this event will cause Access Violation exceptions that will crash the process hosting your application. 

In order to intercept cookies outside of your control, such as authentication or session cookies you must upgrade your application to .NET 4.5 or greater. 
Updating to .NET 4.7.2 or later will give you access to built-in `SameSite` property on the `Cookie` class.

## More Information

[Chrome Updates](https://www.chromium.org/updates/same-site)

[ASP.NET Documentation](https://docs.microsoft.com/en-us/aspnet/samesite/system-web-samesite)
