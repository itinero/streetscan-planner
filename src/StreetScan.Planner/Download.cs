using System.IO;
using System.Net;
using Serilog;

namespace StreetScan.Planner
{
    /// <summary>
    /// Downloads all data.
    /// </summary>
    internal static class Download
    {
        internal const string PBF = "http://planet.anyways.eu/planet/europe/belgium/belgium-latest.osm.pbf";
        internal const string Local = "belgium-latest.osm.pbf";
        
        /// <summary>
        /// Downloads the luxembourg data.
        /// </summary>
        public static void DownloadAll()
        {
            if (File.Exists(Download.Local)) return;
            
            Log.Information("Downloading Belgium OSM data...");
            var client = new WebClient();
            client.DownloadFile(Download.PBF,
                Download.Local);
        }
    }
}
