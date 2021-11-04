using System.Reflection;
using Itinero;
using Itinero.Profiles;
using Serilog;

namespace StreetScan.Planner.Profiles
{
    internal static class Vehicles
    {
        public static Vehicle Car()
        {
            return DynamicVehicle.LoadFromEmbeddedResource(typeof(Program).Assembly, 
                "StreetScan.Planner.Profiles.car.lua");
        }
    }
}