using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Text;

namespace QwestRooms.DAL.Configuration
{
    /// <summary>
    /// Creates the database and loads the demo dataset from the MockData SQL scripts.
    /// <para>
    /// Uses <see cref="DropCreateDatabaseIfModelChanges{TContext}"/> rather than
    /// DropCreateDatabaseAlways: the demo data survives an ordinary application restart, but a
    /// stale database left over from an older version of the model is rebuilt automatically.
    /// </para>
    /// </summary>
    public class RoomsDbInitializer : DropCreateDatabaseIfModelChanges<RoomsContext>
    {
        /// <summary>
        /// Script file names, ordered so that a table's dependencies are always populated first
        /// (addresses need cities/countries/streets, rooms need addresses/companies, and so on).
        /// The scripts insert without explicit primary keys and rely on identity values starting
        /// at 1, so the order here is load-bearing.
        /// </summary>
        private static readonly string[] ScriptFiles =
        {
            "Cities.sql",
            "Countries.sql",
            "Streets.sql",
            "Addresses.sql",
            "Companies.sql",
            "Rooms.sql",
            "Images.sql"
        };

        protected override void Seed(RoomsContext context)
        {
            var scriptDirectory = ResolveScriptDirectory();

            foreach (var fileName in ScriptFiles)
            {
                var fullPath = Path.Combine(scriptDirectory, fileName);
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException(
                        "Seed script not found. Expected the MockData folder to be copied to the " +
                        "application output directory; check that the .sql files are marked as " +
                        "Content with CopyToOutputDirectory=Always.", fullPath);
                }

                // The scripts are UTF-8 without a BOM and contain no GO batch separators, so the
                // whole file can be handed to ExecuteSqlCommand in one go.
                var sql = File.ReadAllText(fullPath, Encoding.UTF8);
                context.Database.ExecuteSqlCommand(sql);
            }

            base.Seed(context);
        }

        /// <summary>
        /// Finds the MockData folder. Under ASP.NET, AppDomain.BaseDirectory is the site root and
        /// the content lands in bin\MockData; under a plain console or test host the base
        /// directory *is* the output folder, so MockData sits directly beneath it. The original
        /// implementation hard-coded the first case with a "/bin/..." string concatenation, which
        /// broke anywhere outside the web host.
        /// </summary>
        private static string ResolveScriptDirectory()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var candidates = new List<string>
            {
                Path.Combine(baseDirectory, "bin", "MockData"),
                Path.Combine(baseDirectory, "MockData")
            };

            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new DirectoryNotFoundException(
                "Could not locate the MockData folder. Looked in: " + string.Join("; ", candidates));
        }
    }
}
