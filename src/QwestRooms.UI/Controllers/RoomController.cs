using Microsoft.AspNetCore.Mvc;
using QwestRooms.BLL.Services.Abstraction;
using QwestRooms.UI.Models;

namespace QwestRooms.UI.Controllers;

public sealed class RoomController(IRoomsService roomsService, IAddressesService addressesService) : Controller
{
    private const int PageSize = 27;

    private readonly IRoomsService _roomsService = roomsService;
    private readonly IAddressesService _addressesService = addressesService;

    /// <summary>The full catalogue page.</summary>
    public async Task<IActionResult> Index(
        RoomFilterViewModel filter,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var model = new RoomCatalogViewModel
        {
            List = await BuildRoomListAsync(filter, page, cancellationToken).ConfigureAwait(false),
            Countries = await _addressesService.GetCountriesAsync(cancellationToken).ConfigureAwait(false)
        };

        return View(model);
    }

    /// <summary>
    /// The rooms grid on its own, for the in-page updates the filter and pager make.
    /// </summary>
    /// <remarks>
    /// Both paging and filtering arrive here as query-string criteria, which is why they compose:
    /// in 2019 the selection lived in <c>Session</c>, so a page link inside a filtered list quietly
    /// dropped the filter and two browser tabs fought over one server-side selection.
    /// </remarks>
    public async Task<IActionResult> Grid(
        RoomFilterViewModel filter,
        int page = 1,
        CancellationToken cancellationToken = default) =>
        PartialView("_RoomGrid", await BuildRoomListAsync(filter, page, cancellationToken).ConfigureAwait(false));

    public async Task<IActionResult> Cities(int countryId, CancellationToken cancellationToken = default) =>
        PartialView(
            "_CityOptions",
            await _addressesService.GetCitiesByCountryAsync(countryId, cancellationToken).ConfigureAwait(false));

    public async Task<IActionResult> Addresses(
        int countryId,
        int cityId,
        CancellationToken cancellationToken = default) =>
        PartialView(
            "_AddressOptions",
            await _addressesService
                .GetAddressesByCountryAndCityAsync(countryId, cityId, cancellationToken)
                .ConfigureAwait(false));

    private async Task<RoomListViewModel> BuildRoomListAsync(
        RoomFilterViewModel? filter,
        int page,
        CancellationToken cancellationToken)
    {
        filter ??= new RoomFilterViewModel();

        var result = await _roomsService
            .GetRoomsAsync(filter.ToFilter(), page, PageSize, cancellationToken)
            .ConfigureAwait(false);

        return new RoomListViewModel { Page = result, Filter = filter };
    }
}
