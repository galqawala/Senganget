using UnityEngine;

public static class BoundsExtension
{
    public static Bounds GrowBounds( this Bounds a, Bounds b )
    {
        // https://forum.unity.com/threads/finding-center-of-a-group-of-objects.11607/
        Vector3 min = Vector3.Min( a.min, b.min );
        Vector3 max = Vector3.Max( a.max, b.max );
        return new Bounds( (max + min)*0.5f, max - min );
    }
}
