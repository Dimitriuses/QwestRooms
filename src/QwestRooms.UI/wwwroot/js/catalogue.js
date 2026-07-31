// The catalogue's filter and pager, in plain DOM APIs.
//
// The 2019 page did this with jQuery, jquery.unobtrusive-ajax and a jQuery flip plugin, and kept
// the selected country/city/address in the ASP.NET Session -- so two browser tabs shared one
// filter and a pager link inside a filtered list silently dropped it. The state lives in this
// closure now, and every request carries it in the query string.
(function () {
    'use strict';

    var endpointsElement = document.getElementById('catalogueEndpoints');
    var roomsElement = document.getElementById('rooms');
    if (!endpointsElement || !roomsElement) {
        return;
    }

    var endpoints = JSON.parse(endpointsElement.textContent);
    var filter = { countryId: null, cityId: null, addressId: null };

    var buttons = {
        country: document.getElementById('countryButton'),
        city: document.getElementById('cityButton'),
        address: document.getElementById('addressButton')
    };

    var menus = {
        country: document.getElementById('countryMenu'),
        city: document.getElementById('cityMenu'),
        address: document.getElementById('addressMenu')
    };

    function query(extra) {
        var parameters = new URLSearchParams();
        Object.keys(extra).forEach(function (key) {
            if (extra[key] !== null && extra[key] !== undefined) {
                parameters.set(key, extra[key]);
            }
        });
        return parameters.toString();
    }

    function fetchHtml(url, target) {
        return fetch(url, { headers: { 'X-Requested-With': 'fetch' } })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error(url + ' returned ' + response.status);
                }
                return response.text();
            })
            .then(function (html) {
                target.innerHTML = html;
                return target;
            });
    }

    function loadRooms(page) {
        var url = endpoints.grid + '?' + query({
            page: page || 1,
            countryId: filter.countryId,
            cityId: filter.cityId,
            addressId: filter.addressId
        });

        return fetchHtml(url, roomsElement).then(function () {
            var pager = roomsElement.querySelector('[data-total-count]');
            var count = document.getElementById('resultCount');
            if (pager && count) {
                count.textContent = pager.getAttribute('data-total-count') + ' rooms';
            }
        });
    }

    function reset(level) {
        filter[level + 'Id'] = null;
        buttons[level].textContent = 'Any';
        buttons[level].disabled = level !== 'country';
        menus[level].innerHTML = '';
    }

    menus.country.addEventListener('click', function (event) {
        var option = event.target.closest('.js-country');
        if (!option) {
            return;
        }

        filter.countryId = option.getAttribute('data-id');
        buttons.country.textContent = option.textContent.trim();
        reset('city');
        reset('address');

        fetchHtml(endpoints.cities + '?' + query({ countryId: filter.countryId }), menus.city)
            .then(function () {
                buttons.city.disabled = false;
            });
    });

    menus.city.addEventListener('click', function (event) {
        var option = event.target.closest('.js-city');
        if (!option) {
            return;
        }

        filter.cityId = option.getAttribute('data-id');
        buttons.city.textContent = option.textContent.trim();
        reset('address');

        var url = endpoints.addresses + '?' + query({
            countryId: filter.countryId,
            cityId: filter.cityId
        });

        fetchHtml(url, menus.address).then(function () {
            buttons.address.disabled = false;
        });
    });

    menus.address.addEventListener('click', function (event) {
        var option = event.target.closest('.js-address');
        if (!option) {
            return;
        }

        filter.addressId = option.getAttribute('data-id');
        buttons.address.textContent = option.textContent.trim();
    });

    document.getElementById('applyFilter').addEventListener('click', function () {
        loadRooms(1);
    });

    document.getElementById('clearFilter').addEventListener('click', function () {
        filter = { countryId: null, cityId: null, addressId: null };
        buttons.country.textContent = 'Any';
        reset('city');
        reset('address');
        loadRooms(1);
    });

    // The pager is re-rendered with every page of results, so bind by delegation. The links are
    // real hrefs pointing at the full page, which is what makes the pager work with scripting
    // disabled; this handler upgrades them to an in-place update.
    roomsElement.addEventListener('click', function (event) {
        var link = event.target.closest('.js-page-link');
        if (!link) {
            return;
        }

        event.preventDefault();
        loadRooms(link.getAttribute('data-page'));
    });
})();
