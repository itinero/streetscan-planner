using System;
using System.IO;

namespace StreetScan.Planner.Tests.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            StreetScan.Planner.Program.Main(new [] {
                Path.Combine("data", "CrabTielt.geojson") });
            StreetScan.Planner.Program.Main(new [] {
                Path.Combine("data", "CrabWingene.geojson") });
        }
    }
}
