# ***WORK IN PROGRESS***

*Not official Microsoft guidance until completed and moved to a Microsoft site*

# ASP.NET Same Site Cookie Samples

## What changed

SameSite is an IETF draft standard designed to provide some protection against cross-site request forgery (CSRF) attacks. 
Originally drafted in [2016](https://tools.ietf.org/html/draft-west-first-party-cookies-07), Google proposed, then implemented an update to the standard and Chrome in 
[2019[(https://tools.ietf.org/html/draft-west-cookie-incrementalism-00). The updated standard is not backward compatible with the previous standard, with the following being the most noticeable differences:

* Cookies without SameSite header are treated as SameSite=Lax by default.
* SameSite=None must be used to allow cross-site cookie use.
* Cookies that assert SameSite=None must also be marked as Secure.
* Applications that use iframe may experience issues with SameSite=Lax or SameSite=Strict cookies because iframes are treated as cross-site scenarios.

The value SameSite=None is not allowed by the 2016 standard and causes some implementations which confirm to the original sample to treat such cookies as SameSite=Strict, which
will break applications which rely on the standardized behavior, including some forms of authentication like OpenID Connect (OIDC) and WS-Federation

## .NET specific changes

.Net 4.7.2 and 4.8 supports the 2019 draft standard for SameSite since the release of updates in December 2019. Developers are able to programmatically control the value 
of the SameSite header using the `HttpCookie.SameSite` property. Setting the `SameSite` property to Strict, Lax, or None results in those values being written on the network 
with the cookie. Setting it equal to (SameSiteMode)(-1) indicates that no SameSite header should be included on the network with the cookie. 
The HttpCookie.Secure Property, or `requireSSL` in config files, can be used to mark the cookie as Secure or not.

The specific behavior change is how the `SameSite` property interprets the `None` value. Before the patch a value of `None` meant "Do not emit the attribute at all", after
the patch it means "Emit the attribute with a value of `None`". After the patch a `SameSite` value of `(SameSiteMode)(-1)` causes the attribute not to be emitted.

## What this means to you

To summarize this in browser terms

If you install the patch and issue a cookie with `SameSite.None` set one of two things will happen;
* Chrome v80 will treat this cookie according to the new implementation, and not enforce same site restrictions on the cookie.
* Any browser that has not been updated to support the new implementation will follow the old implementation which says "If you see a value you don't understand ignore it and switch to strict same site restrictions"

So either you break in Chrome, or you break in a lot of other places.

## Fixing the problem

Microsoft's approach to fixing the problem is to help you implement browser sniffing components to strip the `sameSite=None` attribute from cookies if a browser is known to not support it.
Google's advice was to issue double cookies, one with the new attribute, and one without the attribute at all however we consider this approach limited; some browsers, 
especially mobile browsers have very small limits on the number of cookies a site, or a domain name can send, and sending multiple cookies, especially large cookies like
authentication cookies can reach that limit very quickly causing application breaks that are hard to diagnose and fix. Furthermore as a framework there is a large
ecosystem of third party code and components that may not be updated to use a double cookie approach.

The browser sniffing code used in the sample projects in this solution is contained in two files

* [C# SameSiteSupport.cs](SameSiteSupport.cs)
* [VB SameSiteSupport.vb](SameSiteSupport.vb)

These detections are the most common browser agents we have seen that support the 2016 standard and for which the attribute needs to be completely removed. It
is not meant as a complete implementation, your application may see browsers that our test sites do not, and so you should be prepared to add detections as necessary
for your environment.

How you wire up the detection varies according the version of .NET and the web framework that you are using. 

We *strongly* advise you target .NET 4.7.2 or greater if you are not already doing so, it contains APIs which make supporting sameSite easier.

## Testing


## Sample list

This solution contains examples of what is possible in

* .NET 4.7.2 and ASP.NET WebForms - [C#](AspNet472CSharpWebForms/README.md) and [VB.Net](AspNet472VisualBasicWebForms/README.md)
* .NET 4.5 and ASP.NET WebForms - [C#](AspNet472CSharpWebForms/README.md) and [VB.Net](AspNet472VisualBasicWebForms/README.md)
* .NET 3.5 - [C#](AspNet35CSharp/README.md)

**More to come**

## More Information

[Chrome Updates](https://www.chromium.org/updates/same-site)

[ASP.NET Documentation](https://docs.microsoft.com/en-us/aspnet/samesite/system-web-samesite)

[.NET SameSite Patches](https://docs.microsoft.com/en-us/aspnet/samesite/kbs-samesite)