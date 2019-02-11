using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Itinero.LocalGeo;

namespace StreetScan.Planner.CSV
{
    internal static class CSVReader
    {
        /// <summary>
        /// Reads the CSV file.
        /// </summary>
        /// <param name="csvFile"></param>
        /// <returns></returns>
        public static IEnumerable<Coordinate> Read(string csvFile)
        {
            var l = 0;
            using (var reader = new StreamReader(csvFile))
            {
                int? latitudeColumn = null, longitudeColumn = null;
                var line = reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    l++;
                    var data = line.Split(',').ToList();
                    if (latitudeColumn == null)
                    {
                        latitudeColumn = data.IndexOf("LAT");
                        longitudeColumn = data.IndexOf("LON");
                        line = reader.ReadLine();
                        continue;
                    }

                    if (!float.TryParse(data[latitudeColumn.Value], NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var lat))
                    {
                        line = reader.ReadLine();
                        continue;   
                    }
                    if (!float.TryParse(data[longitudeColumn.Value], NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var lon))
                    {
                        line = reader.ReadLine();
                        continue;   
                    }

                    line = reader.ReadLine();
                    yield return new Coordinate(lat, lon);
                }
            }
        }
    }
}