using System.Collections.Generic;
using System.IO;
using Itinero.LocalGeo;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace StreetScan.Planner.GeoJson
{
    public static class GeoJsonReader1
    {
        /// <summary>
        /// Reads the GeoJson file.
        /// </summary>
        /// <param name="geoJsonFile"></param>
        /// <returns></returns>
        public static IEnumerable<Coordinate> Read(string geoJsonFile)
        {
            using (var stream = File.OpenRead(geoJsonFile))
            using (var textStream = new StreamReader(stream))
            using (var jsonTextStream = new JsonTextReader(textStream))
            {
                var features = NetTopologySuite.IO.GeoJsonSerializer.CreateDefault().Deserialize<FeatureCollection>(jsonTextStream);

                foreach (var feature in features.Features)
                {
                    if (feature?.Geometry == null) continue;
                    if (feature.Geometry is Point p)
                    {
                        yield return new Coordinate((float)p.Coordinate.Y, (float)p.Coordinate.X);
                    }
                }
            }
        }
    }
}