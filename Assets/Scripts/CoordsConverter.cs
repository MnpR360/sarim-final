
using UnityEngine; 

public static class CoordsConverter
{
    public static Vector2 originXZ = new Vector2(0f, 0f); // Origin point in XZ coordinates
    public static Vector2 originLonLat = new Vector2(25.0513380f, 55.3779827f); // Corresponding origin point in LonLat

    // Earth's approximate radius in meters
    private const float EarthRadius = 6371000f;

    public static Vector2 ConvertXZToLonLat(Vector2 xzCoordinates)
    {
        // Calculate the longitude and latitude using a linear transformation with curvature
        float lon = originLonLat.x + (xzCoordinates.x / EarthRadius) * (180f / Mathf.PI);
        float lat = originLonLat.y + (xzCoordinates.y / EarthRadius) * (180f / Mathf.PI) / Mathf.Cos(originLonLat.y * Mathf.Deg2Rad);

        return new Vector2(lon, lat);
    }

    public static Vector2 ConvertLonLatToXZ(Vector2 lonLatCoordinates)
    {
        // Calculate the X and Z coordinates using the inverse of the XZ to LonLat formula
        float deltaX = (lonLatCoordinates.x - originLonLat.x) * (Mathf.PI / 180f) * EarthRadius;
        float deltaZ = (lonLatCoordinates.y - originLonLat.y) * (Mathf.PI / 180f) * EarthRadius * Mathf.Cos(originLonLat.y * Mathf.Deg2Rad);

        return new Vector2(originXZ.x + deltaX, originXZ.y + deltaZ );
    }
}