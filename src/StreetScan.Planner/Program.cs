using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

[assembly: InternalsVisibleTo("StreetScan.Planner.Tests.Functional")]
namespace StreetScan.Planner
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            
#if DEBUG
            if (args == null || args.Length < 1)
            {
                args = new[] {"test"};
                args = new[] {"test.csv", "--turn", "120", "--profile", "car"};
            }
#endif

            if (args == null || args.Length < 1)
            { // show help.
                Log.Fatal("Could not parse arguments");
                ShowHelp();
                return;
            }
            if (args[0] == "help")
            {
                ShowHelp();
                return;
            }
            if (args[0] == "test")
            {
                args = new[]
                {
                    "test.csv",
                    "test.gpx"
                };
                Log.Information($"Running test using: {args[0]}");
            }
            
            // parse arguments.
            var inputFile = new FileInfo(args[0]).FullName;
            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile) + ".gpx");
            var i = 1;
            var turnPenalty = 60;
            var profileName = "car.shortest";
            while (i < args.Length)
            {
                if (i == 1 &&
                    !args[i].StartsWith("--"))
                {
                    // we assume this is the output file.
                    outputFile = new FileInfo(args[i]).FullName;
                    i++;
                    continue;
                }
                
                // this should be one of the options.
                if (args[i] == "--turn")
                {
                    i++;

                    if (i < args.Length && 
                        int.TryParse(args[i], out turnPenalty))
                    {
                        i++;
                        Log.Information("Using custom turn penalty: {TurnPenalty}", turnPenalty);
                    }
                    else
                    {
                        ArgumentParsingFailed("Could not find or parse turn penalty value");
                        return;
                    }
                } 
                else if (args[i] == "--profile")
                {
                    i++;

                    if (i < args.Length)
                    {
                        profileName = args[i];
                        i++;
                        Log.Information("Using custom profile: {ProfileName}", profileName);
                    }
                    else
                    {
                        ArgumentParsingFailed("Could not find or parse profile");
                        return;
                    }
                }
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
            
            if (!File.Exists(inputFile))
            {
                Log.Fatal("Input file {InputFile} not found!", inputFile);
                return;
            }

            var outputPath = new FileInfo(outputFile).DirectoryName;
            if (!Directory.Exists(outputPath))
            {
                Log.Fatal("Output path {OutputPath} not found!", outputPath);
                return;
            }
            
            // read the locations.
            Coordinate[] locations;
            if (inputFile.ToLowerInvariant().EndsWith(".geojson"))
            {
                locations = GeoJson.GeoJsonReader1.Read(inputFile).ToArray();
            }
            else
            {
                locations = CSV.CSVReader.Read(inputFile).Select(r => new Coordinate((float)r.Latitude, (float)r.Longitude)).ToArray();
            }
            
            // extract OSM data so it can later be edited.
            var box = locations.BuildBoundingBox().Value.Resize(0.01f);
            var osmData = RouterDbBuilder.GetOsmData(Path.GetFileNameWithoutExtension(inputFile), outputPath, box);
            
            // build router db if needed.
            var routerDb = RouterDbBuilder.BuildRouterDb(osmData, profileName);
            if (routerDb == null) return;
            
            // create and configure the optimizer.
            var router = new Router(routerDb);
            var optimizer = router.Optimizer(new OptimizerConfiguration(modelMapperRegistry: new ModelMapperRegistry(
                new ByEdgeDirectedModelMapper(1000))));
            
            // run the optimization.
            var profile = routerDb.GetSupportedProfile(profileName);
            var route = optimizer.Optimize(profile.FullName, locations, out _, 0, 0, turnPenalty: turnPenalty);
            if (route.IsError)
            {
                Log.Fatal("Calculating route failed {ErrorMessage}", route.ErrorMessage);
                return;
            }
            File.WriteAllText(outputFile + ".geojson", route.Value.ToGeoJson());
            
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
            using var stream = File.Open(outputFile, FileMode.Create);
            var writerSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, CloseOutput = true };
            using var wr = XmlWriter.Create(stream, writerSettings);
            GpxWriter.Write(wr, null, new GpxMetadata("StreetScan"), features.Features, null);
        }

        private static void ShowHelp()
        {
            Log.Information("Usage: arg1 arg2");
            Log.Information("- arg1: input.csv");
            Log.Information("- arg2: (optional) output.gpx");
            Log.Information($"Example arguments: {Path.Combine("path", "to", "input.csv")} {Path.Combine("path", "to", "output.gpx")}");
        }

        private static void ArgumentParsingFailed(string message)
        {
            Log.Fatal(message);
            ShowHelp();
        }
    }
}
