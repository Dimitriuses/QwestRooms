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
    public class RoomsService: IRoomsService
    {
        private readonly IGenericRepository<Room> roomRepos;

        //private readonly IMapper mapper;

        public RoomsService(IGenericRepository<Room> _roomRepos)//, IMapper _mapper)
        {
            roomRepos = _roomRepos;
            //mapper = _mapper;
        }

        public ICollection<RoomDTO> GetRooms()
        {
            var rooms = roomRepos.GetAll().ToList();
            List<RoomDTO> roomDTOs = new List<RoomDTO>();
            foreach (var item in rooms)
            {
                List<ImageDTO> imageDTOs = new List<ImageDTO>();
                foreach (var item1 in item.Images)
                {
                    imageDTOs.Add(new ImageDTO
                    {
                        Id = item1.Id,
                        Path = item1.Path
                    });
                }
                roomDTOs.Add(new RoomDTO
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    TimeToPass = item.TimeToPass,
                    MinPayers = item.MinPayers,
                    MaxPlayers = item.MaxPlayers,
                    Phone = item.Phone,
                    Email = item.Email,
                    Rating = item.Rating,
                    FearLevel = item.FearLevel,
                    Diffictly = item.Diffictly,
                    LogoPath = item.LogoPath,
                    Company = new CompanyDTO
                    {
                        Id = item.Company.Id,
                        Name = item.Company.Name
                    },
                    Adress = new AdressDTO
                    {
                        Id = item.Adress.Id,
                        HouseNumber = item.Adress.HouseNumber,
                        Country = new CountryDTO
                        {
                            Id = item.Adress.Country.Id,
                            Name = item.Adress.Country.Name
                        },
                        City = new CityDTO
                        {
                            Id = item.Adress.City.Id,
                            Name = item.Adress.City.Name
                        },
                        Street = new StreetDTO
                        {
                            Id = item.Adress.Street.Id,
                            Name = item.Adress.Street.Name
                        }
                    },
                    Images = imageDTOs
                });
            }

            return roomDTOs;

            //return //mapper.Map<List<Room>, ICollection<RoomDTO>>(rooms);


        }
    }
}
