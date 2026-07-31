using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using QwestRooms.Tests.Infrastructure;
using Xunit;

namespace QwestRooms.Tests;

/// <summary>
/// Registration and sign-in, driven through the real forms.
/// </summary>
/// <remarks>
/// In 2019 none of this worked: the account controller resolved its user manager in its
/// constructor, where HttpContext is still null, so every request threw; there was no login or
/// logout action at all; the password box rendered as a plain text field; and the anti-forgery
/// token the form emitted was never validated because no action asked for it.
/// </remarks>
public sealed class AccountEndpointTests : IClassFixture<CatalogueApplication>
{
    private readonly CatalogueApplication _application;

    public AccountEndpointTests(CatalogueApplication application) => _application = application;

    [Fact]
    public async Task Register_ThenSignedInUserIsNamedInTheNavigation()
    {
        using var client = CreateClient();
        var email = $"player-{Guid.NewGuid():N}@qwestrooms.example";

        var registration = await PostFormAsync(client, "/Account/Register", new Dictionary<string, string>
        {
            ["Login"] = email,
            ["Password"] = "Sesame!7",
            ["ConfirmPassword"] = "Sesame!7"
        });

        Assert.Equal(HttpStatusCode.Found, registration.StatusCode);
        Assert.Equal("/", registration.Headers.Location?.OriginalString);
        registration.Dispose();

        var catalogue = await client.GetStringAsync(new Uri("/", UriKind.Relative));
        Assert.Contains(email, catalogue, StringComparison.Ordinal);
        Assert.Contains("Log out", catalogue, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_WithTheWrongPassword_SaysNeitherWhichPartWasWrong()
    {
        using var client = CreateClient();
        var email = $"player-{Guid.NewGuid():N}@qwestrooms.example";

        (await PostFormAsync(client, "/Account/Register", new Dictionary<string, string>
        {
            ["Login"] = email,
            ["Password"] = "Sesame!7",
            ["ConfirmPassword"] = "Sesame!7"
        })).Dispose();

        using var signedOut = CreateClient();
        using var response = await PostFormAsync(signedOut, "/Account/Login", new Dictionary<string, string>
        {
            ["Login"] = email,
            ["Password"] = "NotThePassword!1"
        });

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Invalid email or password.", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Register_WithAWeakPassword_IsRefusedWithAReason()
    {
        using var client = CreateClient();

        using var response = await PostFormAsync(client, "/Account/Register", new Dictionary<string, string>
        {
            ["Login"] = $"player-{Guid.NewGuid():N}@qwestrooms.example",
            ["Password"] = "password",
            ["ConfirmPassword"] = "password"
        });

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text-danger", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Register_WithoutTheAntiforgeryToken_IsRejected()
    {
        using var client = CreateClient();

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Login"] = $"player-{Guid.NewGuid():N}@qwestrooms.example",
            ["Password"] = "Sesame!7",
            ["ConfirmPassword"] = "Sesame!7"
        });

        using var response = await client.PostAsync(new Uri("/Account/Register", UriKind.Relative), content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PasswordFields_AreRenderedAsPasswordInputs()
    {
        using var client = CreateClient();

        var html = await client.GetStringAsync(new Uri("/Account/Register", UriKind.Relative));

        // In 2019 the password box was an ordinary text input, so the password was typed in the
        // clear and left in the browser's form history.
        Assert.Matches(PasswordInput("Password"), html);
        Assert.Matches(PasswordInput("ConfirmPassword"), html);
    }

    /// <summary>Matches an input carrying this id, whatever order the tag helper emits attributes in.</summary>
    private static Regex PasswordInput(string id) =>
        new($"<input(?=[^>]*id=\"{id}\")[^>]*type=\"password\"", RegexOptions.None, TimeSpan.FromSeconds(5));

    private HttpClient CreateClient() =>
        _application.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    /// <summary>Fetches the form, reads its anti-forgery token, and posts the fields back with it.</summary>
    private static async Task<HttpResponseMessage> PostFormAsync(
        HttpClient client,
        string path,
        Dictionary<string, string> fields)
    {
        var page = await client.GetStringAsync(new Uri(path, UriKind.Relative));
        var token = Regex.Match(
            page,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        Assert.True(token.Success, $"{path} did not render an anti-forgery token");

        fields["__RequestVerificationToken"] = token.Groups[1].Value;
        using var content = new FormUrlEncodedContent(fields);
        return await client.PostAsync(new Uri(path, UriKind.Relative), content);
    }
}
