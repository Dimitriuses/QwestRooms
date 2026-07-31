using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using QwestRooms.Tests.Infrastructure;
using Xunit;

namespace QwestRooms.Tests;

/// <summary>
/// End-to-end tests over the running application: real routing, real Razor, real database.
/// </summary>
public sealed class CatalogueEndpointTests : IClassFixture<CatalogueApplication>
{
    private const int PageSize = 27;

    private readonly CatalogueApplication _application;

    public CatalogueEndpointTests(CatalogueApplication application) => _application = application;

    [Fact]
    public async Task Root_ServesTheCatalogue_WithAFullPageOfCards()
    {
        using var client = _application.CreateClient();

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(PageSize, CountCards(html));
        Assert.Contains("450 rooms", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Root_IsServedByTheRoomController_WithoutAnExplicitRoute()
    {
        using var client = _application.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // The 2019 default route named a controller that did not exist, so "/" was a 404 for the
        // entire life of the project.
        using var response = await client.GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SecondPage_ShowsDifferentRoomsFromTheFirst()
    {
        using var client = _application.CreateClient();

        var first = await client.GetStringAsync(new Uri("/Room/Grid?page=1", UriKind.Relative));
        var second = await client.GetStringAsync(new Uri("/Room/Grid?page=2", UriKind.Relative));

        Assert.Equal(PageSize, CountCards(first));
        Assert.Equal(PageSize, CountCards(second));
        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task FilterAndPaging_Compose()
    {
        using var client = _application.CreateClient();

        // Ukraine has 30 rooms, so a filtered list is two pages: 27 and 3. In 2019 the filter
        // lived in Session and the pager did not carry it, so this second page came back with all
        // 450 rooms in it.
        var firstPage = await client.GetStringAsync(new Uri("/Room/Grid?countryId=1&page=1", UriKind.Relative));
        var secondPage = await client.GetStringAsync(new Uri("/Room/Grid?countryId=1&page=2", UriKind.Relative));

        Assert.Equal(30, TotalCount(firstPage));
        Assert.Equal(30, TotalCount(secondPage));
        Assert.Equal(PageSize, CountCards(firstPage));
        Assert.Equal(3, CountCards(secondPage));
    }

    [Fact]
    public async Task Filter_NarrowsToTheChosenCountry()
    {
        using var client = _application.CreateClient();

        var html = await client.GetStringAsync(new Uri("/Room/Grid?countryId=1", UriKind.Relative));

        Assert.Equal(30, TotalCount(html));
        Assert.Contains("Ukraine", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Warsaw", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CascadingFilter_OffersOnlyTheCitiesOfTheChosenCountry()
    {
        using var client = _application.CreateClient();

        var cities = await client.GetStringAsync(new Uri("/Room/Cities?countryId=1", UriKind.Relative));

        Assert.Contains("Kyiv", cities, StringComparison.Ordinal);
        Assert.DoesNotContain("Warsaw", cities, StringComparison.Ordinal);

        // Each city is offered once, however many addresses it holds.
        Assert.Single(Regex.Matches(cities, ">Kyiv<", RegexOptions.None, TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task Health_ReportsTheSeededRoomCount()
    {
        using var client = _application.CreateClient();

        var json = await client.GetStringAsync(new Uri("/healthz", UriKind.Relative));
        using var document = JsonDocument.Parse(json);

        Assert.Equal("ok", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(450, document.RootElement.GetProperty("rooms").GetInt32());
    }

    [Fact]
    public async Task Posters_AreServedFromThisApplication()
    {
        using var client = _application.CreateClient();

        using var response = await client.GetAsync(new Uri("/img/rooms/bunker.svg", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/svg+xml", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Catalogue_LoadsNoThirdPartyAssets()
    {
        using var client = _application.CreateClient();

        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        // The 2019 layout pulled three CDNs, one of them rawgit.com, which shut down in 2019.
        Assert.DoesNotContain("//cdn", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http://", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("https://", html, StringComparison.OrdinalIgnoreCase);
    }

    private static int CountCards(string html) =>
        Regex.Matches(html, "class=\"card flip-card\"", RegexOptions.None, TimeSpan.FromSeconds(5)).Count;

    private static int TotalCount(string html)
    {
        var match = Regex.Match(html, "data-total-count=\"(\\d+)\"", RegexOptions.None, TimeSpan.FromSeconds(5));
        Assert.True(match.Success, "the grid did not render a pager carrying its total count");
        return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    }
}
