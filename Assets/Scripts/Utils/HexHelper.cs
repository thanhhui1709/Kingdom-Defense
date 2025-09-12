using UnityEngine;

public class HexHelper
{
    public static float hexRadius = 1.1545f; 


    public static Vector2Int WorldToHex(Vector3 position)
    {
        float q = (Mathf.Sqrt(3f) / 3f * position.x - 1f / 3f * position.z) / hexRadius;
        float r = (2f / 3f * position.z) / hexRadius;
        return CubeRound(q, r);
    }

    private static Vector2Int CubeRound(float q, float r)
    {
        float x = q;
        float z = r;
        float y = -x - z;

        int rx = Mathf.RoundToInt(x);
        int ry = Mathf.RoundToInt(y);
        int rz = Mathf.RoundToInt(z);

        float dx = Mathf.Abs(rx - x);
        float dy = Mathf.Abs(ry - y);
        float dz = Mathf.Abs(rz - z);

        if (dx > dy && dx > dz) rx = -ry - rz;
        else if (dy > dz) ry = -rx - rz;
        else rz = -rx - ry;

        return new Vector2Int(rx, rz); // (q,r)
    }

    public static Vector3 HexToWorld(Vector2Int hex)
    {
        float x = hexRadius * Mathf.Sqrt(3f) * (hex.x + hex.y / 2f);
        float z = hexRadius * 3f / 2f * hex.y;
        return new Vector3(x, 0, z);
    }
}
