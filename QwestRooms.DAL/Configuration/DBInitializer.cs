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
            "MockData/Citys.sql"
        };

        protected override void Seed(RoomsContext context)
        {
            foreach(var f in SQLs)
            {
                context.Database.ExecuteSqlCommand(GetSQLFromFile(f));
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
