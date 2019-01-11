using System;
using System.IO;
using Itinero;
using Itinero.Optimization;
using Itinero.Optimization.Models.Mapping;
using Itinero.Optimization.Models.Mapping.Directed.Simplified;
using Serilog;

namespace StreetScan.Planner
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
#if DEBUG
            if (args == null || args.Length == 0)
            {
                args = new[]
                {
                    "/home/xivk/work/data/OSM/brussels-2018-09-24.osm.pbf",
                    //"/home/xivk/work/data/OSM/brussels.osm.pbf",
                    "/home/xivk/work/via/data/osm-service/"
                };
            }
#endif
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
            
//            // validate arguments.
//            if (args.Length < 2)
//            {
//                Log.Fatal("At least two arguments expected.");
//                return;
//            }
//
//            var inputFile = args[0];
//            if (!File.Exists(inputFile))
//            {
//                Log.Fatal($"Input file {inputFile} not found!");
//                return;
//            }
//
//            var outputPath = args[1];
//            if (!Directory.Exists(outputPath))
//            {
//                Log.Fatal($"Output path {outputPath} not found!");
//                return;
//            }

            // download data if needed.
            Download.DownloadAll();
            
            // build routerdb if needed.
            var routerDb = RouterDbBuilder.BuildRouterDb();
            
            // create and configure the optimizer.
            var router = new Router(routerDb);
            var optimizer = router.Optimizer(new OptimizerConfiguration(modelMapperRegistry: new ModelMapperRegistry(
                (ByEdgeDirectedModelMapper.Name, ByEdgeDirectedModelMapper.TryMap))));
            
            // run the optimization.
            
        }
    }
}
