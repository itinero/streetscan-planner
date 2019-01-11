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
        internal const string LocalRouterDb = "belgium.routerdb";
        
        /// <summary>
        /// Builds the belgium routerdb if it cannot be loaded.
        /// </summary>
        /// <returns></returns>
        public static RouterDb BuildRouterDb()
        {
            RouterDb routerDb = null;
            try
            {
                if (File.Exists(LocalRouterDb))
                {
                    using (var stream = File.OpenRead(LocalRouterDb))
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
                routerDb = new RouterDb();
                using (var stream = File.OpenRead(Download.Local))
                {
                    routerDb.LoadOsmData(stream, Vehicle.Car);
                }
            }

            if (!routerDb.HasContractedFor(Vehicle.Car.Fastest()))
            {
                routerDb.AddContracted(Vehicle.Car.Fastest());
                using (var stream = File.Open(LocalRouterDb, FileMode.Create))
                {
                    routerDb.Serialize(stream);
                }
            }

            return routerDb;
        }
    }
}