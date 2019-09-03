using DataAccessLayer.Repositories;
using QwestRoom.BLL.DTOModels;
using QwestRoom.BLL.Services.Abstraction;
using QwestRooms.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwestRoom.BLL.Services.Implementation
{
    public class AdressesService : IAdressesService
    {
        private readonly IGenericRepository<Adress> addressRepos;

        public AdressesService(IGenericRepository<Adress> _addressRepos)
        {
            addressRepos = _addressRepos;
        }
        ICollection<AdressDTO> IAdressesService.GetAdresses()
        {
            var Adresses = addressRepos.GetAll().ToList();
            List<AdressDTO> adressDTOs = new List<AdressDTO>();
            foreach (var item in Adresses)
            {
                adressDTOs.Add(new AdressDTO
                {
                    Id = item.Id,
                    HouseNumber = item.HouseNumber,
                    Country = new CountryDTO
                    {
                        Id = item.Country.Id,
                        Name = item.Country.Name
                    },
                    City = new CityDTO
                    {
                        Id = item.City.Id,
                        Name = item.City.Name
                    },
                    Street = new StreetDTO
                    {
                        Id = item.Street.Id,
                        Name = item.Street.Name
                    }
                });
            }
            return adressDTOs;
        }
    }
}
