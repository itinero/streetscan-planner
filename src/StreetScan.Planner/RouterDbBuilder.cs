using System;
using System.IO;
using Itinero;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using OsmSharp.Streams;
using Serilog;

namespace StreetScan.Planner
{
    internal static class RouterDbBuilder
    {
        internal static string GetOsmData(string name, string outputPath, Box box)
        {
            // generate the cutout file name.
            var cutoutFile = new FileInfo(Path.Combine(outputPath, $"{name}.osm")).FullName;
            
            // if it exists, don't rebuild it, it could have been edited.
            if (File.Exists(cutoutFile))
            {
                Log.Information("Using existing OSM extract: {CutoutFile}",
                    cutoutFile);
                return cutoutFile;
            }
            
            // make sure the source data has been downloaded.
            Download.DownloadAll();
            using var sourceDataStream = File.OpenRead(Download.Local);

            // cut out the data.
            var tempFile = "temp.osm";
            using (var cutoutFileStream = File.Open(tempFile, FileMode.Create))
            {
                Log.Warning("OSM extract doesn't exist for file {Name}, creating {CutoutFile}",
                    name, cutoutFile);
                var source = new PBFOsmStreamSource(sourceDataStream); 
                
                var filtered = source.FilterBox( box.MinLon, box.MaxLat, box.MaxLon, box.MinLat, true);

                var target = new XmlOsmStreamTarget(cutoutFileStream);
                target.RegisterSource(filtered);
                target.Initialize();
                target.Pull();
            }
  
            File.Copy(tempFile, cutoutFile);
            return cutoutFile;
        }
        
        internal static RouterDb BuildRouterDb(string osmData, string profileName)
        {
            var localRouterDb = $"{osmData}.{profileName}.routerdb";
            localRouterDb = new FileInfo(localRouterDb).FullName;
            
            // try to load existing router db.
            RouterDb routerDb = null;
            try
            {
                if (File.Exists(localRouterDb))
                {
                    // if source file is more recent rebuild router db.
                    if (new FileInfo(localRouterDb).LastWriteTime < new FileInfo(osmData).LastWriteTime)
                    {
                        Log.Warning("Router db is older than source data, rebuilding");
                    }
                    else
                    {
                        using (var stream = File.OpenRead(localRouterDb))
                        {
                            routerDb = RouterDb.Deserialize(stream);
                        }
                    
                        Log.Information("Using existing router db: {LocalRouterDb}",
                            localRouterDb);
                    }
                }
            }
            catch (Exception e)
            {
                routerDb = null;
                Log.Error(e, "Loading router db failed, rebuilding...");
            }

            // build router db if needed.
            if (routerDb == null)
            {
                Log.Information("Building router db from: {LocalFile}",
                    osmData);
                
                // create new router db.
                routerDb = new RouterDb();
                using (var stream = File.OpenRead(osmData))
                {
                    OsmStreamSource streamSource;
                    if (Path.GetExtension(osmData).EndsWith("pbf"))
                    {
                        streamSource = new PBFOsmStreamSource(stream);
                    }
                    else
                    {
                        streamSource = new XmlOsmStreamSource(stream);
                    }
                    routerDb.LoadOsmData(streamSource, StreetScan.Planner.Profiles.Vehicles.Car());
                }
            }

            // add contraction data if needed.
            if (!routerDb.SupportProfile(profileName))
            {
                Log.Error("Profile not supported: {ProfileName}", 
                    profileName);
                return null;
            }
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