using RomanaWeb.Helper;

namespace RomanaWeb.Tests;

public class ZoneGeometryHelperTests
{
    private const string Polygon =
        "{\"type\":\"Polygon\",\"coordinates\":[[[47.75,30.48],[47.82,30.48],[47.82,30.52],[47.75,30.52],[47.75,30.48]]]}";

    private const string Feature =
        "{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[47.75,30.48],[47.82,30.48],[47.82,30.52],[47.75,30.52],[47.75,30.48]]]}}";

    private const string FeatureCollection =
        "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"properties\":{},\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[47.75,30.48],[47.82,30.48],[47.82,30.52],[47.75,30.52],[47.75,30.48]]]}}]}";

    private const string MultiPolygon =
        "{\"type\":\"MultiPolygon\",\"coordinates\":[[[[47.75,30.48],[47.82,30.48],[47.82,30.52],[47.75,30.52],[47.75,30.48]]]]}";

    [Theory]
    [InlineData(Polygon)]
    [InlineData(Feature)]
    [InlineData(FeatureCollection)]
    [InlineData(MultiPolygon)]
    public void TryParseRing_accepts_common_geojson_formats(string geoJson)
    {
        Assert.True(ZoneGeometryHelper.TryParseRing(geoJson, out var ring));
        Assert.True(ring.Count >= 3);
    }

    [Theory]
    [InlineData(Polygon)]
    [InlineData(Feature)]
    [InlineData(FeatureCollection)]
    [InlineData(MultiPolygon)]
    public void TryNormalizeToPolygonGeoJson_outputs_polygon(string geoJson)
    {
        Assert.True(ZoneGeometryHelper.TryNormalizeToPolygonGeoJson(geoJson, out var normalized));
        Assert.Contains("\"type\":\"Polygon\"", normalized);
        Assert.True(ZoneGeometryHelper.TryParseRing(normalized, out _));
    }

    [Fact]
    public void TryParseRing_accepts_integer_coordinates()
    {
        const string geo = "{\"type\":\"Polygon\",\"coordinates\":[[[47,30],[48,30],[48,31],[47,31],[47,30]]]}";
        Assert.True(ZoneGeometryHelper.TryParseRing(geo, out var ring));
        Assert.Equal(47d, ring[0].Lng);
        Assert.Equal(30d, ring[0].Lat);
    }
}
