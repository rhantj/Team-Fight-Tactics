using UnityEngine;

struct RectZone
{
    public float minX, minY, maxX, maxY;
    public bool IsInside(Vector3 pos)
    {
        return pos.x >= minX && pos.x <= maxX &&
               pos.y >= minY && pos.y <= maxY;
    }
}