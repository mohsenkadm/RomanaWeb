using System.Globalization;
using RomanaWeb.Helper.Interface;

namespace RomanaWeb.Helper.Repository
{
    public class DistanceService : IDistanceService, IRegisterScopped
    {
        private const double MinCoord = 0.0001d;
        private const double EarthRadiusKm = 6371d;

        public double HaversineKm(double lat1, double lng1, double lat2, double lng2)
        {
            double dLat = (lat2 - lat1) * Math.PI / 180d;
            double dLng = (lng2 - lng1) * Math.PI / 180d;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180d) * Math.Cos(lat2 * Math.PI / 180d) *
                       Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        public bool IsValidCoord(double lat, double lng) =>
            Math.Abs(lat) > MinCoord && Math.Abs(lng) > MinCoord &&
            lat is >= -90 and <= 90 && lng is >= -180 and <= 180;

        public bool TryParseCoord(string? lat, string? lng, out double latD, out double lngD)
        {
            latD = lngD = 0;
            if (string.IsNullOrWhiteSpace(lat) || string.IsNullOrWhiteSpace(lng))
                return false;

            if (!double.TryParse(lat.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out latD))
                return false;
            if (!double.TryParse(lng.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out lngD))
                return false;

            return IsValidCoord(latD, lngD);
        }

        public double RoundKm(double km) => Math.Round(km, 2, MidpointRounding.AwayFromZero);
    }
}
