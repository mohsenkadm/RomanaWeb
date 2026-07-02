using System.Globalization;

namespace RomanaWeb.Helper.Interface
{
    public interface IDistanceService
    {
        double HaversineKm(double lat1, double lng1, double lat2, double lng2);
        bool IsValidCoord(double lat, double lng);
        bool TryParseCoord(string? lat, string? lng, out double latD, out double lngD);
        double RoundKm(double km);
    }
}
