using System;
using System.IO;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using OsmSharp.Streams;
using Serilog;

namespace StreetScan.Planner
{
    internal static class RouterDbBuilder
    {
        internal static RouterDb BuildRouterDb(string profileName, string customInputFile = null)
        {
            var localRouterDb = $"belgium.{profileName}.routerdb";
            if (!string.IsNullOrWhiteSpace(customInputFile))
            {
                localRouterDb = $"{Path.GetFileNameWithoutExtension(customInputFile)}.{profileName}.routerdb";
                Log.Information("Using custom input file {CustomInputFile} to build or load {LocalRouterDb}",
                    customInputFile, localRouterDb);
            }
            
            // try to load existing router db.
            RouterDb routerDb = null;
            try
            {
                if (File.Exists(localRouterDb))
                {
                    using (var stream = File.OpenRead(localRouterDb))
                    {
                        routerDb = RouterDb.Deserialize(stream);
                    }
                    
                    Log.Information("Using existing router db: {LocalRouterDb}",
                        localRouterDb);
                }
            }
            catch (Exception e)
            {
                routerDb = null;
                Log.Warning("Loading router db failed, rebuilding...");
            }

            // build router db if needed.
            if (routerDb == null)
            {
                // download data if needed.
                var inputFile = customInputFile;
                if (string.IsNullOrWhiteSpace(customInputFile))
                {
                    Download.DownloadAll();
                    inputFile = Download.Local;
                }
                
                Log.Information("Building router db from: {LocalFile}",
                    customInputFile);
                
                // create new router db.
                routerDb = new RouterDb();
                using (var stream = File.OpenRead(inputFile))
                {
                    OsmStreamSource streamSource;
                    if (Path.GetExtension(inputFile).EndsWith("pbf"))
                    {
                        streamSource = new PBFOsmStreamSource(stream);
                    }
                    else
                    {
                        streamSource = new XmlOsmStreamSource(stream);
                    }
                    routerDb.LoadOsmData(streamSource, Vehicle.Car);
                }
            }

            // add contraction data if needed.
            var profile = routerDb.GetSupportedProfile(profileName);
            if (!routerDb.HasContractedFor(profile))
            {
                Log.Warning("Building contracted graph for {Profile}",
                    profile.FullName);
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