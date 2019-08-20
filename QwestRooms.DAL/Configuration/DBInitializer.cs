using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace QwestRooms.DAL.Configuration
{
    class DBinicialaiser : DropCreateDatabaseAlways<RoomsContext>
    {
        protected override void Seed(RoomsContext context)
        {
            
        }
    }
}
