using System.Collections.Generic;
using Itinero.LocalGeo;

namespace StreetScan.Planner
{
    internal static class GeoExtensions
    {
        public static Box? BuildBoundingBox(this IEnumerable<Coordinate> coordinates)
        {
            using (var enumerator = coordinates.GetEnumerator())
            {
                Box? box = null;
                while (enumerator.MoveNext())
                {
                    if (box == null)
                    {
                        box = new Box(enumerator.Current, enumerator.Current);
                    }
                    else
                    {
                        box = box.Value.ExpandWith(enumerator.Current.Latitude, enumerator.Current.Longitude);
                    }
                }

                return box;
            }
        }
    }
}