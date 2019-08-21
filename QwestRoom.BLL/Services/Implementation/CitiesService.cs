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
    public class CitiesService : ICitiesService
    {
        private readonly IGenericRepository<City> cityRepos;

        public CitiesService(IGenericRepository<City> _citiRepos)
        {
            cityRepos = _citiRepos;
        }
        public ICollection<CityDTO> GetCities()
        {
            var Cities = cityRepos.GetAll();
            List<CityDTO> cityDTOs = new List<CityDTO>();
            foreach (var item in Cities)
            {
                cityDTOs.Add(new CityDTO { Id = item.Id, Name = item.Name });

            }
            return cityDTOs;
        }
    }
}
