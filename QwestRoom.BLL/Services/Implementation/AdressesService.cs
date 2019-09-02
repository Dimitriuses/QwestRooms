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
    class AdressesService : IAdressesService
    {
        private readonly IGenericRepository<Adress> addressRepos;

        public AdressesService(IGenericRepository<Adress> _addressRepos)
        {
            addressRepos = _addressRepos;
        }
        ICollection<AdressDTO> IAdressesService.GetAdresses()
        {

        }
    }
}
