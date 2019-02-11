using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Itinero;
using Itinero.Geo;
using Itinero.LocalGeo;
using Itinero.Optimization;
using Itinero.Optimization.Models.Mapping;
using Itinero.Optimization.Models.Mapping.Directed.Simplified;
using NetTopologySuite.IO;
using Serilog;

namespace StreetScan.Planner
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            if (args == null || args.Length < 1)
            { // show help.

                return;
            }
            if (args[0] == "test")
            {
                args = new[]
                {
                    "test.csv",
                    "test.gpx"
                };
            }
            
            // enable logging.
            OsmSharp.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                var formattedMessage = $"{origin} - {message}";
                switch (level)
                {
                    case "critical":
                        Log.Fatal(formattedMessage);
                        break;
                    case "error":
                        Log.Error(formattedMessage);
                        break;
                    case "warning":
                        Log.Warning(formattedMessage);
                        break; 
                    case "verbose":
                        Log.Verbose(formattedMessage);
                        break; 
                    case "information":
                        Log.Information(formattedMessage);
                        break; 
                    case "debug":
                        Log.Debug(formattedMessage);
                        break;
                }
            };
            Itinero.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                var formattedMessage = $"{origin} - {message}";
                switch (level)
                {
                    case "critical":
                        Log.Fatal(formattedMessage);
                        break;
                    case "error":
                        Log.Error(formattedMessage);
                        break;
                    case "warning":
                        Log.Warning(formattedMessage);
                        break; 
                    case "verbose":
                        Log.Verbose(formattedMessage);
                        break; 
                    case "information":
                        Log.Information(formattedMessage);
                        break; 
                    case "debug":
                        Log.Debug(formattedMessage);
                        break;
                }
            };
            
            // validate arguments.
            if (args.Length < 2)
            {
                Log.Fatal("At least two arguments expected.");
                return;
            }

            var inputFile = args[0];
            if (!File.Exists(inputFile))
            {
                Log.Fatal($"Input file {inputFile} not found!");
                return;
            }

            var outputPath = new FileInfo(args[1]).DirectoryName;
            if (!Directory.Exists(outputPath))
            {
                Log.Fatal($"Output path {outputPath} not found!");
                return;
            }
            
            // read the locations.
            var locations = CSV.CSVReader.Read(args[0]).Select(r => new Coordinate((float)r.Latitude, (float)r.Longitude)).ToArray();
            
            // build routerdb if needed.
            var routerDb = RouterDbBuilder.BuildRouterDb();
            
            // cut out a part of the routerdb.
            var box = locations.BuildBoundingBox().Value.Resize(0.01f);
            routerDb = routerDb.ExtractArea((l) => box.Overlaps(l.Latitude, l.Longitude));
            
            // create and configure the optimizer.
            var router = new Router(routerDb);
            var optimizer = router.Optimizer(new OptimizerConfiguration(modelMapperRegistry: new ModelMapperRegistry(
                (ByEdgeDirectedModelMapper.Name, ByEdgeDirectedModelMapper.TryMap))));
            
            // run the optimization.
            var route = optimizer.Optimize("car", locations, out _, 0, 0, turnPenalty: 60);
            if (route.IsError)
            {
                Log.Fatal("Calculating route failed {@errorMessage}", route.ErrorMessage);
                return;
            }
            File.WriteAllText(args[1] + ".geojson", route.Value.ToGeoJson());
            
            // set a description/name on stops.
            foreach (var stop in route.Value.Stops)
            {
                if (!stop.Attributes.TryGetValue("order", out var order) ||
                    !stop.Attributes.TryGetValue("index", out var index)) continue;
                stop.Attributes.AddOrReplace("Name",
                    $"{order}");
                stop.Attributes.AddOrReplace("Description", 
                    $"Stop {index} @ {order}");
            }
            route.Value.ShapeMeta = null;
            
            // convert to GPX.
            var features = route.Value.ToFeatureCollection();
            using (var stream = File.Open(args[1], FileMode.Create))
            {
                var writerSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, CloseOutput = true };
                using (var wr = XmlWriter.Create(stream, writerSettings))
                {
                    GpxWriter.Write(wr, null, new GpxMetadata("StreetScan"), features.Features, null);
                }
            }
        }
    }
}
