using QwestRooms.BLL.Filtering;
using QwestRooms.BLL.Services.Abstraction;
using QwestRooms.UI.Models;
using System.Web.Mvc;

namespace QwestRooms.UI.Controllers
{
    public class RoomController : Controller
    {
        private const int PageSize = 27;

        private readonly IRoomsService roomsService;
        private readonly IAddressesService addressesService;

        public RoomController(IRoomsService _roomsService, IAddressesService _addressesService)
        {
            roomsService = _roomsService;
            addressesService = _addressesService;
        }

        /// <summary>The full catalogue page.</summary>
        public ActionResult Index(RoomFilterViewModel filter, int page = 1)
        {
            var model = new RoomCatalogViewModel
            {
                List = BuildRoomList(filter, page),
                Countries = addressesService.GetCountries()
            };

            return View("Page", model);
        }

        /// <summary>
        /// The rooms grid on its own, for AJAX updates. Both paging and filtering come through
        /// here, which is why they now compose: the criteria travel in the query string instead
        /// of living in Session, so a page link inside a filtered list keeps the filter.
        /// </summary>
        public ActionResult RoomsPartial(RoomFilterViewModel filter, int page = 1)
        {
            return PartialView("RoomsCollectionView", BuildRoomList(filter, page));
        }

        public ActionResult CitiesPartial(int countryId)
        {
            return PartialView("CitiesCollectionView", addressesService.GetCitiesByCountry(countryId));
        }

        public ActionResult AddressesPartial(int countryId, int cityId)
        {
            return PartialView("AddressCollectionView",
                addressesService.GetAddressesByCountryAndCity(countryId, cityId));
        }

        private RoomListViewModel BuildRoomList(RoomFilterViewModel filter, int page)
        {
            if (filter == null)
            {
                filter = new RoomFilterViewModel();
            }

            var result = roomsService.GetRooms(
                new RoomFilter
                {
                    CountryId = filter.CountryId,
                    CityId = filter.CityId,
                    AddressId = filter.AddressId
                },
                page,
                PageSize);

            return new RoomListViewModel
            {
                Rooms = result.Items,
                Page = new PageViewModel(result.TotalCount, result.PageNumber, result.PageSize),
                Filter = filter
            };
        }
    }
}
