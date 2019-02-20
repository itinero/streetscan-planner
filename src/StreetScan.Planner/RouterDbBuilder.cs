using System;
using System.IO;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using Serilog;

namespace StreetScan.Planner
{
    internal static class RouterDbBuilder
    {
        //internal const string LocalRouterDb = "belgium.routerdb";
        
        /// <summary>
        /// Builds the belgium routerdb if it cannot be loaded.
        /// </summary>
        /// <returns></returns>
        public static RouterDb BuildRouterDb(string profileName)
        {
            var localRouterDb = $"belgium.{profileName}.routerdb";
            RouterDb routerDb = null;
            try
            {
                if (File.Exists(localRouterDb))
                {
                    using (var stream = File.OpenRead(localRouterDb))
                    {
                        routerDb = RouterDb.Deserialize(stream);
                    }
                }
            }
            catch (Exception e)
            {
                routerDb = null;
                Log.Warning($"Loading routerdb failed, rebuilding...");
            }

            if (routerDb == null)
            {
                Log.Information("Building routerdb...");

                // download data if needed.
                Download.DownloadAll();
                
                routerDb = new RouterDb();
                using (var stream = File.OpenRead(Download.Local))
                {
                    routerDb.LoadOsmData(stream, Vehicle.Car);
                }
            }

            var profile = routerDb.GetSupportedProfile(profileName);
            if (!routerDb.HasContractedFor(profile))
            {
                Log.Warning($"Routerdb doesn't have a contracted graph for {profile.FullName}...");
                routerDb.AddContracted(profile);
                using (var stream = File.Open(localRouterDb, FileMode.Create))
                {
                    routerDb.Serialize(stream);
                }
            }

            return routerDb;
        }
    }
}