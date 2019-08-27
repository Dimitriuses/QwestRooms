using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.IO;

namespace QwestRooms.DAL.Configuration
{
    class DBinicialaiser : DropCreateDatabaseAlways<RoomsContext>
    {
        public List<string> SQLs = new List<string>()
        {
            "/bin/MockData/Cities (fix).sql",
            "/bin/MockData/Countries (fix).sql",
            "/bin/MockData/Streets (fix).sql",
            "/bin/MockData/Adresses(fix).sql",
            "/bin/MockData/Companies (fix).sql",
            "/bin/MockData/Rooms (fix).sql",
            "/bin/MockData/Images (fix).sql"
        };

        protected override void Seed(RoomsContext context)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            foreach(var f in SQLs)
            {
                context.Database.ExecuteSqlCommand(GetSQLFromFile(path+f));
            }
        }

        private string GetSQLFromFile(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                return sr.ReadToEnd();
            }
        }

    }
}
