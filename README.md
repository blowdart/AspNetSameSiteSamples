# ***WORK IN PROGRESS***

*Not official Microsoft guidance until completed and moved to a Microsoft site*

# ASP.NET Same Site Cookie Samples

## What changed

SameSite is an IETF draft standard designed to provide some protection against cross-site request forgery (CSRF) attacks. 
Originally drafted in [2016](https://tools.ietf.org/html/draft-west-first-party-cookies-07), Google proposed, then implemented an update to the standard and Chrome in 
[2019[(https://tools.ietf.org/html/draft-west-cookie-incrementalism-00). The updated standard is not backward compatible with the previous standard, with the following being the most noticeable differences:

* Cookies without sameSite attribute are treated as sameSite=Lax by default.
* SameSite=None must be used to allow cross-site cookie use.
* Cookies that assert sameSite=None must also be marked as Secure.
* Applications that use iframes may experience issues with sameSite=Lax or sameSite=Strict cookies because iframes are treated as cross-site scenarios.

The value sameSite=None is not allowed by the 2016 standard and causes some implementations which confirm to the original sample to treat such cookies as SameSite=Strict, which
will break applications which rely on the standardized behavior, including some forms of authentication like OpenID Connect (OIDC) and WS-Federation

## .NET support for the sameSite attribute

.NET 4.7.2 and 4.8 supports the 2019 draft standard for SameSite since the release of updates in December 2019. 
Developers are able to programmatically control the value of the SameSite header using the 
`HttpCookie.SameSite` property. Setting the `SameSite` property to Strict, Lax, or None results in those values 
being written on the network with the cookie. Setting it equal to (SameSiteMode)(-1) indicates that 
no SameSite header should be included on the network with the cookie. 

The HttpCookie.Secure Property, or `requireSSL` in config files, can be used to mark the cookie as Secure or not.

*Any version of .NET or ASP.NET before .NET 4.7.2 is unaware of the sameSite attribute. Samples for these older
versions are provided as a courtesy, but the approaches detailed are unsupported by Microsoft.*

We *strongly* advise you target .NET 4.7.2 or greater if you are not already doing so, 
it contains APIs which make supporting sameSite easier.

### Retargetting your application

To target .NET 4.7.2 or later you must ensure your `web.config` contains the following;

```xml
<system.web>
  <compilation debug="true" targetFramework="4.7.2"/>
  <httpRuntime targetFramework="4.7.2"/>
</system.web>
```

You must also check your project file and look for the TargetFrameworkVersion

```xml
<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
```

The [.NET Migration Guide](https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/) has further details.

## December patch behviour changes

The specific behavior change is how the `SameSite` property interprets the `None` value. 
Before the patch a value of `None` meant "Do not emit the attribute at all", after
the patch it means "Emit the attribute with a value of `None`". 
After the patch a `SameSite` value of `(SameSiteMode)(-1)` causes the attribute not to be emitted.

The default SameSite value for forms authentication and session state cookies was changed from `None` to `Lax`.

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

## Ensuring your site redirects to HTTPS

For ASP.NET 4.x, WebForms and MVC you can use [IIS's URL Rewrite](https://docs.microsoft.com/en-us/iis/extensions/url-rewrite-module/creating-rewrite-rules-for-the-url-rewrite-module) 
feature to redirect all requests to HTTPS. An example rule is as follows;

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Redirect to https" stopProcessing="true">
          <match url="(.*)"/>
          <conditions>
            <add input="{HTTPS}" pattern="Off"/>
            <add input="{REQUEST_METHOD}" pattern="^get$|^head$" />
          </conditions>
          <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent"/>
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

In on-premises installations of IIS URL Rewrite is an optional feature that may need installing.

## Testing

You must test your application with the browsers you support and go through your scenarios that involve cookies. 
These scenarios typically involve

* Login forms
* External login mechanisms such as Facebook, Azure AD, OAuth and OIDC
* Pages that accept requests from other sites
* Pages in your application designed to be embedded in iframes

You should check that cookies are created, persisted and deleted correctly in your application.

### Chrome

To test you will need to [download](https://www.google.com/chrome/) a version of Chrome that supports their new attribute.
At the time of writing this is Chrome 79, which needs a flag (`chrome://flags/#same-site-by-default-cookies`) enabled 
to use the new behavior. You should also enable (`chrome://flags/#cookies-without-same-site-must-be-secure`) to test 
the upcoming behavior for cookies which have no sameSite attribute enabled.
Chrome 80 is on target to make the switch to treat cookies without the attribute as SameSite=Lax, 
albeit with a timed grace period for certain requests. To disable the timed grace 
period Chrome 80 can be launched with the command line argument `--enable-features=SameSiteDefaultChecksMethodRigorously`.

Chrome 79 has warning messages in the browser console (f12 to open) about missing sameSite attributes.

You will also need a browser that does not support the upcoming switch, for example (Chromium v74)[https://commondatastorage.googleapis.com/chromium-browser-snapshots/index.html?prefix=Win_x64/638880/].
If you're not using a 64bit version of Windows you can use the [OmahaProxy viewer](https://omahaproxy.appspot.com/) 
to look up which Chromium branch corresponds to Chrome 74 (v74.0.3729.108) using the 
[instructions provided by Chromium](https://www.chromium.org/getting-involved/download-chromium).

### Safari

Safari 12 strictly implemented the prior draft and fails when the new None value is in a cookie. 
Test Safari 12, Safari 13, and WebKit based OS browsers. The problem is dependent on the underlying OS version. 
OSX Mojave (10.14) and iOS 12 are known to have compatibility problems with the new SameSite behavior. 
Upgrading the OS to OSX Catalina (10.15) or iOS 13 fixes the problem. 
Safari does not currently have an opt-in flag for testing the new spec behavior.

### Firefox

Firefox support for the new standard can be tested on version 68+ by opting in on the about:config page with the 
feature flag `network.cookie.sameSite.laxByDefault`. 

### Test with Edge (Legacy)
Edge supports the old SameSite standard but doesn't have any known compatibility problems with the new standard.

### Electron
Versions of Electron include older versions of Chromium. For example, the version of Electron used by 
Teams is Chromium 66, which exhibits the older behavior. You must perform your own compatibility testing with 
the version of Electron your product uses.

## Sample list

This solution contains examples of what is possible in

* .NET 4.7.2 and ASP.NET MVC 5 - [C#](AspNet472CSharpMVC5/README.md)
* .NET 4.7.2 and ASP.NET WebForms - [C#](AspNet472CSharpWebForms/README.md) and [VB.Net](AspNet472VisualBasicWebForms/README.md)

*Any version of .NET or ASP.NET before .NET 4.7.2 is unaware of the sameSite attribute. Samples for these older
versions are provided as a courtesy, but the approaches detailed are unsupported by Microsoft.*

We *strongly* advise you target .NET 4.7.2 or greater if you are not already doing so, 
it contains APIs which make supporting sameSite easier.

* .NET 4.5 and ASP.NET WebForms - [C#](AspNet472CSharpWebForms/README.md) and [VB.Net](AspNet472VisualBasicWebForms/README.md)
* .NET 3.5 - [C#](AspNet35CSharp/README.md) [VB](AspNet35VisualBasic/README.md)

**More to come**

## More Information

[Chrome Updates](https://www.chromium.org/updates/same-site)

[ASP.NET Documentation](https://docs.microsoft.com/en-us/aspnet/samesite/system-web-samesite)

[.NET SameSite Patches](https://docs.microsoft.com/en-us/aspnet/samesite/kbs-samesite)