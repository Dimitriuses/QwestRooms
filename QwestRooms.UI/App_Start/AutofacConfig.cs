using Autofac;
using Autofac.Integration.Mvc;
using DataAccessLayer.Repositories;
using QwestRoom.BLL.Services.Abstraction;
using QwestRoom.BLL.Services.Implementation;
using QwestRooms.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QwestRooms.UI.App_Start
{
    public class AutofacConfig
    {
        public static void Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.RegisterType<RoomsContext>().As<DbContext>();
            builder.RegisterGeneric(typeof(GenericRepository<>)).As(typeof(IGenericRepository<>));
            builder.RegisterType<CitiesService>().As<ICitiesService>();
            builder.RegisterType<RoomsService>().As<IRoomsService>();


            DependencyResolver.SetResolver(new AutofacDependencyResolver(builder.Build()));
        }
    }
}