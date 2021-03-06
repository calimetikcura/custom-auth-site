# kCura Relativity Authentication Bridge

The kCura Relativity Authentication Bridge allows you to determine how users authenticate to Relativity.
This is typically needed when you already have a custom authentication solution for your users or have some other custom authentication requirement that is beyond what Relativity supports.

## Overview

The kCura Relativity Authentication Bridge is designed as a sample application that you can clone and then modify according to your requirements.

The sample is based on [ASP.NET Core and MVC](https://docs.microsoft.com/en-us/aspnet/core/). 
It will be hosted and execute as its own web application (just as any other ASP.NET Core application would; [see here for more details on hosting ASP.NET Core web application](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/)).
You program the ASP.NET Core application logic to integrate with your custom authentication solution.
The outcome of this authentication is conveyed to Relativity using the OpenID Connect authentication protocol.

To provide the OpenID Connect support, the Relativity Authentication Bridge uses an open source framework called [IdentityServer](http://identityserver.io/).
The IdentityServer framework is responsible for flowing the identity of the user to Relativity.

## Workflow

The authentication workflow follows these steps:

1. An unauthenticated user vists Relativity.
1. Relativity sends the user to login (TODO: how does HRD work, or is the bridge the only IdP configured?).
1. Relativity determines that the user must be authenticated by the Relativity Authentication Bridge.
1. Relativity makes an OpenID Connect authentication request to the Relativity Authentication Bridge.
1. Internally at the Relativity Authentication Bridge, IdentityServer processes the OpenID Connect request and must have the user authenticate with one of the authentication scenarios described below.
1. The custom code in the Relativity Authentication Bridge authenticates the user and obtains the user id.
1. The user id is conveyed to IdentityServer.
1. IdentityServer generates an OpenID Connect authentication response to Relativity.
1. Relativity receives the OpenID Connect authentication response from the Relativity Authentication Bridge and maps the user id to the corresponding user in Relativity.
1. If successful, the user is now logged in at Relativity.

## User Id

The user id that the Relativity Authentication Bridge issues must be unique, consistent, and never reused for any other user.

## Authentication Scenarios

There are two custom authentication scenarios that the Relativity Authentication Bridge is designed to support:

* Interactive Login
* HTTP-based Login

Only one scenario may be enabled at a time. The Relativity Authentication Bridge defaults to the interactive login scenario.

### Interactive Login

Interactive login is designed to display a custom login form to the user. 
This custom login form will then accept and validate the user's credentials.
If successful, then the login form will issue a cookie that contains the user's id. 
IdentityServer uses this cookie to then send a OpenID Connect authentication response to Relativity.

#### Custom Code for Interactive Login

The code for the interactive login is in the `AccountController` in the sample code (in ~/Controllers/AccountController.cs).

There is a `Login` action method that displays the login view, and then posts back to another `Login` action method that accepts the username and password in the `LoginModel`. 
Your custom logic would replace the default authentication check that's being performed in the sample.
Once your custom logic has authenticated the user you must invoke `await HttpContext.Authentication.SignInAsync` to issue the cookie for IdentityServer. 
The first parameter is the user id, and the second parameter is intended for the user's display name but is unused in this sample.
Once the cookie has been issued, the user is then redirected back into IdentityServer (after checking that the URL is valid via the call to `IsValidReturnUrl`).

### HTTP-based Login

The HTTP-based login is designed for the scenario when the user authentication has already been performed elsewhere and a value in the HTTP request is used to identify the user.
Custom logic is then written to determine the user id from the HTTP request.
This user id is then presented to IdentityServer to send a OpenID Connect authentication response to Relativity.

#### Custom Code for HTTP-based Login

The code for the HTTP-based login is a simple callback function that accepts as an input the `HttpContect` and returns a `string` for the user id for the authenticated user (or `null` if the callback can't determine the user from the HTTP request).

This callback function is configured via the `HttpLoginCallback` property on the `RelativityAuthenticationBridgeOptions` class. 
This class is used during application startup to configure IdentityServer. 
This configuration occurs in the `ConfigureServices` method in the `Startup` class (in ~/Startup.cs).
If `HttpLoginCallback` is set, then the interactive login will be disabled and only HTTP-based logins will be allowed.

Note that the `HttpLoginCallback` returns `Task<string>`, thus it supports asychronous operations.