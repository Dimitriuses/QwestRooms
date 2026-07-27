using Autofac;
using Autofac.Integration.Mvc;
using QwestRoom.BLL.Services.Abstraction;
using QwestRoom.BLL.Services.Implementation;
using QwestRooms.DAL;
using QwestRooms.DAL.Repositories;
using System.Data.Entity;
using System.Web.Mvc;

namespace QwestRooms.UI.App_Start
{
    public class AutofacConfig
    {
        public static void Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // One context per request, so every repository injected into a single request's
            // services shares it.
            builder.RegisterType<RoomsContext>().As<DbContext>().InstancePerRequest();
            builder.RegisterGeneric(typeof(GenericRepository<>)).As(typeof(IGenericRepository<>));
            builder.RegisterType<RoomsService>().As<IRoomsService>();
            builder.RegisterType<AddressesService>().As<IAddressesService>();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(builder.Build()));
        }
    }
}
