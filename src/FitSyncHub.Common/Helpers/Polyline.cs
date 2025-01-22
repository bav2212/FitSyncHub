using FitSyncHub.Common.Models;

namespace FitSyncHub.Functions.Helpers;

/// <summary>
/// 
/// <see cref="https://github.com/mapbox/polyline/blob/master/src/polyline.js" if you need methods: encode, fromGeoJSON, toGeoJSON  methods/>
/// </summary>
public static class Polyline
{
    public static List<Coordinate> Decode(string str, int? precision = default)
    {
        var index = 0;
        var lat = 0.0;
        var lng = 0.0;
        List<Coordinate> coordinates = [];
        double latitudeChange;
        double longitudeChange;
        var factor = Math.Pow(10, precision ?? 5);

        while (index < str.Length)
        {
            var shift = 1;
            var result = 0;

            int? byteValue;
            do
            {
                byteValue = str[index++] - 63;
                result += (byteValue.Value & 0x1f) * shift;
                shift *= 32;
            } while (byteValue >= 0x20);

            latitudeChange = (result & 1) == 1 ? (double)(-result - 1) / 2 : result / 2;

            shift = 1;
            result = 0;

            do
            {
                byteValue = str[index++] - 63;
                result += (byteValue.Value & 0x1f) * shift;
                shift *= 32;
            } while (byteValue >= 0x20);

            longitudeChange = (result & 1) == 1 ? (double)(-result - 1) / 2 : result / 2;

            lat += latitudeChange;
            lng += longitudeChange;

            coordinates.Add(new Coordinate(lat / factor, lng / factor));
        }

        return coordinates;
    }
}
