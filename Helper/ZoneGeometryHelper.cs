using Newtonsoft.Json.Linq;

namespace RomanaWeb.Helper
{
    public static class ZoneGeometryHelper
    {
        public static bool TryParseRing(string geoJson, out List<(double Lng, double Lat)> ring)
        {
            ring = new List<(double Lng, double Lat)>();
            if (string.IsNullOrWhiteSpace(geoJson)) return false;
            try
            {
                var root = JToken.Parse(geoJson.Trim());
                var ringArr = ExtractOuterRingArray(root);
                if (ringArr == null || ringArr.Count < 3) return false;

                foreach (var p in ringArr)
                {
                    if (p is not JArray pt || pt.Count < 2) return false;
                    ring.Add((pt[0]!.Value<double>(), pt[1]!.Value<double>()));
                }
                return ring.Count >= 3;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Accept Feature / FeatureCollection / Polygon / MultiPolygon and store canonical Polygon JSON.</summary>
        public static bool TryNormalizeToPolygonGeoJson(string geoJson, out string normalized)
        {
            normalized = "";
            if (!TryParseRing(geoJson, out var ring)) return false;

            var coords = new JArray();
            foreach (var p in ring)
                coords.Add(new JArray(p.Lng, p.Lat));

            if (ring.Count >= 3)
            {
                var first = ring[0];
                var last = ring[^1];
                if (Math.Abs(first.Lng - last.Lng) > 1e-6 || Math.Abs(first.Lat - last.Lat) > 1e-6)
                    coords.Add(new JArray(first.Lng, first.Lat));
            }

            normalized = new JObject
            {
                ["type"] = "Polygon",
                ["coordinates"] = new JArray { coords }
            }.ToString(Newtonsoft.Json.Formatting.None);
            return true;
        }

        private static JArray? ExtractOuterRingArray(JToken? root)
        {
            if (root == null) return null;

            if (root is JObject obj)
            {
                var type = obj["type"]?.Value<string>();

                if (string.Equals(type, "Feature", StringComparison.OrdinalIgnoreCase))
                    return ExtractOuterRingArray(obj["geometry"]);

                if (string.Equals(type, "FeatureCollection", StringComparison.OrdinalIgnoreCase))
                {
                    if (obj["features"] is JArray features && features.Count > 0)
                        return ExtractOuterRingArray(features[0]);
                    return null;
                }

                if (string.Equals(type, "Polygon", StringComparison.OrdinalIgnoreCase))
                    return (obj["coordinates"] as JArray)?[0] as JArray;

                if (string.Equals(type, "MultiPolygon", StringComparison.OrdinalIgnoreCase))
                {
                    var multi = obj["coordinates"] as JArray;
                    var poly = multi?[0] as JArray;
                    return poly?[0] as JArray;
                }

                if (obj["coordinates"] is JArray legacyCoords)
                {
                    if (legacyCoords.Count > 0 && legacyCoords[0] is JArray first && first[0] is JArray)
                        return first;
                    if (legacyCoords.Count > 0 && legacyCoords[0] is JArray maybeRing && maybeRing.Count >= 2 &&
                        (maybeRing[0].Type == JTokenType.Float || maybeRing[0].Type == JTokenType.Integer))
                        return maybeRing;
                }
            }

            return null;
        }

        public static bool PointInPolygon(IReadOnlyList<(double Lng, double Lat)> polygon, double lng, double lat)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];
                bool intersect = ((pi.Lat > lat) != (pj.Lat > lat)) &&
                                 (lng < (pj.Lng - pi.Lng) * (lat - pi.Lat) /
                                  ((pj.Lat - pi.Lat) == 0 ? 1e-12 : (pj.Lat - pi.Lat)) + pi.Lng);
                if (intersect) inside = !inside;
            }
            return inside;
        }

        /// <summary>Closest point on polygon boundary to an external point (lng/lat).</summary>
        public static (double Lng, double Lat) ClosestPointOnBoundary(
            IReadOnlyList<(double Lng, double Lat)> polygon, double lng, double lat)
        {
            double bestLng = polygon[0].Lng, bestLat = polygon[0].Lat;
            double bestDist = double.MaxValue;

            for (int i = 0; i < polygon.Count; i++)
            {
                var a = polygon[i];
                var b = polygon[(i + 1) % polygon.Count];
                var (clng, clat) = ClosestPointOnSegment(a.Lng, a.Lat, b.Lng, b.Lat, lng, lat);
                double d = HaversineKm(clat, clng, lat, lng);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestLng = clng;
                    bestLat = clat;
                }
            }
            return (bestLng, bestLat);
        }

        private static (double Lng, double Lat) ClosestPointOnSegment(
            double x1, double y1, double x2, double y2, double px, double py)
        {
            double dx = x2 - x1, dy = y2 - y1;
            if (Math.Abs(dx) < 1e-12 && Math.Abs(dy) < 1e-12)
                return (x1, y1);

            double t = ((px - x1) * dx + (py - y1) * dy) / (dx * dx + dy * dy);
            t = Math.Clamp(t, 0, 1);
            return (x1 + t * dx, y1 + t * dy);
        }

        public static double HaversineKm(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371d;
            double dLat = (lat2 - lat1) * Math.PI / 180d;
            double dLng = (lng2 - lng1) * Math.PI / 180d;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180d) * Math.Cos(lat2 * Math.PI / 180d) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        public static bool IsPolygonClosed(IReadOnlyList<(double Lng, double Lat)> ring)
        {
            if (ring.Count < 4) return false;
            var first = ring[0];
            var last = ring[^1];
            return Math.Abs(first.Lng - last.Lng) < 1e-6 && Math.Abs(first.Lat - last.Lat) < 1e-6;
        }
    }
}
