using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Helper.Repository
{
    public class RoutingService : IRoutingService, IRegisterScopped
    {
        private const string OsrmBase = "https://router.project-osrm.org/route/v1/driving/";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistanceService _distance;
        private readonly IMemoryCache _cache;

        public RoutingService(IHttpClientFactory httpClientFactory, IDistanceService distance, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _distance = distance;
            _cache = cache;
        }

        public async Task<RoutingResult> GetRouteDistanceKmAsync(double fromLat, double fromLng, double toLat, double toLng)
        {
            return await GetRouteInternalAsync(fromLat, fromLng, toLat, toLng);
        }

        private async Task<RoutingResult> GetRouteInternalAsync(double fromLat, double fromLng, double toLat, double toLng)
        {
            string cacheKey = $"route_{fromLat:F4}_{fromLng:F4}_{toLat:F4}_{toLng:F4}";
            if (_cache.TryGetValue(cacheKey, out RoutingResult? cached) && cached != null)
                return cached;

            var result = await TryOsrmAsync(fromLat, fromLng, toLat, toLng)
                         ?? FallbackHaversine(fromLat, fromLng, toLat, toLng);

            _cache.Set(cacheKey, result, CacheTtl);
            return result;
        }

        private async Task<RoutingResult?> TryOsrmAsync(double fromLat, double fromLng, double toLat, double toLng)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(8);
                string url = $"{OsrmBase}{fromLng.ToString(CultureInfo.InvariantCulture)},{fromLat.ToString(CultureInfo.InvariantCulture)};" +
                             $"{toLng.ToString(CultureInfo.InvariantCulture)},{toLat.ToString(CultureInfo.InvariantCulture)}" +
                             "?overview=full&geometries=geojson";

                using var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                var routes = doc.RootElement.GetProperty("routes");
                if (routes.GetArrayLength() == 0) return null;

                var route = routes[0];
                double meters = route.GetProperty("distance").GetDouble();
                var path = ParseGeoJsonLine(route.GetProperty("geometry"));

                return new RoutingResult
                {
                    DistanceKm = _distance.RoundKm(meters / 1000d),
                    Source = "osrm",
                    Path = path
                };
            }
            catch
            {
                return null;
            }
        }

        private static List<RoutePoint>? ParseGeoJsonLine(JsonElement geometry)
        {
            if (geometry.ValueKind != JsonValueKind.Object) return null;
            if (!geometry.TryGetProperty("coordinates", out var coords) || coords.ValueKind != JsonValueKind.Array)
                return null;

            var path = new List<RoutePoint>();
            foreach (var pt in coords.EnumerateArray())
            {
                if (pt.ValueKind != JsonValueKind.Array || pt.GetArrayLength() < 2) continue;
                path.Add(new RoutePoint
                {
                    Lng = pt[0].GetDouble(),
                    Lat = pt[1].GetDouble()
                });
            }
            return path.Count >= 2 ? path : null;
        }

        private RoutingResult FallbackHaversine(double fromLat, double fromLng, double toLat, double toLng)
        {
            double km = _distance.HaversineKm(fromLat, fromLng, toLat, toLng) * 1.3;
            return new RoutingResult
            {
                DistanceKm = _distance.RoundKm(km),
                Source = "haversine_fallback",
                Path = new List<RoutePoint>
                {
                    new() { Lat = fromLat, Lng = fromLng },
                    new() { Lat = toLat, Lng = toLng }
                }
            };
        }
    }
}
